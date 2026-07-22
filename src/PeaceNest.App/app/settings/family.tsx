import { Redirect, router } from 'expo-router';
import { useEffect, useState } from 'react';
import { Alert, View } from 'react-native';
import { CurrencyPicker } from '@/components/currency-picker';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useLeaveFamily } from '@/hooks/use-family-departure';
import { useFamilyWorkspaces, useUpdatePreferredCurrency } from '@/hooks/use-family-workspaces';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { FamilyMemberRole, type SupportedCurrency } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';
import { setRecoveryCodeHandoff } from '@/lib/family-recovery/recovery-code-handoff';

export default function FamilySettingsRoute() {
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const families = useFamilyWorkspaces(Boolean(session));

  if (!session) return <Redirect href="/auth/sign-in" />;
  if (currentUser.isLoading || families.isLoading) return <LoadingState />;
  if (!primaryFamily) return <Redirect href="/family/choose" />;
  if (families.isError) {
    return <ErrorState title="Family settings could not load" message={families.error.message} actionLabel="Try again" onAction={() => families.refetch()} />;
  }

  const workspace = families.data?.families.find((family) => family.id === primaryFamily.familyId);
  if (!workspace) return <Redirect href="/family/choose" />;

  return <CurrencySettings family={{ ...primaryFamily, memberCount: workspace.memberCount }} />;
}

type FamilySettingsModel = NonNullable<ReturnType<typeof usePrimaryFamily>['primaryFamily']> & { memberCount: number };

function CurrencySettings({ family }: { family: FamilySettingsModel }) {
  const updateCurrency = useUpdatePreferredCurrency(family.familyId);
  const leaveFamily = useLeaveFamily(family.familyId);
  const [currency, setCurrency] = useState<SupportedCurrency>(family.preferredCurrency);
  const canManage = family.role === FamilyMemberRole.Owner || family.role === FamilyMemberRole.ParentAdmin;

  useEffect(() => setCurrency(family.preferredCurrency), [family.preferredCurrency]);

  async function handleSave() {
    try {
      await updateCurrency.mutateAsync({ preferredCurrency: currency });
      Alert.alert('Currency updated', 'New or edited estimates will use this preference. Existing estimates were not converted.');
    } catch (error) {
      Alert.alert('Currency could not update', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  function handleLeaveConfirmation() {
    const isSoleOwner = family.role === FamilyMemberRole.Owner && family.memberCount === 1;
    const message = isSoleOwner
      ? 'Your family workspace becomes inaccessible immediately. PeaceNest will permanently delete it and its family data after 30 days unless you restore it with the one-time recovery code.'
      : 'You will lose access to this family workspace. Your historical plans, notes, votes, and progress attribution remain with the family.';

    Alert.alert('Leave this family?', message, [
      { text: 'Stay', style: 'cancel' },
      {
        text: 'Leave family',
        style: 'destructive',
        onPress: () => void handleLeave(),
      },
    ]);
  }

  async function handleLeave() {
    try {
      const result = await leaveFamily.mutateAsync();
      if (result.recoveryAvailable && result.recoveryCode && result.recoveryExpiresAt) {
        setRecoveryCodeHandoff({
          familyId: result.familyId,
          recoveryCode: result.recoveryCode,
          recoveryExpiresAt: result.recoveryExpiresAt,
        });
        router.replace('/family/recovery-code');
        return;
      }

      router.replace('/family/choose');
    } catch (error) {
      Alert.alert('Family could not be left', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  const ownerNeedsTransfer = family.role === FamilyMemberRole.Owner && family.memberCount > 1;

  return (
    <Screen scroll>
      <View className="gap-2"><Text variant="title">Family settings</Text><Text className="text-peacenest-muted">{family.familyName}</Text></View>
      <Card className="gap-4">
        <Text variant="section">Preferred currency</Text>
        <Text variant="caption">This becomes the default for new estimates. PeaceNest never silently converts existing values.</Text>
        <CurrencyPicker onChange={setCurrency} value={currency} />
        {canManage ? <Button disabled={currency === family.preferredCurrency || updateCurrency.isPending} label={updateCurrency.isPending ? 'Saving' : 'Save currency'} onPress={handleSave} /> : <Text variant="caption">Only owners and parent/admins can change this setting.</Text>}
      </Card>
      <Card className="gap-3">
        <Text variant="section">Leave family</Text>
        <Text variant="caption">
          {ownerNeedsTransfer
            ? 'Transfer ownership before leaving so the family workspace never becomes ownerless.'
            : family.role === FamilyMemberRole.Owner
              ? 'As the sole creator, leaving starts the 30-day family recovery window.'
              : 'Your historical family contributions remain after you leave.'}
        </Text>
        <Button
          disabled={ownerNeedsTransfer || leaveFamily.isPending}
          label={leaveFamily.isPending ? 'Leaving family' : 'Leave family'}
          onPress={handleLeaveConfirmation}
          variant="danger"
        />
      </Card>
      <Button label="Back home" onPress={() => router.replace('/tabs/home')} variant="secondary" />
    </Screen>
  );
}
