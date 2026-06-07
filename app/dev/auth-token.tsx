import { router } from 'expo-router';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { Text } from '@/components/ui/text';
import { appEnv } from '@/lib/config/env';

export default function DevAuthTokenRoute() {
  if (!appEnv.enableDevAuthToken) {
    return (
      <Screen>
        <Card className="gap-3">
          <Text variant="section">Not available</Text>
          <Button label="Go home" onPress={() => router.replace('/')} />
        </Card>
      </Screen>
    );
  }

  const { DevAuthTokenPage } =
    require('@/features/dev-auth-token/dev-auth-token-page') as typeof import('@/features/dev-auth-token/dev-auth-token-page');

  return <DevAuthTokenPage />;
}
