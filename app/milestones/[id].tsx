import { router } from 'expo-router';
import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function MilestoneDetailRoute() {
  return (
    <Screen>
      <EmptyState
        title="Milestone details"
        message="Checklist progress will appear here."
        actionLabel="Back to milestones"
        onAction={() => router.replace('/tabs/milestones')}
      />
    </Screen>
  );
}
