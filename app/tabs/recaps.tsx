import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function RecapsRoute() {
  return (
    <Screen>
      <EmptyState title="Monthly Recaps" message="Family reflections will appear here." />
    </Screen>
  );
}
