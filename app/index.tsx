import { Redirect } from 'expo-router';
import { LoadingState } from '@/components/ui/state';
import { useAuth } from '@/lib/auth/auth-provider';

export default function IndexRoute() {
  const { isLoading, session } = useAuth();

  if (isLoading) {
    return <LoadingState />;
  }

  return <Redirect href={session ? '/tabs/home' : '/auth/sign-in'} />;
}
