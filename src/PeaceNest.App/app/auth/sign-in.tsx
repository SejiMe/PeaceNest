import { Redirect } from 'expo-router';
import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';
import { useAuth } from '@/lib/auth/auth-provider';

export default function SignInRoute() {
  const { isConfigured, session, signInWithGoogle } = useAuth();
  const [isSigningIn, setIsSigningIn] = useState(false);

  if (session) {
    return <Redirect href="/tabs/home" />;
  }

  async function handleGoogleSignIn() {
    try {
      setIsSigningIn(true);
      await signInWithGoogle();
    } catch (error) {
      Alert.alert('Sign in paused', error instanceof Error ? error.message : 'Please try again.');
    } finally {
      setIsSigningIn(false);
    }
  }

  return (
    <Screen className="justify-center" scroll>
      <View className="gap-3">
        <Text variant="title">PeaceNest</Text>
        <Text className="text-peacenest-muted">A calm place for family plans.</Text>
      </View>

      <Card className="gap-4">
        <Text variant="section">Welcome home</Text>
        <Button
          disabled={!isConfigured || isSigningIn}
          label={isSigningIn ? 'Opening Google' : 'Continue with Google'}
          onPress={handleGoogleSignIn}
        />
        {!isConfigured ? (
          <Text variant="caption">Supabase public settings are needed before sign in.</Text>
        ) : null}
      </Card>
    </Screen>
  );
}
