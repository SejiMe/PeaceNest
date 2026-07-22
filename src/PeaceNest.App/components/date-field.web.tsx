import { CalendarDays, X } from 'lucide-react-native';
import { Pressable, TextInput, View } from 'react-native';

export function DateField({ onChange, value }: { onChange: (value: string | null) => void; value: string | null }) {
  return (
    <View className="min-h-12 flex-row items-center gap-3 rounded-2xl border border-peacenest-border bg-peacenest-surface px-4">
      <CalendarDays color="#8A7D78" size={20} />
      <TextInput
        // React Native Web forwards this to an HTML date input.
        // @ts-expect-error Web-only input type.
        type="date"
        className="min-h-12 flex-1 text-base text-peacenest-charcoal"
        onChangeText={(nextValue) => onChange(nextValue || null)}
        value={value ?? ''}
      />
      {value ? (
        <Pressable accessibilityLabel="Clear date" onPress={() => onChange(null)}>
          <X color="#8A7D78" size={20} />
        </Pressable>
      ) : null}
    </View>
  );
}
