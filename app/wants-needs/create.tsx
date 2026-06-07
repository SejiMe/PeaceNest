import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function CreateWantNeedRoute() {
  return (
    <Screen>
      <EmptyState
        title="New want or need"
        message="The create form will connect to the backend slice here."
        actionLabel="Back to plans"
        onAction={() => router.replace('/tabs/wants-needs')}
      />
    </Screen>
  );
}
