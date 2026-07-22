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
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { useWantOrNeed } from '@/hooks/use-wants-and-needs';
import { canUpdateFamilyPlans, formatEstimatedCost, kindLabel, planStatusLabel, PlanStatus, scoreLabel, WantNeedKind, type WantOrNeedResponse } from '@/lib/api/contracts';
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
          <Badge label={planStatusLabel(plan.status)} tone={plan.status === PlanStatus.Completed ? 'sage' : 'muted'} />
          {plan.estimatedCostAmount ? (
            <Badge
              label={formatEstimatedCost(Number(plan.estimatedCostAmount), plan.estimatedCostCurrency)}
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

      {plan.status === PlanStatus.Active && canUpdateFamilyPlans(primaryFamily.role) ? (
        <Button
          label="Edit plan"
          onPress={() => router.push({ pathname: '/wants-needs/[id]/edit', params: { id: plan.id } })}
          variant="secondary"
        />
      ) : null}

      <WantOrNeedActions familyId={primaryFamily.familyId} plan={plan} />

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

function WantOrNeedActions({ familyId, plan }: { familyId: string; plan: WantOrNeedResponse }) {
  const { archivePlan, completePlan, updateProgress } = useFamilyPlanActions(familyId, plan.id);
  const [progress, setProgress] = useState(String(plan.progressPercent));
  const isArchived = plan.status === PlanStatus.Archived;
  const isCompleted = plan.status === PlanStatus.Completed;
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
      Alert.alert('Progress updated', 'Your family plan progress is now up to date.');
    } catch (error) {
      Alert.alert('Progress could not update', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleComplete() {
    try {
      await completePlan.mutateAsync();
      Alert.alert('Plan completed', 'This family plan is now part of your peace wins.');
    } catch (error) {
      Alert.alert('Plan could not be completed', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  function handleArchive() {
    Alert.alert('Archive this plan?', 'It will leave active planning, but the family history stays preserved.', [
      { text: 'Keep active', style: 'cancel' },
      {
        text: 'Archive',
        style: 'destructive',
        onPress: async () => {
          try {
            await archivePlan.mutateAsync();
            router.replace('/tabs/wants-needs');
          } catch (error) {
            Alert.alert('Plan could not be archived', error instanceof Error ? error.message : 'Please try again.');
          }
        },
      },
    ]);
  }

  return (
    <Card className="gap-4">
      <View className="gap-1">
        <Text variant="section">Plan progress</Text>
        <Text variant="caption">Keep this lightweight. No savings ledger, just where the family feels it stands.</Text>
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
