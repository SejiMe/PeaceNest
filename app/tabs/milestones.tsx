import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function MilestonesRoute() {
  return (
    <Screen>
      <EmptyState
        title="Family Milestones"
        message="Shared goals and gentle progress will appear here."
      />
    </Screen>
  );
}
