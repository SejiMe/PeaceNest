import { Redirect, router } from 'expo-router';
import { useState } from 'react';
import { Alert, Pressable, Switch, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { DateField } from '@/components/date-field';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { EmptyState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useCreateMilestone } from '@/hooks/use-milestones';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { useAuth } from '@/lib/auth/auth-provider';
import { peaceNestColors } from '@/lib/theme/colors';
import { cn } from '@/lib/utils';

type DraftStep = {
  id: string;
  title: string;
};

export default function CreateMilestoneRoute() {
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
          message="Milestones belong inside a family workspace."
          actionLabel="Set up family"
          onAction={() => router.replace('/family/setup')}
        />
      </Screen>
    );
  }

  return <CreateMilestoneForm familyId={primaryFamily.familyId} familyName={primaryFamily.familyName} />;
}

function CreateMilestoneForm({ familyId, familyName }: { familyId: string; familyName: string }) {
  const createMilestone = useCreateMilestone(familyId);
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [targetDate, setTargetDate] = useState<string | null>(null);
  const [milestoneType, setMilestoneType] = useState('');
  const [progress, setProgress] = useState('0');
  const [includeInRecap, setIncludeInRecap] = useState(true);
  const [steps, setSteps] = useState<DraftStep[]>([
    { id: 'step-1', title: '' },
    { id: 'step-2', title: '' },
  ]);

  async function handleCreate() {
    const parsedProgress = progress.trim() ? Number(progress) : 0;

    if (Number.isNaN(parsedProgress)) {
      Alert.alert('Check the progress', 'Progress needs to be a number from 0 to 100.');
      return;
    }

    try {
      await createMilestone.mutateAsync({
        title,
        description: description.trim() ? description : null,
        priorityRank: null,
        progressPercent: Math.max(0, Math.min(100, Math.round(parsedProgress))),
        targetDate,
        milestoneType: milestoneType.trim() ? milestoneType : null,
        celebrationNotes: null,
        reflectionPrompt: null,
        includeInRecap,
        steps: steps
          .map((step, index) => ({
            title: step.title.trim(),
            description: null,
            sortOrder: index + 1,
          }))
          .filter((step) => step.title.length > 0),
      });

      router.replace('/tabs/milestones');
    } catch (error) {
      Alert.alert(
        'Milestone could not be added',
        error instanceof Error ? error.message : 'Please check the milestone details and try again.',
      );
    }
  }

  function updateStep(id: string, title: string) {
    setSteps((current) => current.map((step) => (step.id === id ? { ...step, title } : step)));
  }

  function addStep() {
    setSteps((current) => [...current, { id: `step-${Date.now()}`, title: '' }]);
  }

  function removeStep(id: string) {
    setSteps((current) => (current.length <= 1 ? current : current.filter((step) => step.id !== id)));
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">New milestone</Text>
        <Text className="text-peacenest-muted">{familyName}</Text>
      </View>

      <Card className="gap-4">
        <View className="gap-2">
          <Text className="font-semibold">Milestone title</Text>
          <Input
            autoCapitalize="sentences"
            maxLength={180}
            onChangeText={setTitle}
            placeholder="Sunday family dinner"
            value={title}
          />
        </View>

        <View className="gap-2">
          <Text className="font-semibold">Why it matters</Text>
          <Input
            className="min-h-24 py-3"
            maxLength={2000}
            multiline
            onChangeText={setDescription}
            placeholder="A small habit we want to grow together."
            textAlignVertical="top"
            value={description}
          />
        </View>

        <View className="flex-row gap-3">
          <View className="flex-1 gap-2">
            <Text className="font-semibold">Target date</Text>
            <DateField onChange={setTargetDate} value={targetDate} />
          </View>
          <View className="flex-1 gap-2">
            <Text className="font-semibold">Progress</Text>
            <Input keyboardType="number-pad" maxLength={3} onChangeText={setProgress} placeholder="0" value={progress} />
          </View>
        </View>

        <View className="gap-2">
          <Text className="font-semibold">Milestone kind</Text>
          <Input
            autoCapitalize="sentences"
            maxLength={120}
            onChangeText={setMilestoneType}
            placeholder="Habit, event, reflection"
            value={milestoneType}
          />
        </View>

        <View className="flex-row items-center justify-between rounded-lg bg-peacenest-blush p-3">
          <View className="flex-1 gap-1">
            <Text className="font-semibold">Include in monthly recap</Text>
            <Text variant="caption">Useful milestones can appear in family reflections.</Text>
          </View>
          <Switch
            onValueChange={setIncludeInRecap}
            thumbColor={peaceNestColors.surface}
            trackColor={{ false: peaceNestColors.border, true: peaceNestColors.sage }}
            value={includeInRecap}
          />
        </View>

        <View className="gap-3">
          <View className="flex-row items-center justify-between">
            <Text className="font-semibold">Starter checklist</Text>
            <Badge label={`${steps.filter((step) => step.title.trim()).length} steps`} tone="sage" />
          </View>

          {steps.map((step, index) => (
            <View key={step.id} className="gap-2">
              <Text variant="caption">Step {index + 1}</Text>
              <View className="flex-row gap-2">
                <Input
                  className="flex-1"
                  maxLength={180}
                  onChangeText={(value) => updateStep(step.id, value)}
                  placeholder="Pick a Sunday"
                  value={step.title}
                />
                <Pressable
                  accessibilityRole="button"
                  className={cn(
                    'min-h-12 justify-center rounded-lg border border-peacenest-border px-3',
                    steps.length <= 1 ? 'opacity-40' : 'bg-peacenest-surface',
                  )}
                  disabled={steps.length <= 1}
                  onPress={() => removeStep(step.id)}
                >
                  <Text className="font-semibold text-peacenest-muted">Remove</Text>
                </Pressable>
              </View>
            </View>
          ))}

          <Button label="Add step" onPress={addStep} variant="secondary" />
        </View>

        <Button
          disabled={!title.trim() || createMilestone.isPending}
          label={createMilestone.isPending ? 'Adding milestone' : 'Add milestone'}
          onPress={handleCreate}
        />
      </Card>
    </Screen>
  );
}
