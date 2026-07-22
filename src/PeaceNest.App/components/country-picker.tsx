import { getData } from 'country-list';
import { ChevronDown, Search, X } from 'lucide-react-native';
import { useMemo, useState } from 'react';
import { FlatList, Modal, Pressable, View } from 'react-native';
import { Input } from '@/components/ui/input';
import { Text } from '@/components/ui/text';

const countries = getData().sort((left, right) => left.name.localeCompare(right.name));

export function CountryPicker({ onChange, value }: { onChange: (countryCode: string) => void; value: string }) {
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');
  const selected = countries.find((country) => country.code === value);
  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase();
    return query
      ? countries.filter((country) =>
          country.name.toLowerCase().includes(query) || country.code.toLowerCase().includes(query),
        )
      : countries;
  }, [search]);

  return (
    <>
      <Pressable
        accessibilityRole="button"
        className="min-h-12 flex-row items-center justify-between rounded-2xl border border-peacenest-border bg-peacenest-surface px-4"
        onPress={() => setIsOpen(true)}
      >
        <Text className={selected ? '' : 'text-peacenest-muted'}>{selected?.name ?? 'Select country or region'}</Text>
        <ChevronDown color="#8A7D78" size={20} />
      </Pressable>

      <Modal animationType="slide" onRequestClose={() => setIsOpen(false)} transparent visible={isOpen}>
        <View className="flex-1 justify-end bg-black/30">
          <View className="max-h-[85%] min-h-[70%] gap-4 rounded-t-3xl bg-peacenest-background px-5 pb-6 pt-5">
            <View className="flex-row items-center justify-between">
              <Text variant="section">Country or region</Text>
              <Pressable accessibilityLabel="Close country picker" onPress={() => setIsOpen(false)}>
                <X color="#2F2A28" size={24} />
              </Pressable>
            </View>

            <View className="flex-row items-center gap-2">
              <Search color="#8A7D78" size={20} />
              <Input className="flex-1" onChangeText={setSearch} placeholder="Search countries" value={search} />
            </View>

            <FlatList
              data={filtered}
              keyExtractor={(country) => country.code}
              keyboardShouldPersistTaps="handled"
              renderItem={({ item }) => (
                <Pressable
                  className="min-h-12 justify-center border-b border-peacenest-border px-2"
                  onPress={() => {
                    onChange(item.code);
                    setSearch('');
                    setIsOpen(false);
                  }}
                >
                  <Text className={item.code === value ? 'font-semibold text-peacenest-rose' : ''}>
                    {item.name}
                  </Text>
                </Pressable>
              )}
            />
          </View>
        </View>
      </Modal>
    </>
  );
}
