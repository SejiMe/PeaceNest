import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';
import { useCreateFamilyWorkspace } from '@/hooks/use-family-workspaces';

export default function FamilySetupRoute() {
  const createFamily = useCreateFamilyWorkspace();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  async function handleCreateFamily() {
    try {
      const created = await createFamily.mutateAsync({
        name,
        description: description.trim() ? description : null,
      });

      router.replace('/tabs/home');
      Alert.alert('Family workspace created', `${created.name} is ready.`);
    } catch (error) {
      Alert.alert(
        'Family setup paused',
        error instanceof Error ? error.message : 'Please check the family details and try again.',
      );
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Set up family</Text>
        <Text className="text-peacenest-muted">
          Create the shared planning space your family will use together.
        </Text>
      </View>

      <Card className="gap-4">
        <View className="gap-2">
          <Text className="font-semibold">Family name</Text>
          <Input
            autoCapitalize="words"
            maxLength={160}
            onChangeText={setName}
            placeholder="The Santos Nest"
            value={name}
          />
        </View>

        <View className="gap-2">
          <Text className="font-semibold">Gentle note</Text>
          <Input
            className="min-h-24 py-3"
            maxLength={500}
            multiline
            onChangeText={setDescription}
            placeholder="A calm place for our family plans."
            textAlignVertical="top"
            value={description}
          />
        </View>

        <Button
          disabled={!name.trim() || createFamily.isPending}
          label={createFamily.isPending ? 'Creating nest' : 'Create family workspace'}
          onPress={handleCreateFamily}
        />
      </Card>
    </Screen>
  );
}
