import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { PlanNotesSection } from '@/components/plan-notes-section';
import { useMilestone } from '@/hooks/use-milestones';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { milestoneProgress } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function MilestoneDetailRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const planId = Array.isArray(id) ? id[0] : id;
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const milestone = useMilestone(primaryFamily?.familyId, planId);

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
          message="Create a family workspace before opening family milestones."
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
          title="Milestone could not open"
          message="The milestone link is missing its identifier."
          actionLabel="Back to milestones"
          onAction={() => router.replace('/tabs/milestones')}
        />
      </Screen>
    );
  }

  if (milestone.isLoading) {
    return <LoadingState title="Opening milestone" />;
  }

  if (milestone.isError) {
    return (
      <Screen>
        <ErrorState
          title="Milestone could not open"
          message={milestone.error.message}
          actionLabel="Try again"
          onAction={() => milestone.refetch()}
        />
      </Screen>
    );
  }

  const plan = milestone.data?.milestone;

  if (!plan) {
    return (
      <Screen>
        <EmptyState
          title="Milestone not found"
          message="This family milestone may have moved or been archived."
          actionLabel="Back to milestones"
          onAction={() => router.replace('/tabs/milestones')}
        />
      </Screen>
    );
  }

  const progress = milestoneProgress(plan);

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">{plan.title}</Text>
        <Text className="text-peacenest-muted">{primaryFamily.familyName}</Text>
      </View>

      <Card className="gap-4">
        <View className="flex-row flex-wrap gap-2">
          <Badge label="Milestone" tone="gold" />
          {plan.targetDate ? <Badge label={`Target ${formatDate(plan.targetDate)}`} tone="muted" /> : null}
          {plan.milestoneType ? <Badge label={plan.milestoneType} tone="muted" /> : null}
          {plan.includeInRecap ? <Badge label="Recap ready" tone="sage" /> : null}
        </View>

        {plan.description ? <Text>{plan.description}</Text> : <Text variant="caption">No milestone note added yet.</Text>}

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
      </Card>

      {plan.steps.length > 0 ? (
        <Card className="gap-3">
          <Text variant="section">Checklist</Text>
          {plan.steps.map((step) => (
            <View key={step.id} className="rounded-lg bg-peacenest-blush p-3">
              <Text className="font-semibold">{step.title}</Text>
              {step.description ? <Text variant="caption">{step.description}</Text> : null}
              <Text variant="caption">{step.isCompleted ? 'Completed' : 'Still growing'}</Text>
            </View>
          ))}
        </Card>
      ) : null}

      <PlanNotesSection
        currentUserId={currentUser.data.user.id}
        currentUserRole={primaryFamily.role}
        familyId={primaryFamily.familyId}
        planId={plan.id}
      />

      <Button label="Back to Milestones" onPress={() => router.replace('/tabs/milestones')} variant="secondary" />
    </Screen>
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
