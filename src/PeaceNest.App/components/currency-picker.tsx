import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { supportedCurrencies, type SupportedCurrency } from '@/lib/api/contracts';
import { cn } from '@/lib/utils';

export function CurrencyPicker({ onChange, value }: { onChange: (value: SupportedCurrency) => void; value: SupportedCurrency }) {
  return (
    <View className="flex-row gap-2">
      {supportedCurrencies.map((currency) => (
        <Pressable
          key={currency}
          accessibilityRole="button"
          className={cn(
            'min-h-11 flex-1 items-center justify-center rounded-lg border px-3',
            value === currency
              ? 'border-peacenest-rose bg-peacenest-rose'
              : 'border-peacenest-border bg-peacenest-surface',
          )}
          onPress={() => onChange(currency)}
        >
          <Text className={cn('font-semibold', value === currency && 'text-white')}>{currency}</Text>
        </Pressable>
      ))}
    </View>
  );
}
