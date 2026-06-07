import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function WantsNeedsRoute() {
  return (
    <Screen>
      <EmptyState
        title="Wants & Needs"
        message="Family priorities will appear here."
      />
    </Screen>
  );
}
