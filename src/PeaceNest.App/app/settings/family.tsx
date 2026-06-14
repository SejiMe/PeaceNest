import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function FamilySettingsRoute() {
  return (
    <Screen>
      <EmptyState
        title="Family settings"
        message="Family membership settings will appear here."
        actionLabel="Back home"
        onAction={() => router.replace('/tabs/home')}
      />
    </Screen>
  );
}
