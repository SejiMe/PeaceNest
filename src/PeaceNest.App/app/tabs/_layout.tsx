import { Tabs } from 'expo-router';
import { Bell, Flag, Heart, Home, ListChecks } from 'lucide-react-native';
import type { ColorValue } from 'react-native';
import { peaceNestColors } from '@/lib/theme/colors';

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: peaceNestColors.roseAccent,
        tabBarInactiveTintColor: peaceNestColors.mutedText,
        tabBarStyle: {
          backgroundColor: peaceNestColors.surface,
          borderTopColor: peaceNestColors.border,
        },
      }}
    >
      <Tabs.Screen name="home" options={{ title: 'Home', tabBarIcon: icon(Home) }} />
      <Tabs.Screen name="wants-needs" options={{ title: 'Plans', tabBarIcon: icon(ListChecks) }} />
      <Tabs.Screen name="milestones" options={{ title: 'Milestones', tabBarIcon: icon(Flag) }} />
      <Tabs.Screen name="recaps" options={{ title: 'Recaps', tabBarIcon: icon(Heart) }} />
      <Tabs.Screen name="notifications" options={{ title: 'Notices', tabBarIcon: icon(Bell) }} />
    </Tabs>
  );
}

function icon(Icon: typeof Home) {
  return function TabIcon({ color, size }: { color: ColorValue; size: number }) {
    return <Icon color={String(color)} size={size} strokeWidth={2} />;
  };
}
