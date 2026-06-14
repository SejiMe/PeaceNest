import { Redirect, router } from 'expo-router';
import { useMemo, useState } from 'react';
import { Pressable, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { useWantsAndNeeds } from '@/hooks/use-wants-and-needs';
import { kindLabel, scoreLabel, WantNeedKind, type WantOrNeedResponse } from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';
import { cn } from '@/lib/utils';

type Filter = 'all' | 'needs' | 'wants';

const filters: Array<{ label: string; value: Filter }> = [
  { label: 'All', value: 'all' },
  { label: 'Needs', value: 'needs' },
  { label: 'Wants', value: 'wants' },
];

export default function WantsNeedsRoute() {
  const { session } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const wantsAndNeeds = useWantsAndNeeds(primaryFamily?.familyId);
  const [filter, setFilter] = useState<Filter>('all');

  const visibleItems = useMemo(() => {
    const items = wantsAndNeeds.data?.wantsAndNeeds ?? [];

    if (filter === 'needs') {
      return items.filter((item) => item.kind === WantNeedKind.Need);
    }

    if (filter === 'wants') {
      return items.filter((item) => item.kind === WantNeedKind.Want);
    }

    return items;
  }, [filter, wantsAndNeeds.data?.wantsAndNeeds]);

  if (!session) {
    return <Redirect href="/auth/sign-in" />;
  }

  if (currentUser.isLoading) {
    return <LoadingState />;
  }

  if (!primaryFamily) {
    return (
      <Screen>
        <EmptyState
          title="Start with your family space"
          message="Create a family workspace before adding shared wants and needs."
          actionLabel="Set up family"
          onAction={() => router.push('/family/setup')}
        />
      </Screen>
    );
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Wants & Needs</Text>
        <Text className="text-peacenest-muted">{primaryFamily.familyName}</Text>
      </View>

      <View className="flex-row gap-2">
        {filters.map((item) => (
          <Pressable
            key={item.value}
            accessibilityRole="button"
            className={cn(
              'rounded-lg border border-peacenest-border px-4 py-2',
              filter === item.value ? 'bg-peacenest-rose' : 'bg-peacenest-surface',
            )}
            onPress={() => setFilter(item.value)}
          >
            <Text className={cn('font-semibold', filter === item.value ? 'text-white' : 'text-peacenest-charcoal')}>
              {item.label}
            </Text>
          </Pressable>
        ))}
      </View>

      {wantsAndNeeds.isLoading ? <LoadingState title="Gathering family plans" /> : null}

      {wantsAndNeeds.isError ? (
        <ErrorState
          title="Plans could not open"
          message={wantsAndNeeds.error.message}
          actionLabel="Try again"
          onAction={() => wantsAndNeeds.refetch()}
        />
      ) : null}

      {!wantsAndNeeds.isLoading && visibleItems.length === 0 ? (
        <EmptyState
          title="Start with one family plan"
          message="Add a need, a want, or something your family wants to talk about together."
          actionLabel="Add first plan"
          onAction={() => router.push('/wants-needs/create')}
        />
      ) : null}

      <View className="gap-3">
        {visibleItems.map((item) => (
          <WantNeedCard key={item.id} item={item} />
        ))}
      </View>

      <Button label="Add plan" onPress={() => router.push('/wants-needs/create')} />
    </Screen>
  );
}

function WantNeedCard({ item }: { item: WantOrNeedResponse }) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => router.push({ pathname: '/wants-needs/[id]', params: { id: item.id } })}
    >
      <Card className="gap-3">
        <View className="flex-row items-start justify-between gap-3">
          <View className="flex-1 gap-1">
            <Text className="text-lg font-semibold">{item.title}</Text>
            {item.description ? <Text variant="caption">{item.description}</Text> : null}
          </View>
          <Badge label={kindLabel(item.kind)} tone={item.kind === WantNeedKind.Need ? 'need' : 'want'} />
        </View>

        <View className="flex-row flex-wrap gap-2">
          <Badge label={scoreLabel(item.urgencyLevel)} tone={item.urgencyLevel === 2 ? 'gold' : 'muted'} />
          {item.estimatedCostAmount ? (
            <Badge
              label={`${item.estimatedCostCurrency ?? 'USD'} ${Number(item.estimatedCostAmount).toLocaleString()}`}
              tone="muted"
            />
          ) : null}
        </View>

        <View className="gap-2">
          <View className="flex-row justify-between">
            <Text variant="caption">Progress</Text>
            <Text variant="caption">{item.progressPercent}%</Text>
          </View>
          <View className="h-2 overflow-hidden rounded-lg bg-peacenest-blush">
            <View className="h-full bg-peacenest-sage" style={{ width: `${item.progressPercent}%` }} />
          </View>
        </View>
      </Card>
    </Pressable>
  );
}
