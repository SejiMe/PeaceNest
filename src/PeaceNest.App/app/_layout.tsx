import '../global.css';

import { PortalHost } from '@rn-primitives/portal';
import { Stack } from 'expo-router';
import { RootProviders } from '@/lib/providers/root-providers';

export default function RootLayout() {
  return (
    <RootProviders>
      <Stack screenOptions={{ headerShown: false }}>
        <Stack.Screen name="index" />
        <Stack.Screen name="auth/sign-in" />
        <Stack.Screen name="dev/auth-token" />
        <Stack.Screen name="family/setup" />
        <Stack.Screen name="family/invite" />
        <Stack.Screen name="tabs" />
      </Stack>
      <PortalHost />
    </RootProviders>
  );
}
