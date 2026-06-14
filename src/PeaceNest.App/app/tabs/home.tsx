import { Redirect, router } from 'expo-router';
import { Pressable, View } from 'react-native';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Screen } from '@/components/ui/screen';
import { EmptyState, ErrorState, LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useMilestones } from '@/hooks/use-milestones';
import { useNotifications } from '@/hooks/use-notifications';
import { usePrimaryFamily } from '@/hooks/use-primary-family';
import { currentMonthlyRecapPeriod, useMonthlyRecap } from '@/hooks/use-recaps';
import { useWantsAndNeeds } from '@/hooks/use-wants-and-needs';
import { ApiError } from '@/lib/api/api-error';
import {
  kindLabel,
  milestoneProgress,
  notificationTypeLabel,
  PlanStatus,
  roleLabel,
  type FamilyMemberRole,
  type MilestoneResponse,
  type NotificationResponse,
  type WantOrNeedResponse,
} from '@/lib/api/contracts';
import { useAuth } from '@/lib/auth/auth-provider';

export default function HomeRoute() {
  const { session, signOut, user } = useAuth();
  const { currentUser, primaryFamily } = usePrimaryFamily();
  const wantsAndNeeds = useWantsAndNeeds(primaryFamily?.familyId);
  const milestones = useMilestones(primaryFamily?.familyId);
  const notifications = useNotifications(primaryFamily?.familyId);
  const period = currentMonthlyRecapPeriod();
  const monthlyRecap = useMonthlyRecap(primaryFamily?.familyId, period.year, period.month);
  const missingRecap = monthlyRecap.error instanceof ApiError && monthlyRecap.error.status === 404;

  if (!session) {
    return <Redirect href="/auth/sign-in" />;
  }

  if (currentUser.isLoading) {
    return <LoadingState />;
  }

  return (
    <Screen scroll>
      <View className="gap-2">
        <Text variant="title">Family nest</Text>
        <Text className="text-peacenest-muted">{user?.email}</Text>
      </View>

      {currentUser.isError ? (
        <ErrorState
          title="We could not open your nest"
          message={currentUser.error.message}
          actionLabel="Try again"
          onAction={() => currentUser.refetch()}
        />
      ) : null}

      {currentUser.data ? (
        currentUser.data.familyMemberships.length > 0 && primaryFamily ? (
          <FamilyDashboard
            isLoading={
              wantsAndNeeds.isLoading ||
              milestones.isLoading ||
              notifications.isLoading ||
              (monthlyRecap.isLoading && !missingRecap)
            }
            milestones={milestones.data?.milestones ?? []}
            notifications={notifications.data?.notifications ?? []}
            primaryFamily={primaryFamily}
            recapSummary={missingRecap ? null : monthlyRecap.data?.recap.summary ?? null}
            unreadCount={notifications.data?.unreadCount ?? 0}
            wantsAndNeeds={wantsAndNeeds.data?.wantsAndNeeds ?? []}
          />
        ) : (
          <EmptyState
            title="Start your family space"
            message="Create or join a family workspace so everyone can plan from one calm place."
            actionLabel="Set up family"
            onAction={() => router.push('/family/setup')}
          />
        )
      ) : null}

      {currentUser.data?.familyMemberships.length ? (
        <View className="gap-3">
          <Button label="Add family plan" onPress={() => router.push('/wants-needs/create')} />
          <Button label="Invite family" onPress={() => router.push('/family/invite')} variant="secondary" />
        </View>
      ) : null}

      <Button label="Sign out" onPress={signOut} variant="ghost" />
    </Screen>
  );
}

type FamilyDashboardProps = {
  isLoading: boolean;
  milestones: MilestoneResponse[];
  notifications: NotificationResponse[];
  primaryFamily: {
    familyId: string;
    familyName: string;
    role: FamilyMemberRole;
  };
  recapSummary: string | null;
  unreadCount: number;
  wantsAndNeeds: WantOrNeedResponse[];
};

function FamilyDashboard({
  isLoading,
  milestones,
  notifications,
  primaryFamily,
  recapSummary,
  unreadCount,
  wantsAndNeeds,
}: FamilyDashboardProps) {
  const activeWantsAndNeeds = wantsAndNeeds.filter((plan) => plan.status === PlanStatus.Active);
  const activeMilestones = milestones.filter((milestone) => milestone.status === PlanStatus.Active);
  const topPriorities = getTopPriorities(activeWantsAndNeeds, activeMilestones);
  const upcomingMilestone = getUpcomingMilestone(activeMilestones);
  const latestNotifications = notifications.slice(0, 3);
  const completedPlans = wantsAndNeeds.filter((plan) => plan.status === PlanStatus.Completed).length +
    milestones.filter((milestone) => milestone.status === PlanStatus.Completed).length;

  return (
    <View className="gap-5">
      <Card className="gap-4 bg-peacenest-blush">
        <View className="gap-1">
          <Text variant="section">{primaryFamily.familyName}</Text>
          <Text variant="caption">{roleLabel(primaryFamily.role)} in this family workspace</Text>
        </View>

        <View className="flex-row flex-wrap gap-3">
          <OverviewTile label="Active plans" value={activeWantsAndNeeds.length + activeMilestones.length} tone="sage" />
          <OverviewTile label="Peace wins" value={completedPlans} tone="gold" />
          <OverviewTile label="Unread" value={unreadCount} tone="blush" />
        </View>
      </Card>

      {isLoading ? <LoadingState title="Gathering your family table" /> : null}

      <DashboardSection
        actionLabel="Open board"
        onAction={() => router.push('/tabs/wants-needs')}
        title="Top Priorities"
      >
        {topPriorities.length === 0 ? (
          <Text variant="caption">Add a want, need, or milestone to start shaping family priorities.</Text>
        ) : (
          topPriorities.map((priority) => <PriorityRow key={priority.id} priority={priority} />)
        )}
      </DashboardSection>

      <DashboardSection
        actionLabel="Open milestones"
        onAction={() => router.push('/tabs/milestones')}
        title="Upcoming Milestone"
      >
        {upcomingMilestone ? (
          <MilestonePreview milestone={upcomingMilestone} />
        ) : (
          <Text variant="caption">Create a family milestone for habits, events, and shared growth.</Text>
        )}
      </DashboardSection>

      <DashboardSection
        actionLabel="Open recap"
        onAction={() => router.push('/tabs/recaps')}
        title="Monthly Recap"
      >
        {recapSummary ? (
          <View className="gap-2 rounded-lg bg-peacenest-goldLight p-3">
            <Badge label="Ready" tone="gold" />
            <Text>{recapSummary}</Text>
          </View>
        ) : (
          <Text variant="caption">Your recap is growing as your family completes plans, votes, and shares notes.</Text>
        )}
      </DashboardSection>

      <DashboardSection
        actionLabel="Open updates"
        onAction={() => router.push('/tabs/notifications')}
        title="Recent Activity"
      >
        {latestNotifications.length === 0 ? (
          <Text variant="caption">Family updates will appear here when there is something gentle to notice.</Text>
        ) : (
          latestNotifications.map((notification) => (
            <NotificationPreview key={notification.id} notification={notification} />
          ))
        )}
      </DashboardSection>
    </View>
  );
}

type OverviewTileProps = {
  label: string;
  tone: 'blush' | 'gold' | 'sage';
  value: number;
};

function OverviewTile({ label, tone, value }: OverviewTileProps) {
  const toneClass =
    tone === 'gold' ? 'bg-peacenest-goldLight' : tone === 'sage' ? 'bg-peacenest-sage/20' : 'bg-peacenest-surface';

  return (
    <View className={`min-w-24 flex-1 rounded-lg p-3 ${toneClass}`}>
      <Text className="text-2xl font-bold">{value}</Text>
      <Text variant="caption">{label}</Text>
    </View>
  );
}

type DashboardSectionProps = {
  actionLabel: string;
  children: React.ReactNode;
  onAction: () => void;
  title: string;
};

function DashboardSection({ actionLabel, children, onAction, title }: DashboardSectionProps) {
  return (
    <Card className="gap-3">
      <View className="flex-row items-center justify-between gap-3">
        <Text variant="section">{title}</Text>
        <Button className="min-h-10 px-3" label={actionLabel} onPress={onAction} variant="secondary" />
      </View>
      {children}
    </Card>
  );
}

type Priority = {
  id: string;
  kind: 'Milestone' | 'Need' | 'Want';
  priorityRank?: number | null;
  priorityScore: number;
  progressPercent: number;
  route: '/milestones/[id]' | '/wants-needs/[id]';
  title: string;
};

function getTopPriorities(wantsAndNeeds: WantOrNeedResponse[], milestones: MilestoneResponse[]) {
  const priorities: Priority[] = [
    ...wantsAndNeeds.map((plan) => ({
      id: plan.id,
      kind: kindLabel(plan.kind) as 'Need' | 'Want',
      priorityRank: plan.priorityRank,
      priorityScore: plan.priorityScore,
      progressPercent: plan.progressPercent,
      route: '/wants-needs/[id]' as const,
      title: plan.title,
    })),
    ...milestones.map((milestone) => ({
      id: milestone.id,
      kind: 'Milestone' as const,
      priorityRank: milestone.priorityRank,
      priorityScore: milestone.priorityScore,
      progressPercent: milestoneProgress(milestone).percent,
      route: '/milestones/[id]' as const,
      title: milestone.title,
    })),
  ];

  return priorities
    .sort((left, right) => {
      const leftRank = left.priorityRank ?? Number.MAX_SAFE_INTEGER;
      const rightRank = right.priorityRank ?? Number.MAX_SAFE_INTEGER;

      if (leftRank !== rightRank) {
        return leftRank - rightRank;
      }

      return right.priorityScore - left.priorityScore;
    })
    .slice(0, 3);
}

function PriorityRow({ priority }: { priority: Priority }) {
  const tone = priority.kind === 'Need' ? 'need' : priority.kind === 'Want' ? 'want' : 'gold';

  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => router.push({ pathname: priority.route, params: { id: priority.id } })}
    >
      <View className="gap-2 rounded-lg bg-peacenest-blush p-3">
        <View className="flex-row flex-wrap items-start justify-between gap-2">
          <View className="flex-1 gap-1">
            <Text className="font-semibold">{priority.title}</Text>
            <Text variant="caption">{priority.priorityRank ? `Priority #${priority.priorityRank}` : 'Priority signal'}</Text>
          </View>
          <Badge label={priority.kind} tone={tone} />
        </View>
        <View className="h-2 overflow-hidden rounded-lg bg-peacenest-surface">
          <View className="h-full bg-peacenest-sage" style={{ width: `${priority.progressPercent}%` }} />
        </View>
      </View>
    </Pressable>
  );
}

function getUpcomingMilestone(milestones: MilestoneResponse[]) {
  const today = new Date();
  today.setHours(0, 0, 0, 0);

  return milestones
    .filter((milestone) => {
      if (!milestone.targetDate) {
        return false;
      }

      return new Date(`${milestone.targetDate}T00:00:00`) >= today;
    })
    .sort((left, right) => String(left.targetDate).localeCompare(String(right.targetDate)))[0] ?? null;
}

function MilestonePreview({ milestone }: { milestone: MilestoneResponse }) {
  const progress = milestoneProgress(milestone);

  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => router.push({ pathname: '/milestones/[id]', params: { id: milestone.id } })}
    >
      <View className="gap-3 rounded-lg bg-peacenest-blush p-3">
        <View className="flex-row flex-wrap items-start justify-between gap-2">
          <View className="flex-1 gap-1">
            <Text className="font-semibold">{milestone.title}</Text>
            <Text variant="caption">
              {milestone.targetDate ? `Target ${formatDate(milestone.targetDate)}` : 'No target date yet'}
            </Text>
          </View>
          <Badge label="Milestone" tone="gold" />
        </View>
        <View className="h-2 overflow-hidden rounded-lg bg-peacenest-surface">
          <View className="h-full bg-peacenest-sage" style={{ width: `${progress.percent}%` }} />
        </View>
        <Text variant="caption">
          {progress.total > 0 ? `${progress.completed}/${progress.total} steps completed` : `${progress.percent}% complete`}
        </Text>
      </View>
    </Pressable>
  );
}

function NotificationPreview({ notification }: { notification: NotificationResponse }) {
  return (
    <View className="gap-1 rounded-lg bg-peacenest-blush p-3">
      <View className="flex-row flex-wrap items-start justify-between gap-2">
        <Text className="flex-1 font-semibold">{notification.title}</Text>
        <Badge label={notificationTypeLabel(notification.type)} tone={notification.readAt ? 'muted' : 'gold'} />
      </View>
      <Text variant="caption">
        {notification.actorDisplayName
          ? `${notification.actorDisplayName} - ${formatDateTime(notification.createdAt)}`
          : formatDateTime(notification.createdAt)}
      </Text>
    </View>
  );
}

function formatDate(value: string) {
  const date = new Date(`${value}T00:00:00`);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
  }).format(date);
}

function formatDateTime(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
  }).format(date);
}
