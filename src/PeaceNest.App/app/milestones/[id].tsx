import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { PlanNotesSection } from '@/components/plan-notes-section';
import { PlanVotingSection } from '@/components/plan-voting-section';
import { useFamilyPlanActions } from '@/hooks/use-family-plan-actions';
import { useMilestone, useUpdateMilestoneStepCompletion } from '@/hooks/use-milestones';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import {
  milestoneProgress,
  planStatusLabel,
  PlanStatus,
  type MilestoneResponse,
  type MilestoneStepResponse,
} from '@/lib/api/contracts';
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
          <Badge label={planStatusLabel(plan.status)} tone={plan.status === PlanStatus.Completed ? 'sage' : 'muted'} />
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

      <MilestoneActions familyId={primaryFamily.familyId} milestone={plan} />

      {plan.steps.length > 0 ? (
        <MilestoneChecklist familyId={primaryFamily.familyId} milestone={plan} />
      ) : null}

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

      <Button label="Back to Milestones" onPress={() => router.replace('/tabs/milestones')} variant="secondary" />
    </Screen>
  );
}

function MilestoneActions({ familyId, milestone }: { familyId: string; milestone: MilestoneResponse }) {
  const { archivePlan, completePlan, updateProgress } = useFamilyPlanActions(familyId, milestone.id);
  const [progress, setProgress] = useState(String(milestone.progressPercent));
  const isArchived = milestone.status === PlanStatus.Archived;
  const isCompleted = milestone.status === PlanStatus.Completed;
  const isUpdating = archivePlan.isPending || completePlan.isPending || updateProgress.isPending;

  async function handleUpdateProgress() {
    const parsedProgress = Number(progress);

    if (Number.isNaN(parsedProgress)) {
      Alert.alert('Check the progress', 'Progress needs to be a number from 0 to 100.');
      return;
    }

    try {
      await updateProgress.mutateAsync({
        progressPercent: Math.max(0, Math.min(100, Math.round(parsedProgress))),
      });
      Alert.alert('Progress updated', 'This milestone progress is now up to date.');
    } catch (error) {
      Alert.alert('Progress could not update', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleComplete() {
    try {
      await completePlan.mutateAsync();
      Alert.alert('Milestone completed', 'This milestone is now part of your family wins.');
    } catch (error) {
      Alert.alert('Milestone could not be completed', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  function handleArchive() {
    Alert.alert('Archive this milestone?', 'It will leave active planning, but the family history stays preserved.', [
      { text: 'Keep active', style: 'cancel' },
      {
        text: 'Archive',
        style: 'destructive',
        onPress: async () => {
          try {
            await archivePlan.mutateAsync();
            router.replace('/tabs/milestones');
          } catch (error) {
            Alert.alert('Milestone could not be archived', error instanceof Error ? error.message : 'Please try again.');
          }
        },
      },
    ]);
  }

  return (
    <Card className="gap-4">
      <View className="gap-1">
        <Text variant="section">Milestone progress</Text>
        <Text variant="caption">Use this for a simple shared sense of movement. Checklist steps can update it too.</Text>
      </View>

      <View className="flex-row gap-3">
        <Input
          className="flex-1"
          editable={!isArchived}
          keyboardType="number-pad"
          maxLength={3}
          onChangeText={setProgress}
          value={progress}
        />
        <Button
          className="px-4"
          disabled={isArchived || isUpdating}
          label={updateProgress.isPending ? 'Saving' : 'Save'}
          onPress={handleUpdateProgress}
          variant="secondary"
        />
      </View>

      <View className="flex-row flex-wrap gap-3">
        {!isCompleted ? (
          <Button
            className="flex-1"
            disabled={isArchived || isUpdating}
            label={completePlan.isPending ? 'Completing' : 'Mark complete'}
            onPress={handleComplete}
          />
        ) : null}
        {!isArchived ? (
          <Button
            className="flex-1"
            disabled={isUpdating}
            label={archivePlan.isPending ? 'Archiving' : 'Archive'}
            onPress={handleArchive}
            variant="secondary"
          />
        ) : null}
      </View>
    </Card>
  );
}

function MilestoneChecklist({ familyId, milestone }: { familyId: string; milestone: MilestoneResponse }) {
  const updateStep = useUpdateMilestoneStepCompletion(familyId, milestone.id);

  async function handleStepChange(step: MilestoneStepResponse) {
    try {
      await updateStep.mutateAsync({
        stepId: step.id,
        isCompleted: !step.isCompleted,
      });
    } catch (error) {
      Alert.alert('Checklist could not update', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Card className="gap-3">
      <Text variant="section">Checklist</Text>
      {milestone.steps.map((step) => (
        <View key={step.id} className="gap-3 rounded-lg bg-peacenest-blush p-3">
          <View className="gap-1">
            <Text className="font-semibold">{step.title}</Text>
            {step.description ? <Text variant="caption">{step.description}</Text> : null}
            <Text variant="caption">{step.isCompleted ? 'Completed' : 'Still growing'}</Text>
          </View>
          <Button
            disabled={updateStep.isPending || milestone.status === PlanStatus.Archived}
            label={step.isCompleted ? 'Mark still growing' : 'Mark complete'}
            onPress={() => handleStepChange(step)}
            variant={step.isCompleted ? 'secondary' : 'primary'}
          />
        </View>
      ))}
    </Card>
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
