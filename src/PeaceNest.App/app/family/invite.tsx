import * as Clipboard from 'expo-clipboard';
import { Redirect, router } from 'expo-router';
import { Copy, RefreshCw, Share2, ShieldCheck, UserRound } from 'lucide-react-native';
import { useEffect, useMemo, useState } from 'react';
import { Alert, Image, Pressable, Share, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import {
  useApproveFamilyJoinRequest,
  useFamilyJoinCode,
  useFamilyJoinRequests,
  useGenerateFamilyJoinCode,
  useRejectFamilyJoinRequest,
  useRevokeFamilyJoinCode,
} from '@/hooks/use-family-join-requests';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import {
  FamilyMemberRole,
  roleLabel,
  type FamilyJoinRequestResponse,
  type FamilyMemberRole as FamilyMemberRoleValue,
  type GenerateFamilyJoinCodeResponse,
} from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';
import { cn } from '@/lib/utils';

export default function FamilyInviteRoute() {
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();

  if (!session) return <Redirect href="/auth/sign-in" />;
  if (currentUser.isLoading) return <LoadingState />;
  if (!primaryFamily) return <Redirect href="/family/choose" />;

  const canManage = primaryFamily.role === FamilyMemberRole.Owner || primaryFamily.role === FamilyMemberRole.ParentAdmin;
  if (!canManage) {
    return (
      <Screen>
        <EmptyState title="Family access is protected" message="Only owners and parent/admins can manage join codes and requests." />
        <Button label="Back home" onPress={() => router.replace('/tabs/home')} variant="secondary" />
      </Screen>
    );
  }

  return <JoinCodeManager family={primaryFamily} />;
}

function JoinCodeManager({ family }: { family: NonNullable<ReturnType<typeof usePrimaryFamily>['primaryFamily']> }) {
  const joinCode = useFamilyJoinCode(family.familyId);
  const requests = useFamilyJoinRequests(family.familyId);
  const generateCode = useGenerateFamilyJoinCode(family.familyId);
  const revokeCode = useRevokeFamilyJoinCode(family.familyId);
  const [revealedCode, setRevealedCode] = useState<GenerateFamilyJoinCodeResponse | null>(null);
  const activeMetadata = revealedCode ?? (joinCode.data?.hasActiveCode ? joinCode.data : null);
  const countdown = useCountdown(activeMetadata?.expiresAt);

  async function handleGenerate() {
    try {
      setRevealedCode(await generateCode.mutateAsync());
    } catch (error) {
      Alert.alert('Join code could not be generated', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleRevoke() {
    try {
      await revokeCode.mutateAsync();
      setRevealedCode(null);
    } catch (error) {
      Alert.alert('Join code could not be revoked', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleShare() {
    if (!revealedCode) return;
    await Share.share({
      message: `Ask to join ${family.familyName} in PeaceNest with this temporary code: ${revealedCode.code}\nIt expires ${formatDateTime(revealedCode.expiresAt)}.`,
    });
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Family access</Text>
        <Text className="text-peacenest-muted">{family.familyName}</Text>
      </View>

      <Card className="gap-4 bg-peacenest-blush">
        <View className="flex-row items-start gap-3">
          <ShieldCheck color="#61715F" size={24} />
          <View className="flex-1 gap-1">
            <Text variant="section">Temporary join code</Text>
            <Text variant="caption">Anyone with the code may request access. You still approve every person and role.</Text>
          </View>
        </View>

        {joinCode.isLoading ? <LoadingState title="Checking family access" /> : null}
        {joinCode.isError ? <ErrorState title="Join-code status could not load" message={joinCode.error.message} actionLabel="Try again" onAction={() => joinCode.refetch()} /> : null}

        {activeMetadata ? (
          <View className="gap-3 rounded-lg border border-peacenest-border bg-peacenest-surface p-4">
            <View className="flex-row flex-wrap items-center justify-between gap-2">
              <Badge label={countdown.expired ? 'Expired' : countdown.label} tone={countdown.expired ? 'muted' : 'gold'} />
              <Text variant="caption">{activeMetadata.requestCount ?? 0}/{activeMetadata.maxRequests ?? 10} requests</Text>
            </View>
            {revealedCode ? (
              <>
                <Text className="text-center text-2xl font-bold">{revealedCode.code}</Text>
                <Text variant="caption">This plaintext code is shown only now. Rotate it if you lose it.</Text>
                <View className="flex-row gap-2">
                  <IconAction icon={<Copy color="#2F2A28" size={20} />} label="Copy" onPress={() => Clipboard.setStringAsync(revealedCode.code)} />
                  <IconAction icon={<Share2 color="#2F2A28" size={20} />} label="Share" onPress={handleShare} />
                </View>
              </>
            ) : (
              <Text variant="caption">An active code exists, but its plaintext cannot be retrieved. Rotate it to reveal a new one.</Text>
            )}
            <View className="flex-row gap-2">
              <Button className="flex-1" label="Rotate code" onPress={handleGenerate} variant="secondary" />
              <Button className="flex-1" label="Revoke" onPress={handleRevoke} variant="danger" />
            </View>
          </View>
        ) : !joinCode.isLoading ? (
          <Button disabled={generateCode.isPending} label={generateCode.isPending ? 'Generating' : 'Generate join code'} onPress={handleGenerate} />
        ) : null}
      </Card>

      <View className="gap-3">
        <View className="flex-row items-center justify-between gap-3">
          <Text variant="section">Pending requests</Text>
          <Pressable accessibilityLabel="Refresh join requests" accessibilityRole="button" onPress={() => requests.refetch()}>
            <RefreshCw color="#61715F" size={20} />
          </Pressable>
        </View>
        {requests.isLoading ? <LoadingState title="Gathering requests" /> : null}
        {requests.isError ? <ErrorState title="Requests could not load" message={requests.error.message} actionLabel="Try again" onAction={() => requests.refetch()} /> : null}
        {!requests.isLoading && (requests.data?.joinRequests.length ?? 0) === 0 ? (
          <EmptyState title="No one is waiting" message="New requests will appear here after someone enters the active code." />
        ) : null}
        {requests.data?.joinRequests.map((request) => (
          <JoinRequestCard key={request.id} familyRole={family.role} request={request} />
        ))}
      </View>

      <Button label="Back home" onPress={() => router.replace('/tabs/home')} variant="secondary" />
    </Screen>
  );
}

function JoinRequestCard({ familyRole, request }: { familyRole: FamilyMemberRoleValue; request: FamilyJoinRequestResponse }) {
  const approve = useApproveFamilyJoinRequest(request.familyId);
  const reject = useRejectFamilyJoinRequest(request.familyId);
  const options = useMemo(() => {
    const base = [FamilyMemberRole.AdultMember, FamilyMemberRole.ChildMember, FamilyMemberRole.Viewer];
    return familyRole === FamilyMemberRole.Owner ? [FamilyMemberRole.ParentAdmin, ...base] : base;
  }, [familyRole]);
  const [role, setRole] = useState<FamilyMemberRoleValue>(FamilyMemberRole.AdultMember);

  async function handleApprove() {
    try {
      await approve.mutateAsync({ requestId: request.id, role });
    } catch (error) {
      Alert.alert('Request could not be approved', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleReject() {
    try {
      await reject.mutateAsync(request.id);
    } catch (error) {
      Alert.alert('Request could not be updated', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Card className="gap-4">
      <View className="flex-row items-center gap-3">
        {request.requesterAvatarUrl ? (
          <Image className="h-11 w-11 rounded-full" source={{ uri: request.requesterAvatarUrl }} />
        ) : (
          <View className="h-11 w-11 items-center justify-center rounded-full bg-peacenest-sage/20"><UserRound color="#61715F" size={22} /></View>
        )}
        <View className="flex-1 gap-1">
          <Text className="font-semibold">{request.requesterDisplayName}</Text>
          <Text variant="caption">{request.maskedRequesterEmail}</Text>
        </View>
        <Badge label="Pending" tone="gold" />
      </View>
      <Text variant="caption">Review by {formatDateTime(request.expiresAt)}</Text>
      <View className="flex-row flex-wrap gap-2">
        {options.map((option) => (
          <Pressable key={option} className={cn('min-h-11 flex-1 items-center justify-center rounded-lg border px-2', role === option ? 'border-peacenest-sage bg-peacenest-sage/20' : 'border-peacenest-border bg-peacenest-surface')} onPress={() => setRole(option)}>
            <Text className="text-center text-sm font-semibold">{roleLabel(option)}</Text>
          </Pressable>
        ))}
      </View>
      <View className="flex-row gap-2">
        <Button className="flex-1" disabled={approve.isPending || reject.isPending} label="Approve" onPress={handleApprove} />
        <Button className="flex-1" disabled={approve.isPending || reject.isPending} label="Reject" onPress={handleReject} variant="danger" />
      </View>
    </Card>
  );
}

function IconAction({ icon, label, onPress }: { icon: React.ReactNode; label: string; onPress: () => void }) {
  return <Pressable className="min-h-12 flex-1 flex-row items-center justify-center gap-2 rounded-lg border border-peacenest-border bg-peacenest-surface px-3" onPress={onPress}>{icon}<Text className="font-semibold">{label}</Text></Pressable>;
}

function useCountdown(expiresAt?: string | null) {
  const [now, setNow] = useState(Date.now());
  useEffect(() => {
    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, []);

  if (!expiresAt) return { expired: true, label: 'No active code' };
  const seconds = Math.max(0, Math.floor((new Date(expiresAt).getTime() - now) / 1000));
  return {
    expired: seconds <= 0,
    label: `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')} remaining`,
  };
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat(undefined, { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' }).format(new Date(value));
}
