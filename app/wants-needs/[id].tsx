import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { PlanNotesSection } from '@/components/plan-notes-section';
import { PlanVotingSection } from '@/components/plan-voting-section';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { useWantOrNeed } from '@/hooks/use-wants-and-needs';
import { kindLabel, scoreLabel, WantNeedKind } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function WantNeedDetailRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const planId = Array.isArray(id) ? id[0] : id;
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const wantOrNeed = useWantOrNeed(primaryFamily?.familyId, planId);

  if (!session) {
    return <Redirect href="/auth/sign-in" />;
  }

  if (currentUser.isLoading) {
    return <LoadingState />;
  }

  if (!primaryFamily || !currentUser.data) {
    return (
      <Screen>
        <EmptyState
          title="Start with your family space"
          message="Create a family workspace before opening family plans."
          actionLabel="Set up family"
          onAction={() => router.replace('/family/setup')}
        />
      </Screen>
    );
  }

  if (!planId) {
    return (
      <Screen>
        <ErrorState
          title="Plan could not open"
          message="The plan link is missing its identifier."
          actionLabel="Back to plans"
          onAction={() => router.replace('/tabs/wants-needs')}
        />
      </Screen>
    );
  }

  if (wantOrNeed.isLoading) {
    return <LoadingState title="Opening plan" />;
  }

  if (wantOrNeed.isError) {
    return (
      <Screen>
        <ErrorState
          title="Plan could not open"
          message={wantOrNeed.error.message}
          actionLabel="Try again"
          onAction={() => wantOrNeed.refetch()}
        />
      </Screen>
    );
  }

  const plan = wantOrNeed.data?.wantOrNeed;

  if (!plan) {
    return (
      <Screen>
        <EmptyState
          title="Plan not found"
          message="This family plan may have moved or been archived."
          actionLabel="Back to plans"
          onAction={() => router.replace('/tabs/wants-needs')}
        />
      </Screen>
    );
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">{plan.title}</Text>
        <Text className="text-peacenest-muted">{primaryFamily.familyName}</Text>
      </View>

      <Card className="gap-4">
        <View className="flex-row flex-wrap gap-2">
          <Badge label={kindLabel(plan.kind)} tone={plan.kind === WantNeedKind.Need ? 'need' : 'want'} />
          <Badge label={scoreLabel(plan.urgencyLevel)} tone={plan.urgencyLevel === 2 ? 'gold' : 'muted'} />
          {plan.estimatedCostAmount ? (
            <Badge
              label={`${plan.estimatedCostCurrency ?? 'USD'} ${Number(plan.estimatedCostAmount).toLocaleString()}`}
              tone="muted"
            />
          ) : null}
        </View>

        {plan.description ? <Text>{plan.description}</Text> : <Text variant="caption">No note added yet.</Text>}

        <View className="gap-2">
          <View className="flex-row justify-between">
            <Text variant="caption">Progress</Text>
            <Text variant="caption">{plan.progressPercent}%</Text>
          </View>
          <View className="h-2 overflow-hidden rounded-lg bg-peacenest-blush">
            <View className="h-full bg-peacenest-sage" style={{ width: `${plan.progressPercent}%` }} />
          </View>
        </View>
      </Card>

      <PlanNotesSection
        currentUserId={currentUser.data.user.id}
        currentUserRole={primaryFamily.role}
        familyId={primaryFamily.familyId}
        planId={plan.id}
      />

      <PlanVotingSection
        currentUserId={currentUser.data.user.id}
        currentUserRole={primaryFamily.role}
        familyId={primaryFamily.familyId}
        planId={plan.id}
      />

      <Button label="Back to Wants & Needs" onPress={() => router.replace('/tabs/wants-needs')} variant="secondary" />
    </Screen>
  );
}
