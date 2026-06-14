import { Redirect, router } from 'expo-router';
import { Alert, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { currentMonthlyRecapPeriod, useGenerateMonthlyRecap, useMonthlyRecap } from '@/hooks/use-recaps';
import { ApiError } from '@/lib/api/api-error';
import { recapItemLabel, type MonthlyRecapResponse, type RecapItemResponse } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function RecapsRoute() {
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const period = currentMonthlyRecapPeriod();
  const monthlyRecap = useMonthlyRecap(primaryFamily?.familyId, period.year, period.month);
  const generateRecap = useGenerateMonthlyRecap(primaryFamily?.familyId ?? 'missing-family', period.year, period.month);
  const missingRecap = monthlyRecap.error instanceof ApiError && monthlyRecap.error.status === 404;

  if (!session) {
    return <Redirect href="/auth/sign-in" />;
  }

  if (currentUser.isLoading) {
    return <LoadingState />;
  }

  if (!primaryFamily) {
    return (
      <Screen>
        <EmptyState
          title="Start with your family space"
          message="Create a family workspace before opening monthly recaps."
          actionLabel="Set up family"
          onAction={() => router.push('/family/setup')}
        />
      </Screen>
    );
  }

  async function handleGenerateRecap() {
    try {
      await generateRecap.mutateAsync();
    } catch (error) {
      Alert.alert('Recap could not be generated', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Monthly Recap</Text>
        <Text className="text-peacenest-muted">
          {primaryFamily.familyName} - {formatPeriodLabel(period.year, period.month)}
        </Text>
      </View>

      <Card className="gap-3 bg-peacenest-blush">
        <View className="flex-row flex-wrap items-start justify-between gap-3">
          <View className="flex-1 gap-1">
            <Text variant="section">A gentle family reflection</Text>
            <Text variant="caption">
              Generate this month when your family is ready to look back at wins, notes, votes, and plans still
              growing.
            </Text>
          </View>
          <Badge label="Monthly" tone="gold" />
        </View>
        <Button
          disabled={generateRecap.isPending}
          label={generateRecap.isPending ? 'Generating' : monthlyRecap.data ? 'Refresh recap' : 'Generate recap'}
          onPress={handleGenerateRecap}
        />
      </Card>

      {monthlyRecap.isLoading ? <LoadingState title="Opening this month's reflection" /> : null}

      {monthlyRecap.isError && !missingRecap ? (
        <ErrorState
          title="Recap could not open"
          message={monthlyRecap.error.message}
          actionLabel="Try again"
          onAction={() => monthlyRecap.refetch()}
        />
      ) : null}

      {missingRecap ? (
        <EmptyState
          title="Your first recap is growing"
          message="Complete plans, add milestones, share votes, and PeaceNest can summarize the month when you ask."
          actionLabel="View plans"
          onAction={() => router.push('/tabs/wants-needs')}
        />
      ) : null}

      {monthlyRecap.data ? <MonthlyRecapCard recap={monthlyRecap.data.recap} /> : null}
    </Screen>
  );
}

function MonthlyRecapCard({ recap }: { recap: MonthlyRecapResponse }) {
  const peaceWins = recap.items.filter((item) => item.itemType === 'completed_plan');
  const stillGrowing = recap.items.filter((item) => item.itemType === 'still_growing');

  return (
    <View className="gap-4">
      <Card className="gap-3">
        <View className="flex-row flex-wrap items-start justify-between gap-3">
          <View className="flex-1 gap-1">
            <Text variant="section">{recap.title}</Text>
            <Text variant="caption">
              {formatDate(recap.periodStart)} to {formatDate(recap.periodEnd)}
            </Text>
          </View>
          <Badge label={recap.publishedAt ? 'Ready' : 'Draft'} tone="sage" />
        </View>
        {recap.summary ? <Text>{recap.summary}</Text> : null}
      </Card>

      <View className="flex-row flex-wrap gap-3">
        <StatTile label="Peace wins" value={recap.stats.completedPlans} tone="gold" />
        <StatTile label="Milestones" value={recap.stats.completedMilestones} tone="sage" />
        <StatTile label="Notes shared" value={recap.stats.notesAdded} tone="blush" />
        <StatTile label="Votes shared" value={recap.stats.votesCast} tone="blush" />
        <StatTile label="New plans" value={recap.stats.newPlans} tone="sage" />
        <StatTile label="Still growing" value={recap.stats.delayedPlans} tone="blush" />
      </View>

      <RecapSection
        emptyMessage="Completed plans will become peace wins here."
        items={peaceWins}
        title="Peace Wins"
        tone="gold"
      />
      <RecapSection
        emptyMessage="Plans that need more time will appear here gently."
        items={stillGrowing}
        title="Still Growing"
        tone="sage"
      />
    </View>
  );
}

type StatTileProps = {
  label: string;
  value: number;
  tone: 'gold' | 'sage' | 'blush';
};

function StatTile({ label, tone, value }: StatTileProps) {
  const backgroundClass = tone === 'gold' ? 'bg-peacenest-goldLight' : tone === 'sage' ? 'bg-peacenest-sage/20' : 'bg-peacenest-blush';

  return (
    <View className={`min-w-36 flex-1 rounded-lg p-3 ${backgroundClass}`}>
      <Text className="text-2xl font-bold">{value}</Text>
      <Text variant="caption">{label}</Text>
    </View>
  );
}

type RecapSectionProps = {
  emptyMessage: string;
  items: RecapItemResponse[];
  title: string;
  tone: 'gold' | 'sage';
};

function RecapSection({ emptyMessage, items, title, tone }: RecapSectionProps) {
  return (
    <Card className="gap-3">
      <View className="flex-row items-center justify-between gap-3">
        <Text variant="section">{title}</Text>
        <Badge label={`${items.length}`} tone={tone} />
      </View>

      {items.length === 0 ? <Text variant="caption">{emptyMessage}</Text> : null}

      {items.map((item) => (
        <RecapItem key={item.id} item={item} />
      ))}
    </Card>
  );
}

function RecapItem({ item }: { item: RecapItemResponse }) {
  return (
    <View className="gap-1 rounded-lg bg-peacenest-blush p-3">
      <Badge label={recapItemLabel(item.itemType)} tone={item.itemType === 'completed_plan' ? 'gold' : 'sage'} />
      <Text className="font-semibold">{item.title}</Text>
      {item.description ? <Text variant="caption">{item.description}</Text> : null}
    </View>
  );
}

function formatPeriodLabel(year: number, month: number) {
  return new Intl.DateTimeFormat(undefined, {
    month: 'long',
    year: 'numeric',
  }).format(new Date(year, month - 1, 1));
}

function formatDate(value: string) {
  const date = new Date(`${value}T00:00:00`);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
  }).format(date);
}
