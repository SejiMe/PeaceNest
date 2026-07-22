import { Redirect, router } from 'expo-router';
import { RefreshCw } from 'lucide-react-native';
import { useState } from 'react';
import { Alert, Pressable, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import {
  useCreateFamilyJoinRequest,
  useMyFamilyJoinRequests,
  useWithdrawFamilyJoinRequest,
} from '@/hooks/use-family-join-requests';
import {
  FamilyJoinRequestStatus,
  joinRequestStatusLabel,
  roleLabel,
  type FamilyJoinRequestResponse,
} from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function JoinFamilyRoute() {
  const { session } = useAuth();
  const requests = useMyFamilyJoinRequests(Boolean(session));
  const createRequest = useCreateFamilyJoinRequest();
  const withdrawRequest = useWithdrawFamilyJoinRequest();
  const [code, setCode] = useState('');

  if (!session) return <Redirect href="/auth/sign-in" />;

  async function handleRequest() {
    try {
      const result = await createRequest.mutateAsync(code);
      setCode('');
      Alert.alert(
        result.wasAlreadyPending ? 'Request already waiting' : 'Request sent',
        `${result.joinRequest.familyName} will review your request before you receive access.`,
      );
    } catch (error) {
      Alert.alert('Join request could not be sent', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleWithdraw(requestId: string) {
    try {
      await withdrawRequest.mutateAsync(requestId);
    } catch (error) {
      Alert.alert('Request could not be withdrawn', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  const normalizedLength = code.replace(/[-\s]/g, '').length;

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Join your family</Text>
        <Text className="text-peacenest-muted">Enter the temporary code shared by a family owner or parent/admin.</Text>
      </View>

      <Card className="gap-4">
        <View className="gap-2">
          <Text className="font-semibold">Family join code</Text>
          <Input
            autoCapitalize="characters"
            autoCorrect={false}
            maxLength={11}
            onChangeText={(value) => setCode(value.toUpperCase())}
            placeholder="ABCDE-FGHIJ"
            value={code}
          />
          <Text variant="caption">Submitting a code requests access. It never joins you automatically.</Text>
        </View>
        <Button disabled={normalizedLength !== 10 || createRequest.isPending} label={createRequest.isPending ? 'Sending request' : 'Request to join'} onPress={handleRequest} />
      </Card>

      <View className="gap-3">
        <View className="flex-row items-center justify-between gap-3">
          <Text variant="section">Your requests</Text>
          <Pressable accessibilityLabel="Refresh my join requests" accessibilityRole="button" onPress={() => requests.refetch()}>
            <RefreshCw color="#61715F" size={20} />
          </Pressable>
        </View>
        {requests.isLoading ? <LoadingState title="Checking your requests" /> : null}
        {requests.isError ? <ErrorState title="Requests could not load" message={requests.error.message} actionLabel="Try again" onAction={() => requests.refetch()} /> : null}
        {!requests.isLoading && (requests.data?.joinRequests.length ?? 0) === 0 ? (
          <EmptyState title="No join requests yet" message="A request will appear here after you submit a valid family code." />
        ) : null}
        {requests.data?.joinRequests.map((request) => (
          <MyRequestCard key={request.id} isWithdrawing={withdrawRequest.isPending} onWithdraw={handleWithdraw} request={request} />
        ))}
      </View>

      <Button label="Back" onPress={() => router.back()} variant="ghost" />
    </Screen>
  );
}

function MyRequestCard({
  isWithdrawing,
  onWithdraw,
  request,
}: {
  isWithdrawing: boolean;
  onWithdraw: (requestId: string) => void;
  request: FamilyJoinRequestResponse;
}) {
  const isPending = request.status === FamilyJoinRequestStatus.Pending;
  const tone = request.status === FamilyJoinRequestStatus.Approved ? 'sage' : isPending ? 'gold' : 'muted';

  return (
    <Card className="gap-3">
      <View className="flex-row flex-wrap items-start justify-between gap-3">
        <View className="flex-1 gap-1">
          <Text className="font-semibold">{request.familyName}</Text>
          <Text variant="caption">
            {request.status === FamilyJoinRequestStatus.Approved && request.approvedRole != null
              ? roleLabel(request.approvedRole)
              : `Requested ${formatDateTime(request.createdAt)}`}
          </Text>
        </View>
        <Badge label={joinRequestStatusLabel(request.status)} tone={tone} />
      </View>
      {isPending ? (
        <>
          <Text variant="caption">Review window ends {formatDateTime(request.expiresAt)}.</Text>
          <Button disabled={isWithdrawing} label={isWithdrawing ? 'Withdrawing' : 'Withdraw request'} onPress={() => onWithdraw(request.id)} variant="secondary" />
        </>
      ) : null}
      {request.status === FamilyJoinRequestStatus.Approved ? (
        <Button label="Open family" onPress={() => router.replace('/tabs/home')} />
      ) : null}
    </Card>
  );
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' }).format(new Date(value));
}
