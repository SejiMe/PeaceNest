import { Redirect, router } from 'expo-router';
import { Pressable, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useMilestones } from '@/hooks/use-milestones';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { milestoneProgress, type MilestoneResponse } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function MilestonesRoute() {
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const milestones = useMilestones(primaryFamily?.familyId);

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
          message="Create a family workspace before adding shared milestones."
          actionLabel="Set up family"
          onAction={() => router.push('/family/setup')}
        />
      </Screen>
    );
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Family Milestones</Text>
        <Text className="text-peacenest-muted">{primaryFamily.familyName}</Text>
      </View>

      {milestones.isLoading ? <LoadingState title="Gathering family milestones" /> : null}

      {milestones.isError ? (
        <ErrorState
          title="Milestones could not open"
          message={milestones.error.message}
          actionLabel="Try again"
          onAction={() => milestones.refetch()}
        />
      ) : null}

      {!milestones.isLoading && (milestones.data?.milestones.length ?? 0) === 0 ? (
        <EmptyState
          title="Create your first family milestone"
          message="Milestones can be big or small, from graduation goals to Sunday dinners."
          actionLabel="Add milestone"
          onAction={() => router.push('/milestones/create')}
        />
      ) : null}

      <View className="gap-3">
        {milestones.data?.milestones.map((milestone) => (
          <MilestoneCard key={milestone.id} milestone={milestone} />
        ))}
      </View>

      <Button label="Add milestone" onPress={() => router.push('/milestones/create')} />
    </Screen>
  );
}

function MilestoneCard({ milestone }: { milestone: MilestoneResponse }) {
  const progress = milestoneProgress(milestone);
  const nextStep = milestone.steps.find((step) => !step.isCompleted);

  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => router.push({ pathname: '/milestones/[id]', params: { id: milestone.id } })}
    >
      <Card className="gap-3">
        <View className="flex-row items-start justify-between gap-3">
          <View className="flex-1 gap-1">
            <Text className="text-lg font-semibold">{milestone.title}</Text>
            {milestone.description ? <Text variant="caption">{milestone.description}</Text> : null}
          </View>
          <Badge label="Milestone" tone="gold" />
        </View>

        <View className="flex-row flex-wrap gap-2">
          {milestone.targetDate ? <Badge label={`Target ${formatDate(milestone.targetDate)}`} tone="muted" /> : null}
          {milestone.milestoneType ? <Badge label={milestone.milestoneType} tone="muted" /> : null}
          {milestone.includeInRecap ? <Badge label="Recap ready" tone="sage" /> : null}
        </View>

        <View className="gap-2">
          <View className="flex-row justify-between">
            <Text variant="caption">Checklist progress</Text>
            <Text variant="caption">
              {progress.total > 0 ? `${progress.completed}/${progress.total}` : `${progress.percent}%`}
            </Text>
          </View>
          <View className="h-2 overflow-hidden rounded-lg bg-peacenest-blush">
            <View className="h-full bg-peacenest-sage" style={{ width: `${progress.percent}%` }} />
          </View>
        </View>

        {nextStep ? (
          <View className="rounded-lg bg-peacenest-blush p-3">
            <Text variant="caption">Next gentle step</Text>
            <Text className="font-semibold">{nextStep.title}</Text>
          </View>
        ) : null}
      </Card>
    </Pressable>
  );
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
