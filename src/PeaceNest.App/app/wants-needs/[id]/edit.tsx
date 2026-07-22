import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { useState } from 'react';
import { Alert, Pressable, View } from 'react-native';
import { CurrencyPicker } from '@/components/currency-picker';
import { DateField } from '@/components/date-field';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { useUpdateWantOrNeed, useWantOrNeed } from '@/hooks/use-wants-and-needs';
import {
  PlanStatus,
  ScoreLevel,
  WantNeedKind,
  type ScoreLevel as ScoreLevelValue,
  type SupportedCurrency,
  type WantNeedKind as WantNeedKindValue,
  type WantOrNeedResponse,
} from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';
import { cn } from '@/lib/utils';

const urgencyOptions = [
  { label: 'Someday', value: ScoreLevel.Low },
  { label: 'Soon', value: ScoreLevel.Medium },
  { label: 'Now', value: ScoreLevel.High },
] as const;
const levelOptions = [
  { label: 'Low', value: ScoreLevel.Low },
  { label: 'Medium', value: ScoreLevel.Medium },
  { label: 'High', value: ScoreLevel.High },
] as const;

export default function EditWantOrNeedRoute() {
  const { session } = useAuth();
  const { id } = useLocalSearchParams<{ id: string }>();
  const planId = Array.isArray(id) ? id[0] : id;
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const query = useWantOrNeed(primaryFamily?.familyId, planId);

  if (!session) return <Redirect href="/auth/sign-in" />;
  if (currentUser.isLoading || query.isLoading) return <LoadingState title="Opening plan editor" />;
  if (!primaryFamily || !planId || query.isError || !query.data) {
    return <ErrorState title="Plan could not open" message={query.error?.message ?? 'The plan is unavailable.'} actionLabel="Back" onAction={() => router.back()} />;
  }
  if (query.data.wantOrNeed.status !== PlanStatus.Active) return <Redirect href={{ pathname: '/wants-needs/[id]', params: { id: planId } }} />;

  return <EditForm familyId={primaryFamily.familyId} plan={query.data.wantOrNeed} />;
}

function EditForm({ familyId, plan }: { familyId: string; plan: WantOrNeedResponse }) {
  const updatePlan = useUpdateWantOrNeed(familyId, plan.id);
  const [kind, setKind] = useState<WantNeedKindValue>(plan.kind);
  const [title, setTitle] = useState(plan.title);
  const [description, setDescription] = useState(plan.description ?? '');
  const [cost, setCost] = useState(plan.estimatedCostAmount?.toString() ?? '');
  const [currency, setCurrency] = useState<SupportedCurrency>((plan.estimatedCostCurrency as SupportedCurrency) ?? 'PHP');
  const [priorityRank, setPriorityRank] = useState(plan.priorityRank?.toString() ?? '');
  const [urgency, setUrgency] = useState<ScoreLevelValue>(plan.urgencyLevel);
  const [importance, setImportance] = useState<ScoreLevelValue>(plan.importanceLevel);
  const [emotionalValue, setEmotionalValue] = useState<ScoreLevelValue>(plan.emotionalValueLevel);
  const [desiredByDate, setDesiredByDate] = useState<string | null>(plan.desiredByDate ?? null);
  const [targetDate, setTargetDate] = useState<string | null>(plan.targetDate ?? null);

  async function handleSave() {
    const amount = cost.trim() ? Number(cost) : null;
    const rank = priorityRank.trim() ? Number(priorityRank) : null;
    if ((amount !== null && Number.isNaN(amount)) || (rank !== null && Number.isNaN(rank))) {
      Alert.alert('Check the numbers', 'Cost and priority rank need valid numbers.');
      return;
    }

    try {
      await updatePlan.mutateAsync({
        kind,
        title,
        description: description.trim() || null,
        priorityRank: rank,
        estimatedCostAmount: amount,
        estimatedCostCurrency: amount === null ? null : currency,
        urgencyLevel: urgency,
        importanceLevel: importance,
        emotionalValueLevel: emotionalValue,
        desiredByDate,
        targetDate,
        version: plan.version,
      });
      router.replace({ pathname: '/wants-needs/[id]', params: { id: plan.id } });
    } catch (error) {
      Alert.alert('Plan could not save', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2"><Text variant="title">Edit plan</Text><Text className="text-peacenest-muted">Active plans can grow with your family.</Text></View>
      <Card className="gap-4">
        <View className="flex-row gap-2">
          <Choice active={kind === WantNeedKind.Need} label="Need" onPress={() => setKind(WantNeedKind.Need)} />
          <Choice active={kind === WantNeedKind.Want} label="Want" onPress={() => setKind(WantNeedKind.Want)} />
        </View>
        <Field label="Title"><Input maxLength={180} onChangeText={setTitle} value={title} /></Field>
        <Field label="Why it matters"><Input className="min-h-24 py-3" maxLength={2000} multiline onChangeText={setDescription} value={description} /></Field>
        <Field label="Estimated cost"><Input keyboardType="decimal-pad" onChangeText={setCost} value={cost} /></Field>
        <Field label="Currency"><CurrencyPicker onChange={setCurrency} value={currency} /></Field>
        <Field label="Priority rank"><Input keyboardType="number-pad" onChangeText={setPriorityRank} value={priorityRank} /></Field>
        <Score label="Urgency" onChange={setUrgency} options={urgencyOptions} value={urgency} />
        <Score label="Importance" onChange={setImportance} options={levelOptions} value={importance} />
        <Score label="Emotional value" onChange={setEmotionalValue} options={levelOptions} value={emotionalValue} />
        <Field label="Desired by"><DateField onChange={setDesiredByDate} value={desiredByDate} /></Field>
        <Field label="Target date"><DateField onChange={setTargetDate} value={targetDate} /></Field>
        <Button disabled={!title.trim() || updatePlan.isPending} label={updatePlan.isPending ? 'Saving' : 'Save changes'} onPress={handleSave} />
        <Button label="Cancel" onPress={() => router.back()} variant="ghost" />
      </Card>
    </Screen>
  );
}

function Field({ children, label }: { children: React.ReactNode; label: string }) {
  return <View className="gap-2"><Text className="font-semibold">{label}</Text>{children}</View>;
}

function Choice({ active, label, onPress }: { active: boolean; label: string; onPress: () => void }) {
  return <Pressable className={cn('min-h-11 flex-1 items-center justify-center rounded-lg border', active ? 'border-peacenest-rose bg-peacenest-rose' : 'border-peacenest-border bg-peacenest-surface')} onPress={onPress}><Text className={active ? 'font-semibold text-white' : 'font-semibold'}>{label}</Text></Pressable>;
}

function Score({ label, onChange, options, value }: { label: string; onChange: (value: ScoreLevelValue) => void; options: ReadonlyArray<{ label: string; value: ScoreLevelValue }>; value: ScoreLevelValue }) {
  return <View className="gap-2"><View className="flex-row items-center justify-between"><Text className="font-semibold">{label}</Text><Badge label={options.find((option) => option.value === value)?.label ?? ''} /></View><View className="flex-row gap-2">{options.map((option) => <Choice key={`${label}-${option.value}`} active={option.value === value} label={option.label} onPress={() => onChange(option.value)} />)}</View></View>;
}
