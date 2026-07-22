import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { useEffect } from 'react';
import { LoadingState } from '@/components/ui/state';
import { pendingInvitation } from '@/lib/invitations/pending-invitation';

export default function InvitationLinkRoute() {
  const params = useLocalSearchParams<{ token?: string }>();
  const token = typeof params.token === 'string' ? params.token : null;

  useEffect(() => {
    if (token) pendingInvitation.save(token).then(() => router.replace('/'));
  }, [token]);

  if (!token) return <Redirect href="/" />;
  return <LoadingState title="Keeping your invitation ready" />;
}
