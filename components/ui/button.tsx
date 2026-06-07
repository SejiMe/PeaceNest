import { Pressable, type PressableProps } from 'react-native';
import { cn } from '@/lib/utils';
import { Text } from './text';

type ButtonProps = PressableProps & {
  label: string;
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
};

const variants = {
  primary: 'bg-peacenest-rose',
  secondary: 'border border-peacenest-border bg-peacenest-surface',
  ghost: 'bg-transparent',
  danger: 'bg-peacenest-danger',
};

const labelVariants = {
  primary: 'text-white',
  secondary: 'text-peacenest-charcoal',
  ghost: 'text-peacenest-charcoal',
  danger: 'text-white',
};

export function Button({ className, disabled, label, variant = 'primary', ...props }: ButtonProps) {
  return (
    <Pressable
      className={cn(
        'min-h-12 items-center justify-center rounded-lg px-5',
        variants[variant],
        disabled && 'opacity-50',
        className,
      )}
      disabled={disabled}
      accessibilityRole="button"
      {...props}
    >
      <Text className={cn('font-semibold', labelVariants[variant])}>{label}</Text>
    </Pressable>
  );
}
