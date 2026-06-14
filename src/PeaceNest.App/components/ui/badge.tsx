import { View, type ViewProps } from 'react-native';
import { cn } from '@/lib/utils';
import { Text } from './text';

type BadgeProps = ViewProps & {
  label: string;
  tone?: 'need' | 'want' | 'sage' | 'gold' | 'muted';
};

const badgeTones = {
  need: 'bg-peacenest-goldLight',
  want: 'bg-peacenest-blush',
  sage: 'bg-peacenest-sage/20',
  gold: 'bg-peacenest-goldLight',
  muted: 'bg-peacenest-blush',
};

const textTones = {
  need: 'text-peacenest-clay',
  want: 'text-peacenest-rose',
  sage: 'text-peacenest-charcoal',
  gold: 'text-peacenest-clay',
  muted: 'text-peacenest-muted',
};

export function Badge({ className, label, tone = 'muted', ...props }: BadgeProps) {
  return (
    <View className={cn('self-start rounded-lg px-3 py-1', badgeTones[tone], className)} {...props}>
      <Text className={cn('text-xs font-semibold', textTones[tone])}>{label}</Text>
    </View>
  );
}
