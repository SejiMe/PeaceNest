import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function CreateMilestoneRoute() {
  return (
    <Screen>
      <EmptyState
        title="New milestone"
        message="The milestone form will connect here."
        actionLabel="Back to milestones"
        onAction={() => router.replace('/tabs/milestones')}
      />
    </Screen>
  );
}
