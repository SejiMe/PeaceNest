import { router } from 'expo-router';
import { Button } from '@/components/ui/button';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function FamilySetupRoute() {
  return (
    <Screen>
      <EmptyState
        title="Family setup"
        message="Create and join flows will connect here."
        actionLabel="Back home"
        onAction={() => router.replace('/tabs/home')}
      />
      <Button label="Invite family" onPress={() => router.push('/family/invite')} variant="secondary" />
    </Screen>
  );
}
