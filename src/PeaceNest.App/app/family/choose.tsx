import { router } from 'expo-router';
import { KeyRound, Plus, RotateCcw } from 'lucide-react-native';
import { Pressable, View } from 'react-native';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';

export default function FamilyChoiceRoute() {
  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Find your family space</Text>
        <Text className="text-peacenest-muted">Create a new nest or join one waiting for you.</Text>
      </View>
      <View className="gap-3">
        <Choice icon={<Plus color="#D97C83" size={24} />} label="Create a family" onPress={() => router.push('/family/setup')} />
        <Choice icon={<KeyRound color="#B8755A" size={24} />} label="Join with family code" onPress={() => router.push('/family/join')} />
        <Choice icon={<RotateCcw color="#61715F" size={24} />} label="Recover a family" onPress={() => router.push('/family/recover')} />
      </View>
    </Screen>
  );
}

function Choice({ icon, label, onPress }: { icon: React.ReactNode; label: string; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="button" onPress={onPress}>
      <Card className="min-h-20 flex-row items-center gap-4">
        {icon}
        <Text className="flex-1 font-semibold">{label}</Text>
      </Card>
    </Pressable>
  );
}
