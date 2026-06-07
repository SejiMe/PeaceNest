import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function FamilyInviteRoute() {
  return (
    <Screen>
      <EmptyState
        title="Invite family"
        message="Invitation forms will connect here."
        actionLabel="Back home"
        onAction={() => router.replace('/tabs/home')}
      />
    </Screen>
  );
}
