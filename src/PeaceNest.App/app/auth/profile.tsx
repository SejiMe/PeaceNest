import { getLocales } from 'expo-localization';
import { Redirect, router } from 'expo-router';
import { useEffect, useState } from 'react';
import { Alert, View } from 'react-native';
import { CountryPicker } from '@/components/country-picker';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useCompleteProfile, useCurrentUser } from '@/hooks/use-current-user';
import { useAuth } from '@/lib/auth/auth-provider';
import { pendingInvitation } from '@/lib/invitations/pending-invitation';

export default function ProfileOnboardingRoute() {
  const { session, user } = useAuth();
  const currentUser = useCurrentUser(Boolean(session));
  const completeProfile = useCompleteProfile();
  const [displayName, setDisplayName] = useState('');
  const [countryCode, setCountryCode] = useState('');
  const [didInitialize, setDidInitialize] = useState(false);

  useEffect(() => {
    if (!currentUser.data || didInitialize) return;
    const googleName = user?.user_metadata?.full_name ?? user?.user_metadata?.name;
    setDisplayName(currentUser.data.user.displayName || googleName || '');
    setCountryCode(currentUser.data.user.countryCode ?? getLocales()[0]?.regionCode ?? 'PH');
    setDidInitialize(true);
  }, [currentUser.data, didInitialize, user]);

  if (!session) return <Redirect href="/auth/sign-in" />;
  if (currentUser.isLoading || !didInitialize) return <LoadingState title="Preparing your profile" />;

  async function handleContinue() {
    try {
      await completeProfile.mutateAsync({ displayName, countryCode });
      const token = await pendingInvitation.get();
      if (token) router.replace({ pathname: '/family/join', params: { token } });
      else if ((currentUser.data?.familyMemberships.length ?? 0) > 0) router.replace('/tabs/home');
      else router.replace('/family/choose');
    } catch (error) {
      Alert.alert('Profile could not save', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">A little about you</Text>
        <Text className="text-peacenest-muted">Choose how your family will know you and where your planning starts.</Text>
      </View>

      <Card className="gap-4">
        <View className="gap-2">
          <Text className="font-semibold">Display name</Text>
          <Input autoCapitalize="words" maxLength={200} onChangeText={setDisplayName} value={displayName} />
        </View>
        <View className="gap-2">
          <Text className="font-semibold">Country or region</Text>
          <CountryPicker onChange={setCountryCode} value={countryCode} />
          <Text variant="caption">This only suggests defaults for new family workspaces.</Text>
        </View>
        <Button
          disabled={!displayName.trim() || !countryCode || completeProfile.isPending}
          label={completeProfile.isPending ? 'Saving profile' : 'Continue'}
          onPress={handleContinue}
        />
      </Card>
    </Screen>
  );
}
