import { View, type ViewProps } from 'react-native';
import { cn } from '@/lib/utils';

export function Card({ className, ...props }: ViewProps) {
  return (
    <View
      className={cn('rounded-lg border border-peacenest-border bg-peacenest-surface p-4', className)}
      {...props}
    />
  );
}
