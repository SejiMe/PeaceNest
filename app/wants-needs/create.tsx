import { Redirect, router } from 'expo-router';
import { useState } from 'react';
import { Alert, Pressable, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { EmptyState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { useCreateWantOrNeed } from '@/hooks/use-wants-and-needs';
import { ScoreLevel, WantNeedKind, type ScoreLevel as ScoreLevelValue, type WantNeedKind as WantNeedKindValue } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';
import { cn } from '@/lib/utils';

const scoreOptions: Array<{ label: string; value: ScoreLevelValue }> = [
  { label: 'Someday', value: ScoreLevel.Low },
  { label: 'Soon', value: ScoreLevel.Medium },
  { label: 'Now', value: ScoreLevel.High },
];

export default function CreateWantNeedRoute() {
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();

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
          title="Create your family space first"
          message="Wants and needs belong inside a family workspace."
          actionLabel="Set up family"
          onAction={() => router.replace('/family/setup')}
        />
      </Screen>
    );
  }

  return <CreateWantNeedForm familyId={primaryFamily.familyId} familyName={primaryFamily.familyName} />;
}

function CreateWantNeedForm({ familyId, familyName }: { familyId: string; familyName: string }) {
  const createWantOrNeed = useCreateWantOrNeed(familyId);
  const [kind, setKind] = useState<WantNeedKindValue>(WantNeedKind.Need);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [estimatedCost, setEstimatedCost] = useState('');
  const [progress, setProgress] = useState('0');
  const [urgencyLevel, setUrgencyLevel] = useState<ScoreLevelValue>(ScoreLevel.Medium);
  const [importanceLevel, setImportanceLevel] = useState<ScoreLevelValue>(ScoreLevel.Medium);
  const [emotionalValueLevel, setEmotionalValueLevel] = useState<ScoreLevelValue>(ScoreLevel.Medium);

  async function handleCreate() {
    const parsedCost = estimatedCost.trim() ? Number(estimatedCost) : null;
    const parsedProgress = progress.trim() ? Number(progress) : 0;

    if (Number.isNaN(parsedCost) || Number.isNaN(parsedProgress)) {
      Alert.alert('Check the numbers', 'Estimated cost and progress need to be numbers.');
      return;
    }

    try {
      await createWantOrNeed.mutateAsync({
        kind,
        title,
        description: description.trim() ? description : null,
        priorityRank: null,
        progressPercent: Math.max(0, Math.min(100, Math.round(parsedProgress))),
        estimatedCostAmount: parsedCost,
        estimatedCostCurrency: parsedCost === null ? null : 'USD',
        urgencyLevel,
        importanceLevel,
        emotionalValueLevel,
        desiredByDate: null,
        targetDate: null,
      });

      router.replace('/tabs/wants-needs');
    } catch (error) {
      Alert.alert(
        'Plan could not be added',
        error instanceof Error ? error.message : 'Please check the plan details and try again.',
      );
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">New plan</Text>
        <Text className="text-peacenest-muted">{familyName}</Text>
      </View>

      <Card className="gap-4">
        <View className="gap-2">
          <Text className="font-semibold">Type</Text>
          <View className="flex-row gap-2">
            <KindButton active={kind === WantNeedKind.Need} label="Need" onPress={() => setKind(WantNeedKind.Need)} />
            <KindButton active={kind === WantNeedKind.Want} label="Want" onPress={() => setKind(WantNeedKind.Want)} />
          </View>
        </View>

        <View className="gap-2">
          <Text className="font-semibold">Title</Text>
          <Input
            autoCapitalize="sentences"
            maxLength={180}
            onChangeText={setTitle}
            placeholder="School supplies"
            value={title}
          />
        </View>

        <View className="gap-2">
          <Text className="font-semibold">Note</Text>
          <Input
            className="min-h-24 py-3"
            maxLength={2000}
            multiline
            onChangeText={setDescription}
            placeholder="Why this matters for the family."
            textAlignVertical="top"
            value={description}
          />
        </View>

        <View className="flex-row gap-3">
          <View className="flex-1 gap-2">
            <Text className="font-semibold">Estimated cost</Text>
            <Input
              keyboardType="decimal-pad"
              onChangeText={setEstimatedCost}
              placeholder="75"
              value={estimatedCost}
            />
          </View>
          <View className="flex-1 gap-2">
            <Text className="font-semibold">Progress</Text>
            <Input keyboardType="number-pad" maxLength={3} onChangeText={setProgress} placeholder="0" value={progress} />
          </View>
        </View>

        <ScorePicker label="Urgency" onChange={setUrgencyLevel} value={urgencyLevel} />
        <ScorePicker label="Importance" onChange={setImportanceLevel} value={importanceLevel} />
        <ScorePicker label="Emotional value" onChange={setEmotionalValueLevel} value={emotionalValueLevel} />

        <Button
          disabled={!title.trim() || createWantOrNeed.isPending}
          label={createWantOrNeed.isPending ? 'Adding plan' : 'Add family plan'}
          onPress={handleCreate}
        />
      </Card>
    </Screen>
  );
}

function KindButton({ active, label, onPress }: { active: boolean; label: string; onPress: () => void }) {
  return (
    <Pressable
      accessibilityRole="button"
      className={cn(
        'flex-1 rounded-lg border border-peacenest-border px-4 py-3',
        active ? 'bg-peacenest-rose' : 'bg-peacenest-surface',
      )}
      onPress={onPress}
    >
      <Text className={cn('text-center font-semibold', active ? 'text-white' : 'text-peacenest-charcoal')}>{label}</Text>
    </Pressable>
  );
}

function ScorePicker({
  label,
  onChange,
  value,
}: {
  label: string;
  onChange: (value: ScoreLevelValue) => void;
  value: ScoreLevelValue;
}) {
  return (
    <View className="gap-2">
      <View className="flex-row items-center justify-between">
        <Text className="font-semibold">{label}</Text>
        <Badge label={scoreOptions.find((option) => option.value === value)?.label ?? 'Soon'} />
      </View>
      <View className="flex-row gap-2">
        {scoreOptions.map((option) => (
          <Pressable
            key={`${label}-${option.value}`}
            accessibilityRole="button"
            className={cn(
              'flex-1 rounded-lg border border-peacenest-border px-3 py-2',
              option.value === value ? 'bg-peacenest-blush' : 'bg-peacenest-surface',
            )}
            onPress={() => onChange(option.value)}
          >
            <Text className="text-center text-sm font-semibold">{option.label}</Text>
          </Pressable>
        ))}
      </View>
    </View>
  );
}
