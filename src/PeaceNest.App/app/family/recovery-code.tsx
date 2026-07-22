import * as Clipboard from 'expo-clipboard';
import { Redirect, router } from 'expo-router';
import { Copy, ShieldCheck } from 'lucide-react-native';
import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';
import {
  takeRecoveryCodeHandoff,
  type RecoveryCodeHandoff,
} from '@/lib/family-recovery/recovery-code-handoff';
import { useAuth } from '@/lib/auth/auth-provider';

export default function FamilyRecoveryCodeRoute() {
  const { session } = useAuth();
  const [recovery] = useState<RecoveryCodeHandoff | null>(() => takeRecoveryCodeHandoff());

  if (!session) return <Redirect href="/auth/sign-in" />;
  if (!recovery) return <Redirect href="/family/choose" />;

  async function handleCopy() {
    await Clipboard.setStringAsync(recovery!.recoveryCode);
    Alert.alert('Recovery code copied', 'Keep it somewhere private. PeaceNest cannot show it again.');
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Keep your recovery code</Text>
        <Text className="text-peacenest-muted">Your family workspace is now inactive.</Text>
      </View>

      <Card className="gap-4 bg-peacenest-blush">
        <View className="flex-row items-start gap-3">
          <ShieldCheck color="#61715F" size={24} />
          <View className="flex-1 gap-1">
            <Text variant="section">One-time recovery</Text>
            <Text variant="caption">Only your Google-authenticated creator profile can use this code.</Text>
          </View>
        </View>
        <Text className="text-center text-xl font-bold">{recovery.recoveryCode}</Text>
        <Button label="Copy recovery code" onPress={handleCopy} variant="secondary" />
      </Card>

      <Card className="gap-2 border-peacenest-danger">
        <Text className="font-semibold text-peacenest-danger">Permanent deletion</Text>
        <Text variant="caption">
          Recover before {formatDeadline(recovery.recoveryExpiresAt)}. After that deadline, the workspace and its family data are permanently deleted.
        </Text>
      </Card>

      <Button label="I have kept the code" onPress={() => router.replace('/family/choose')} />
    </Screen>
  );
}

function formatDeadline(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}
