import * as Clipboard from 'expo-clipboard';
import { router } from 'expo-router';
import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';
import { useAuth } from '@/lib/auth/auth-provider';

export function DevAuthTokenPage() {
  const { accessToken, refreshSession, session, signInWithGoogle, signOut, user } = useAuth();
  const [isRefreshing, setIsRefreshing] = useState(false);

  async function handleCopy() {
    if (!accessToken) {
      return;
    }

    await Clipboard.setStringAsync(accessToken);
    Alert.alert('Copied', 'Token copied for local backend testing.');
  }

  async function handleRefresh() {
    try {
      setIsRefreshing(true);
      await refreshSession();
    } catch (error) {
      Alert.alert('Refresh paused', error instanceof Error ? error.message : 'Please sign in again.');
    } finally {
      setIsRefreshing(false);
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Dev token</Text>
        <Text className="text-peacenest-muted">{user?.email ?? 'No active session'}</Text>
      </View>

      <Card className="gap-4">
        <Text variant="section">Access token</Text>
        <Text className="rounded-lg bg-peacenest-blush p-3 font-mono text-xs" selectable>
          {accessToken ?? 'Sign in to view a local testing token.'}
        </Text>
        <Text variant="caption">
          {session?.expires_at
            ? `Expires at ${new Date(session.expires_at * 1000).toLocaleString()}`
            : 'No expiry available'}
        </Text>
        <Button disabled={!accessToken} label="Copy token" onPress={handleCopy} />
        <Button
          disabled={!session || isRefreshing}
          label={isRefreshing ? 'Refreshing' : 'Refresh session'}
          onPress={handleRefresh}
          variant="secondary"
        />
        {session ? (
          <Button label="Sign out" onPress={signOut} variant="ghost" />
        ) : (
          <Button label="Continue with Google" onPress={signInWithGoogle} />
        )}
      </Card>
    </Screen>
  );
}
