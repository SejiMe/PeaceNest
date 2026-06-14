import { useCurrentUser } from '@/hooks/use-current-user';
import { useAuth } from '@/lib/auth/auth-provider';

export function usePrimaryFamily() {
  const { session } = useAuth();
  const currentUser = useCurrentUser(Boolean(session));
  const primaryFamily = currentUser.data?.familyMemberships[0] ?? null;

  return {
    currentUser,
    isAuthenticated: Boolean(session),
    primaryFamily,
  };
}
