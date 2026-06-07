import { TextInput, type TextInputProps } from 'react-native';
import { cn } from '@/lib/utils';

export function Input({ className, placeholderTextColor = '#8A7D78', ...props }: TextInputProps) {
  return (
    <TextInput
      className={cn(
        'min-h-12 rounded-2xl border border-peacenest-border bg-peacenest-surface px-4 text-base text-peacenest-charcoal',
        className,
      )}
      placeholderTextColor={placeholderTextColor}
      {...props}
    />
  );
}
