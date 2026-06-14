import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function ProfileSettingsRoute() {
  return (
    <Screen>
      <EmptyState
        title="Profile"
        message="Your PeaceNest profile will appear here."
        actionLabel="Back home"
        onAction={() => router.replace('/tabs/home')}
      />
    </Screen>
  );
}
