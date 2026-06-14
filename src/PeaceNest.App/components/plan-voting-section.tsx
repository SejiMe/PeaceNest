import { useEffect, useMemo, useState } from 'react';
import { Alert, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useCastPlanVote, usePlanVotes } from '@/hooks/use-plan-votes';
import {
  canCastPlanVotes,
  type FamilyMemberRole as FamilyMemberRoleValue,
  VoteValue,
  type VoteValue as VoteValueValue,
  voteValueLabel,
} from '@/lib/api/contracts';
import { cn } from '@/lib/utils';

type PlanVotingSectionProps = {
  familyId: string;
  planId: string;
  currentUserId: string;
  currentUserRole: FamilyMemberRoleValue;
};

const voteOptions: { label: string; value: VoteValueValue }[] = [
  { label: 'Support', value: VoteValue.Support },
  { label: 'Neutral', value: VoteValue.Neutral },
  { label: 'Not now', value: VoteValue.NotNow },
];

const priorityPointOptions = [0, 1, 2, 3, 4, 5];

export function PlanVotingSection({
  currentUserId,
  currentUserRole,
  familyId,
  planId,
}: PlanVotingSectionProps) {
  const votes = usePlanVotes(familyId, planId);
  const castVote = useCastPlanVote(familyId, planId);
  const canVote = canCastPlanVotes(currentUserRole);
  const ownVote = useMemo(
    () => votes.data?.voteSummary.votes.find((vote) => vote.userId === currentUserId),
    [currentUserId, votes.data?.voteSummary.votes],
  );
  const [voteValue, setVoteValue] = useState<VoteValueValue>(ownVote?.voteValue ?? VoteValue.Support);
  const [priorityPoints, setPriorityPoints] = useState(ownVote?.priorityPoints ?? 3);
  const [note, setNote] = useState(ownVote?.note ?? '');

  const summary = votes.data?.voteSummary;

  useEffect(() => {
    if (!ownVote) {
      return;
    }

    setVoteValue(ownVote.voteValue);
    setPriorityPoints(ownVote.priorityPoints);
    setNote(ownVote.note ?? '');
  }, [ownVote]);

  async function handleCastVote() {
    try {
      await castVote.mutateAsync({
        voteValue,
        priorityPoints,
        note: note.trim() ? note.trim() : null,
      });
    } catch (error) {
      Alert.alert('Vote could not be saved', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Card className="gap-4">
      <View className="gap-1">
        <Text variant="section">Family vote</Text>
        <Text variant="caption">A simple signal for how this plan feels to the family.</Text>
      </View>

      {votes.isLoading ? <LoadingState title="Gathering votes" /> : null}

      {votes.isError ? (
        <View className="gap-3 rounded-lg border border-peacenest-danger bg-peacenest-surface p-3">
          <Text className="font-semibold">Votes could not open</Text>
          <Text variant="caption">{votes.error.message}</Text>
          <Button label="Try again" onPress={() => votes.refetch()} variant="secondary" />
        </View>
      ) : null}

      {summary ? (
        <View className="gap-3">
          <View className="flex-row flex-wrap gap-2">
            <Badge label={`${summary.totalVotes} votes`} tone="muted" />
            <Badge label={`${summary.supportCount} support`} tone="sage" />
            <Badge label={`${summary.neutralCount} neutral`} tone="gold" />
            <Badge label={`${summary.notNowCount} not now`} tone="want" />
          </View>

          <Text variant="caption">
            {summary.totalPriorityPoints > 0
              ? `${summary.totalPriorityPoints} priority points shared so far.`
              : 'No priority points shared yet.'}
          </Text>
        </View>
      ) : null}

      {canVote ? (
        <View className="gap-4">
          <View className="gap-2">
            <Text className="font-semibold">Your signal</Text>
            <View className="flex-row flex-wrap gap-2">
              {voteOptions.map((option) => (
                <Button
                  key={option.value}
                  className={cn('min-h-10 px-4', voteValue !== option.value && 'bg-peacenest-surface')}
                  label={option.label}
                  onPress={() => setVoteValue(option.value)}
                  variant={voteValue === option.value ? 'primary' : 'secondary'}
                />
              ))}
            </View>
          </View>

          <View className="gap-2">
            <Text className="font-semibold">Priority points</Text>
            <View className="flex-row flex-wrap gap-2">
              {priorityPointOptions.map((points) => (
                <Button
                  key={points}
                  className={cn('min-h-10 min-w-10 px-3', priorityPoints !== points && 'bg-peacenest-surface')}
                  label={String(points)}
                  onPress={() => setPriorityPoints(points)}
                  variant={priorityPoints === points ? 'primary' : 'secondary'}
                />
              ))}
            </View>
          </View>

          <Input
            className="min-h-20 py-3"
            maxLength={1000}
            multiline
            onChangeText={setNote}
            placeholder="Optional note for your vote."
            textAlignVertical="top"
            value={note}
          />

          <Button
            disabled={castVote.isPending}
            label={castVote.isPending ? 'Saving vote' : ownVote ? 'Update vote' : 'Save vote'}
            onPress={handleCastVote}
          />
        </View>
      ) : (
        <View className="rounded-lg bg-peacenest-blush p-3">
          <Text className="font-semibold">Voting is read-only</Text>
          <Text variant="caption">You can follow the family signal, but this role cannot vote.</Text>
        </View>
      )}

      {(summary?.votes.length ?? 0) > 0 ? (
        <View className="gap-3">
          {summary?.votes.map((vote) => (
            <View key={vote.id} className="gap-1 rounded-lg bg-peacenest-blush p-3">
              <View className="flex-row flex-wrap items-center justify-between gap-2">
                <Text className="font-semibold">{vote.userDisplayName}</Text>
                <Badge label={voteValueLabel(vote.voteValue)} tone={vote.voteValue === VoteValue.Support ? 'sage' : 'muted'} />
              </View>
              <Text variant="caption">{vote.priorityPoints} priority points</Text>
              {vote.note ? <Text>{vote.note}</Text> : null}
            </View>
          ))}
        </View>
      ) : null}
    </Card>
  );
}
