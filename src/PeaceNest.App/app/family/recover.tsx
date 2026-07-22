import { Redirect, router } from 'expo-router';
import { RotateCcw } from 'lucide-react-native';
import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';
import { useRecoverFamily } from '@/hooks/use-family-departure';
import { useAuth } from '@/lib/auth/auth-provider';

export default function RecoverFamilyRoute() {
  const { session } = useAuth();
  const recoverFamily = useRecoverFamily();
  const [code, setCode] = useState('');

  if (!session) return <Redirect href="/auth/sign-in" />;

  const normalizedLength = code.replace(/[-\s]/g, '').length;

  async function handleRecover() {
    try {
      const recovered = await recoverFamily.mutateAsync({ code });
      Alert.alert('Family restored', `${recovered.familyName} is ready for you again.`);
      router.replace('/tabs/home');
    } catch (error) {
      Alert.alert('Family could not be restored', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Recover a family</Text>
        <Text className="text-peacenest-muted">Restore a workspace you created and left within its recovery window.</Text>
      </View>
      <Card className="gap-4">
        <View className="flex-row items-center gap-3">
          <RotateCcw color="#61715F" size={24} />
          <Text variant="section">Recovery code</Text>
        </View>
        <Input
          autoCapitalize="characters"
          autoCorrect={false}
          maxLength={23}
          onChangeText={(value) => setCode(value.toUpperCase())}
          placeholder="ABCDE-FGHIJ-KLMNP-QRSTU"
          value={code}
        />
        <Button
          disabled={normalizedLength !== 20 || recoverFamily.isPending}
          label={recoverFamily.isPending ? 'Restoring family' : 'Restore family'}
          onPress={handleRecover}
        />
      </Card>
      <Button label="Back" onPress={() => router.back()} variant="ghost" />
    </Screen>
  );
}
