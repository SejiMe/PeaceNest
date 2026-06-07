import { ActivityIndicator, View } from 'react-native';
import { peaceNestColors } from '@/lib/theme/colors';
import { Button } from './button';
import { Card } from './card';
import { Text } from './text';

type StateBlockProps = {
  title: string;
  message?: string;
  actionLabel?: string;
  onAction?: () => void;
};

export function LoadingState({ title = 'Gathering your nest' }: Partial<StateBlockProps>) {
  return (
    <View className="flex-1 items-center justify-center gap-4">
      <ActivityIndicator color={peaceNestColors.roseAccent} />
      <Text variant="caption">{title}</Text>
    </View>
  );
}

export function EmptyState({ actionLabel, message, onAction, title }: StateBlockProps) {
  return (
    <Card className="gap-3">
      <Text variant="section">{title}</Text>
      {message ? <Text className="text-peacenest-muted">{message}</Text> : null}
      {actionLabel && onAction ? <Button label={actionLabel} onPress={onAction} /> : null}
    </Card>
  );
}

export function ErrorState({ actionLabel, message, onAction, title }: StateBlockProps) {
  return (
    <Card className="gap-3 border-peacenest-danger bg-peacenest-surface">
      <Text variant="section">{title}</Text>
      {message ? <Text className="text-peacenest-muted">{message}</Text> : null}
      {actionLabel && onAction ? <Button label={actionLabel} onPress={onAction} variant="secondary" /> : null}
    </Card>
  );
}
