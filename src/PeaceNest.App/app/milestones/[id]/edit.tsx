import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { Plus, Trash2 } from 'lucide-react-native';
import { useState } from 'react';
import { Alert, Pressable, Switch, View } from 'react-native';
import { DateField } from '@/components/date-field';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useMilestone, useUpdateMilestone } from '@/hooks/use-milestones';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { PlanStatus, type MilestoneResponse } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';
import { peaceNestColors } from '@/lib/theme/colors';

type DraftStep = {
  key: string;
  id?: string;
  title: string;
  description: string;
};

export default function EditMilestoneRoute() {
  const { session } = useAuth();
  const { id } = useLocalSearchParams<{ id: string }>();
  const milestoneId = Array.isArray(id) ? id[0] : id;
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const query = useMilestone(primaryFamily?.familyId, milestoneId);

  if (!session) return <Redirect href="/auth/sign-in" />;
  if (currentUser.isLoading || query.isLoading) return <LoadingState title="Opening milestone editor" />;
  if (!primaryFamily || !milestoneId || query.isError || !query.data) {
    return <ErrorState title="Milestone could not open" message={query.error?.message ?? 'The milestone is unavailable.'} actionLabel="Back" onAction={() => router.back()} />;
  }
  if (query.data.milestone.status !== PlanStatus.Active) return <Redirect href={{ pathname: '/milestones/[id]', params: { id: milestoneId } }} />;

  return <EditForm familyId={primaryFamily.familyId} milestone={query.data.milestone} />;
}

function EditForm({ familyId, milestone }: { familyId: string; milestone: MilestoneResponse }) {
  const updateMilestone = useUpdateMilestone(familyId, milestone.id);
  const [title, setTitle] = useState(milestone.title);
  const [description, setDescription] = useState(milestone.description ?? '');
  const [priorityRank, setPriorityRank] = useState(milestone.priorityRank?.toString() ?? '');
  const [targetDate, setTargetDate] = useState<string | null>(milestone.targetDate ?? null);
  const [milestoneType, setMilestoneType] = useState(milestone.milestoneType ?? '');
  const [celebrationNotes, setCelebrationNotes] = useState(milestone.celebrationNotes ?? '');
  const [reflectionPrompt, setReflectionPrompt] = useState(milestone.reflectionPrompt ?? '');
  const [includeInRecap, setIncludeInRecap] = useState(milestone.includeInRecap);
  const [steps, setSteps] = useState<DraftStep[]>(
    milestone.steps.map((step) => ({ key: step.id, id: step.id, title: step.title, description: step.description ?? '' })),
  );

  function updateStep(key: string, change: Partial<DraftStep>) {
    setSteps((current) => current.map((step) => step.key === key ? { ...step, ...change } : step));
  }

  async function handleSave() {
    const rank = priorityRank.trim() ? Number(priorityRank) : null;
    if (rank !== null && Number.isNaN(rank)) {
      Alert.alert('Check priority rank', 'Priority rank needs to be a number.');
      return;
    }

    try {
      await updateMilestone.mutateAsync({
        title,
        description: description.trim() || null,
        priorityRank: rank,
        targetDate,
        milestoneType: milestoneType.trim() || null,
        celebrationNotes: celebrationNotes.trim() || null,
        reflectionPrompt: reflectionPrompt.trim() || null,
        includeInRecap,
        steps: steps.filter((step) => step.title.trim()).map((step, index) => ({
          id: step.id ?? null,
          title: step.title.trim(),
          description: step.description.trim() || null,
          sortOrder: index + 1,
        })),
        version: milestone.version,
      });
      router.replace({ pathname: '/milestones/[id]', params: { id: milestone.id } });
    } catch (error) {
      Alert.alert('Milestone could not save', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2"><Text variant="title">Edit milestone</Text><Text className="text-peacenest-muted">Keep the shared path clear and meaningful.</Text></View>
      <Card className="gap-4">
        <Field label="Milestone title"><Input maxLength={180} onChangeText={setTitle} value={title} /></Field>
        <Field label="Why it matters"><Input className="min-h-24 py-3" maxLength={2000} multiline onChangeText={setDescription} value={description} /></Field>
        <Field label="Target date"><DateField onChange={setTargetDate} value={targetDate} /></Field>
        <Field label="Priority rank"><Input keyboardType="number-pad" onChangeText={setPriorityRank} value={priorityRank} /></Field>
        <Field label="Milestone kind"><Input maxLength={120} onChangeText={setMilestoneType} value={milestoneType} /></Field>
        <Field label="Celebration note"><Input className="min-h-20 py-3" maxLength={1000} multiline onChangeText={setCelebrationNotes} value={celebrationNotes} /></Field>
        <Field label="Reflection prompt"><Input className="min-h-20 py-3" maxLength={1000} multiline onChangeText={setReflectionPrompt} value={reflectionPrompt} /></Field>

        <View className="flex-row items-center justify-between rounded-lg bg-peacenest-blush p-3">
          <Text className="flex-1 font-semibold">Include in monthly recap</Text>
          <Switch onValueChange={setIncludeInRecap} thumbColor={peaceNestColors.surface} trackColor={{ false: peaceNestColors.border, true: peaceNestColors.sage }} value={includeInRecap} />
        </View>

        <View className="gap-3">
          <Text className="font-semibold">Checklist</Text>
          {steps.map((step, index) => (
            <View key={step.key} className="gap-2 rounded-lg bg-peacenest-blush p-3">
              <View className="flex-row items-center justify-between"><Text variant="caption">Step {index + 1}</Text><Pressable accessibilityLabel="Remove step" onPress={() => setSteps((current) => current.filter((item) => item.key !== step.key))}><Trash2 color="#D96B6B" size={20} /></Pressable></View>
              <Input maxLength={180} onChangeText={(value) => updateStep(step.key, { title: value })} placeholder="Step title" value={step.title} />
              <Input maxLength={1000} onChangeText={(value) => updateStep(step.key, { description: value })} placeholder="Optional note" value={step.description} />
            </View>
          ))}
          <Pressable className="min-h-11 flex-row items-center justify-center gap-2 rounded-lg border border-peacenest-border bg-peacenest-surface" disabled={steps.length >= 25} onPress={() => setSteps((current) => [...current, { key: `new-${Date.now()}`, title: '', description: '' }])}><Plus color="#D97C83" size={20} /><Text className="font-semibold">Add step</Text></Pressable>
        </View>

        <Button disabled={!title.trim() || updateMilestone.isPending} label={updateMilestone.isPending ? 'Saving' : 'Save changes'} onPress={handleSave} />
        <Button label="Cancel" onPress={() => router.back()} variant="ghost" />
      </Card>
    </Screen>
  );
}

function Field({ children, label }: { children: React.ReactNode; label: string }) {
  return <View className="gap-2"><Text className="font-semibold">{label}</Text>{children}</View>;
}
