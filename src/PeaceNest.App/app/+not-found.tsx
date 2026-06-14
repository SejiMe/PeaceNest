import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function NotFoundRoute() {
  return (
    <Screen>
      <EmptyState
        title="This nest is quiet"
        message="That page is not available."
        actionLabel="Go home"
        onAction={() => router.replace('/')}
      />
    </Screen>
  );
}
