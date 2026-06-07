import { Text as RNText, type TextProps as RNTextProps } from 'react-native';
import { cn } from '@/lib/utils';

type TextProps = RNTextProps & {
  variant?: 'title' | 'section' | 'body' | 'caption' | 'badge';
};

const variants = {
  title: 'text-3xl font-bold text-peacenest-charcoal',
  section: 'text-xl font-semibold text-peacenest-charcoal',
  body: 'text-base text-peacenest-charcoal',
  caption: 'text-sm text-peacenest-muted',
  badge: 'text-xs font-semibold',
};

export function Text({ className, variant = 'body', ...props }: TextProps) {
  return <RNText className={cn(variants[variant], className)} {...props} />;
}
