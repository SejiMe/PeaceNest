import { Screen } from '@/components/ui/screen';
import { EmptyState } from '@/components/ui/state';

export default function NotificationsRoute() {
  return (
    <Screen>
      <EmptyState title="Notifications" message="Gentle family updates will appear here." />
    </Screen>
  );
}
