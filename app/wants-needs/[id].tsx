import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function WantNeedDetailRoute() {
  return (
    <Screen>
      <EmptyState
        title="Plan details"
        message="Want or need details will appear here."
        actionLabel="Back to plans"
        onAction={() => router.replace('/tabs/wants-needs')}
      />
    </Screen>
  );
}
