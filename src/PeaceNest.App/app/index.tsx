import { Redirect } from 'expo-router';
import { useEffect, useState } from 'react';
import { ErrorState, LoadingState } from '@/components/ui/state';
import { useCurrentUser } from '@/hooks/use-current-user';
import { useAuth } from '@/lib/auth/auth-provider';
import { pendingInvitation } from '@/lib/invitations/pending-invitation';

export default function IndexRoute() {
  const { isLoading: isAuthLoading, session } = useAuth();
  const currentUser = useCurrentUser(Boolean(session));
  const [pendingToken, setPendingToken] = useState<string | null | undefined>(undefined);

  useEffect(() => {
    if (session) pendingInvitation.get().then(setPendingToken);
    else setPendingToken(null);
  }, [session]);

  if (isAuthLoading || (session && (currentUser.isLoading || pendingToken === undefined))) {
    return <LoadingState title="Opening your nest" />;
  }

  if (!session) return <Redirect href="/auth/sign-in" />;

  if (currentUser.isError) {
    return (
      <ErrorState
        title="We could not open your profile"
        message={currentUser.error.message}
        actionLabel="Try again"
        onAction={() => currentUser.refetch()}
      />
    );
  }

  if (!currentUser.data?.user.onboardingCompletedAt) return <Redirect href="/auth/profile" />;

  if (pendingToken) {
    return <Redirect href={{ pathname: '/family/join', params: { token: pendingToken } }} />;
  }

  return currentUser.data.familyMemberships.length > 0
    ? <Redirect href="/tabs/home" />
    : <Redirect href="/family/choose" />;
}
