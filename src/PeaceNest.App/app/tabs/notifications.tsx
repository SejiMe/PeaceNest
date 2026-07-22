import { Redirect } from 'expo-router';
import { Alert, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useNotifications, useMarkNotificationRead } from '@/hooks/use-notifications';
import { notificationTypeLabel, type NotificationResponse } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function NotificationsRoute() {
  const { session } = useAuth();
  const notifications = useNotifications(Boolean(session));
  const markRead = useMarkNotificationRead();

  if (!session) {
    return <Redirect href="/auth/sign-in" />;
  }

  async function handleMarkRead(notificationId: string) {
    try {
      await markRead.mutateAsync(notificationId);
    } catch (error) {
      Alert.alert('Notification could not update', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Notifications</Text>
        <Text className="text-peacenest-muted">Updates meant for you across your family spaces.</Text>
      </View>

      {notifications.data ? (
        <Card className="gap-2 bg-peacenest-blush">
          <Text variant="section">
            {notifications.data.unreadCount === 0 ? 'All caught up' : `${notifications.data.unreadCount} unread`}
          </Text>
          <Text variant="caption">Gentle family updates, without private details in the preview.</Text>
        </Card>
      ) : null}

      {notifications.isLoading ? <LoadingState title="Gathering family updates" /> : null}

      {notifications.isError ? (
        <ErrorState
          title="Notifications could not open"
          message={notifications.error.message}
          actionLabel="Try again"
          onAction={() => notifications.refetch()}
        />
      ) : null}

      {!notifications.isLoading && (notifications.data?.notifications.length ?? 0) === 0 ? (
        <EmptyState
          title="No gentle updates yet"
          message="Family votes, notes, milestones, and recaps will appear here when there is something new."
        />
      ) : null}

      <View className="gap-3">
        {notifications.data?.notifications.map((notification) => (
          <NotificationCard
            key={notification.id}
            isUpdating={markRead.isPending}
            notification={notification}
            onMarkRead={handleMarkRead}
          />
        ))}
      </View>
    </Screen>
  );
}

type NotificationCardProps = {
  notification: NotificationResponse;
  isUpdating: boolean;
  onMarkRead: (notificationId: string) => void;
};

function NotificationCard({ isUpdating, notification, onMarkRead }: NotificationCardProps) {
  const isUnread = !notification.readAt;

  return (
    <Card className="gap-3">
      <View className="flex-row flex-wrap items-start justify-between gap-3">
        <View className="flex-1 gap-1">
          <Text className="text-lg font-semibold">{notification.title}</Text>
          <Text variant="caption">
            {notification.actorDisplayName
              ? `${notification.actorDisplayName} - ${formatDateTime(notification.createdAt)}`
              : formatDateTime(notification.createdAt)}
          </Text>
        </View>
        <View className="gap-2">
          <Badge label={isUnread ? 'Unread' : 'Read'} tone={isUnread ? 'gold' : 'sage'} />
          <Badge label={notificationTypeLabel(notification.type)} tone="muted" />
        </View>
      </View>

      {notification.body ? <Text>{notification.body}</Text> : null}

      {isUnread ? (
        <Button
          className="self-start px-4"
          disabled={isUpdating}
          label={isUpdating ? 'Updating' : 'Mark read'}
          onPress={() => onMarkRead(notification.id)}
          variant="secondary"
        />
      ) : null}
    </Card>
  );
}

function formatDateTime(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  }).format(date);
}
