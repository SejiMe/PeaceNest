import { Redirect, router } from 'expo-router';
import { View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useCurrentUser } from '@/hooks/use-current-user';
import { roleLabel } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function HomeRoute() {
  const { session, signOut, user } = useAuth();
  const currentUser = useCurrentUser(Boolean(session));

  if (!session) {
    return <Redirect href="/auth/sign-in" />;
  }

  if (currentUser.isLoading) {
    return <LoadingState />;
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Family nest</Text>
        <Text className="text-peacenest-muted">{user?.email}</Text>
      </View>

      {currentUser.isError ? (
        <ErrorState
          title="We could not open your nest"
          message={currentUser.error.message}
          actionLabel="Try again"
          onAction={() => currentUser.refetch()}
        />
      ) : null}

      {currentUser.data ? (
        <Card className="gap-3">
          <Text variant="section">Family workspace</Text>
          {currentUser.data.familyMemberships.length > 0 ? (
            currentUser.data.familyMemberships.map((membership) => (
              <View key={membership.familyId} className="rounded-lg bg-peacenest-blush p-3">
                <Text className="font-semibold">{membership.familyName}</Text>
                <Text variant="caption">{roleLabel(membership.role)}</Text>
              </View>
            ))
          ) : (
            <View className="gap-3 rounded-lg bg-peacenest-blush p-3">
              <Text className="font-semibold">Start your family space</Text>
              <Text variant="caption">Create or join a family workspace.</Text>
              <Button label="Set up family" onPress={() => router.push('/family/setup')} />
            </View>
          )}
        </Card>
      ) : null}

      {currentUser.data?.familyMemberships.length ? (
        <View className="gap-3">
          <Button label="Open Wants & Needs" onPress={() => router.push('/tabs/wants-needs')} />
          <Button label="Invite family" onPress={() => router.push('/family/invite')} variant="secondary" />
        </View>
      ) : null}

      <Button label="Sign out" onPress={signOut} variant="ghost" />
    </Screen>
  );
}
