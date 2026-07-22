import DateTimePicker from '@react-native-community/datetimepicker';
import { CalendarDays, X } from 'lucide-react-native';
import { useState } from 'react';
import { Platform, Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';

export function DateField({ onChange, value }: { onChange: (value: string | null) => void; value: string | null }) {
  const [isOpen, setIsOpen] = useState(false);
  const selectedDate = value ? new Date(`${value}T00:00:00`) : new Date();

  return (
    <View className="gap-2">
      <View className="min-h-12 flex-row items-center rounded-2xl border border-peacenest-border bg-peacenest-surface px-4">
        <Pressable className="flex-1 flex-row items-center gap-3" onPress={() => setIsOpen(true)}>
          <CalendarDays color="#8A7D78" size={20} />
          <Text className={value ? '' : 'text-peacenest-muted'}>
            {value ? selectedDate.toLocaleDateString() : 'Choose a date'}
          </Text>
        </Pressable>
        {value ? (
          <Pressable accessibilityLabel="Clear date" onPress={() => onChange(null)}>
            <X color="#8A7D78" size={20} />
          </Pressable>
        ) : null}
      </View>

      {isOpen ? (
        <DateTimePicker
          display={Platform.OS === 'ios' ? 'inline' : 'calendar'}
          mode="date"
          onChange={(_event, date) => {
            if (Platform.OS !== 'ios') setIsOpen(false);
            if (date) onChange(toDateOnly(date));
          }}
          value={selectedDate}
        />
      ) : null}
      {Platform.OS === 'ios' && isOpen ? (
        <Pressable className="items-center py-2" onPress={() => setIsOpen(false)}>
          <Text className="font-semibold text-peacenest-rose">Done</Text>
        </Pressable>
      ) : null}
    </View>
  );
}

function toDateOnly(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
