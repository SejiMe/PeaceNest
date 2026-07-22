export const FamilyMemberRole = {
  Owner: 0,
  ParentAdmin: 1,
  AdultMember: 2,
  ChildMember: 3,
  Viewer: 4,
} as const;

export type FamilyMemberRole = (typeof FamilyMemberRole)[keyof typeof FamilyMemberRole];

export const FamilyMemberStatus = {
  Active: 0,
  Removed: 1,
} as const;

export type FamilyMemberStatus = (typeof FamilyMemberStatus)[keyof typeof FamilyMemberStatus];

export const WantNeedKind = {
  Need: 0,
  Want: 1,
} as const;

export type WantNeedKind = (typeof WantNeedKind)[keyof typeof WantNeedKind];

export const PlanStatus = {
  Active: 0,
  Completed: 1,
  Archived: 2,
} as const;

export type PlanStatus = (typeof PlanStatus)[keyof typeof PlanStatus];

export const ScoreLevel = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const;

export type ScoreLevel = (typeof ScoreLevel)[keyof typeof ScoreLevel];

export const VoteValue = {
  Support: 0,
  Neutral: 1,
  NotNow: 2,
} as const;

export type VoteValue = (typeof VoteValue)[keyof typeof VoteValue];

export const NotificationType = {
  FamilyPlanCreated: 0,
  PlanUpdated: 1,
  CommentAdded: 2,
  VoteCast: 3,
  MilestoneCompleted: 4,
  MonthlyRecapReady: 5,
  FamilyJoinRequestCreated: 6,
  FamilyJoinRequestApproved: 7,
  FamilyJoinRequestRejected: 8,
} as const;

export type NotificationType = (typeof NotificationType)[keyof typeof NotificationType];

export const FamilyJoinCodeStatus = {
  Active: 0,
  Revoked: 1,
  Expired: 2,
  CapacityReached: 3,
} as const;

export type FamilyJoinCodeStatus = (typeof FamilyJoinCodeStatus)[keyof typeof FamilyJoinCodeStatus];

export const FamilyJoinRequestStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
  Withdrawn: 3,
  Expired: 4,
  Cancelled: 5,
} as const;

export type FamilyJoinRequestStatus = (typeof FamilyJoinRequestStatus)[keyof typeof FamilyJoinRequestStatus];

export const RecapPeriodType = {
  Monthly: 0,
} as const;

export type RecapPeriodType = (typeof RecapPeriodType)[keyof typeof RecapPeriodType];

export type CurrentUserResponse = {
  id: string;
  supabaseUserId: string;
  email: string;
  displayName: string;
  countryCode?: string | null;
  onboardingCompletedAt?: string | null;
  avatarUrl?: string | null;
  timezone?: string | null;
  lastSeenAt?: string | null;
};

export type FamilyMembershipResponse = {
  familyId: string;
  familyName: string;
  preferredCurrency: SupportedCurrency;
  role: FamilyMemberRole;
  status: FamilyMemberStatus;
};

export type GetMeResponse = {
  user: CurrentUserResponse;
  familyMemberships: FamilyMembershipResponse[];
};

export type FamilyWorkspaceResponse = {
  id: string;
  name: string;
  description?: string | null;
  preferredCurrency: SupportedCurrency;
  currentUserRole: FamilyMemberRole;
  memberCount: number;
  createdAt: string;
};

export type ListFamiliesResponse = {
  families: FamilyWorkspaceResponse[];
};

export type CreateFamilyRequest = {
  name: string;
  description?: string | null;
  preferredCurrency: SupportedCurrency;
};

export type CreateFamilyResponse = FamilyWorkspaceResponse;

export type UpdatePreferredCurrencyRequest = {
  preferredCurrency: SupportedCurrency;
};

export type UpdatePreferredCurrencyResponse = {
  familyId: string;
  preferredCurrency: SupportedCurrency;
};

export type WantOrNeedResponse = {
  id: string;
  familyId: string;
  createdByUserId: string;
  kind: WantNeedKind;
  title: string;
  description?: string | null;
  status: PlanStatus;
  priorityRank?: number | null;
  priorityScore: number;
  progressPercent: number;
  estimatedCostAmount?: number | null;
  estimatedCostCurrency?: string | null;
  urgencyLevel: ScoreLevel;
  importanceLevel: ScoreLevel;
  emotionalValueLevel: ScoreLevel;
  desiredByDate?: string | null;
  targetDate?: string | null;
  version: number;
  createdAt: string;
  updatedAt: string;
};

export type ListWantsAndNeedsResponse = {
  wantsAndNeeds: WantOrNeedResponse[];
};

export type CreateWantOrNeedRequest = {
  kind: WantNeedKind;
  title: string;
  description?: string | null;
  priorityRank?: number | null;
  progressPercent: number;
  estimatedCostAmount?: number | null;
  estimatedCostCurrency?: string | null;
  urgencyLevel: ScoreLevel;
  importanceLevel: ScoreLevel;
  emotionalValueLevel: ScoreLevel;
  desiredByDate?: string | null;
  targetDate?: string | null;
};

export type CreateWantOrNeedResponse = {
  wantOrNeed: WantOrNeedResponse;
};

export type GetWantOrNeedResponse = {
  wantOrNeed: WantOrNeedResponse;
};

export type UpdateWantOrNeedRequest = Omit<CreateWantOrNeedRequest, 'progressPercent'> & {
  version: number;
};

export type UpdateWantOrNeedResponse = {
  wantOrNeed: WantOrNeedResponse;
};

export type MilestoneStepResponse = {
  id: string;
  title: string;
  description?: string | null;
  sortOrder: number;
  isCompleted: boolean;
  completedByUserId?: string | null;
  completedAt?: string | null;
};

export type MilestoneResponse = {
  id: string;
  familyId: string;
  createdByUserId: string;
  title: string;
  description?: string | null;
  status: PlanStatus;
  priorityRank?: number | null;
  priorityScore: number;
  progressPercent: number;
  targetDate?: string | null;
  milestoneType?: string | null;
  celebrationNotes?: string | null;
  reflectionPrompt?: string | null;
  includeInRecap: boolean;
  steps: MilestoneStepResponse[];
  version: number;
  createdAt: string;
  updatedAt: string;
};

export type ListMilestonesResponse = {
  milestones: MilestoneResponse[];
};

export type CreateMilestoneStepRequest = {
  title: string;
  description?: string | null;
  sortOrder?: number | null;
};

export type CreateMilestoneRequest = {
  title: string;
  description?: string | null;
  priorityRank?: number | null;
  progressPercent: number;
  targetDate?: string | null;
  milestoneType?: string | null;
  celebrationNotes?: string | null;
  reflectionPrompt?: string | null;
  includeInRecap: boolean;
  steps: CreateMilestoneStepRequest[];
};

export type CreateMilestoneResponse = {
  milestone: MilestoneResponse;
};

export type GetMilestoneResponse = {
  milestone: MilestoneResponse;
};

export type UpdateMilestoneStepRequest = {
  id?: string | null;
  title: string;
  description?: string | null;
  sortOrder: number;
};

export type UpdateMilestoneRequest = Omit<CreateMilestoneRequest, 'progressPercent' | 'steps'> & {
  steps: UpdateMilestoneStepRequest[];
  version: number;
};

export type UpdateMilestoneResponse = {
  milestone: MilestoneResponse;
};

export type CompleteProfileRequest = {
  displayName: string;
  countryCode: string;
};

export type CompleteProfileResponse = {
  id: string;
  email: string;
  displayName: string;
  countryCode: string;
  onboardingCompletedAt: string;
};

export type AcceptInvitationRequest = {
  invitationToken?: string | null;
  invitationCode?: string | null;
};

export type AcceptInvitationResponse = {
  invitationId: string;
  familyId: string;
  familyName: string;
  role: FamilyMemberRole;
  membershipStatus: FamilyMemberStatus;
  acceptedAt: string;
};

export type CreateInvitationRequest = {
  invitedEmail: string;
  invitedRole: FamilyMemberRole;
};

export type CreateInvitationResponse = {
  id: string;
  familyId: string;
  invitedEmail: string;
  invitedRole: FamilyMemberRole;
  status: number;
  expiresAt: string;
  invitationToken: string;
  invitationCode: string;
};

export type GenerateFamilyJoinCodeResponse = {
  id: string;
  code: string;
  status: FamilyJoinCodeStatus;
  requestCount: number;
  maxRequests: number;
  createdAt: string;
  expiresAt: string;
};

export type GetFamilyJoinCodeResponse = {
  hasActiveCode: boolean;
  id?: string | null;
  status?: FamilyJoinCodeStatus | null;
  requestCount?: number | null;
  maxRequests?: number | null;
  createdAt?: string | null;
  expiresAt?: string | null;
};

export type FamilyJoinRequestResponse = {
  id: string;
  familyId: string;
  familyName: string;
  requesterDisplayName: string;
  maskedRequesterEmail: string;
  requesterAvatarUrl?: string | null;
  status: FamilyJoinRequestStatus;
  approvedRole?: FamilyMemberRole | null;
  createdAt: string;
  expiresAt: string;
  reviewedAt?: string | null;
};

export type CreateFamilyJoinRequestResponse = {
  joinRequest: FamilyJoinRequestResponse;
  wasAlreadyPending: boolean;
};

export type ListFamilyJoinRequestsResponse = {
  joinRequests: FamilyJoinRequestResponse[];
};

export type FamilyJoinRequestActionResponse = {
  joinRequest: FamilyJoinRequestResponse;
};

export type LeaveFamilyResponse = {
  familyId: string;
  recoveryAvailable: boolean;
  recoveryCode?: string | null;
  recoveryExpiresAt?: string | null;
};

export type RecoverFamilyRequest = {
  code: string;
};

export type RecoverFamilyResponse = {
  familyId: string;
  familyName: string;
  preferredCurrency: SupportedCurrency;
  role: FamilyMemberRole;
  recoveredAt: string;
};

export const supportedCurrencies = ['PHP', 'SGD', 'USD'] as const;
export type SupportedCurrency = (typeof supportedCurrencies)[number];

export function suggestedCurrency(countryCode?: string | null): SupportedCurrency {
  if (countryCode === 'SG') return 'SGD';
  if (countryCode === 'US') return 'USD';
  return 'PHP';
}

export function formatEstimatedCost(amount: number, currency?: string | null) {
  const resolvedCurrency = currency ?? 'PHP';
  try {
    return new Intl.NumberFormat(undefined, { style: 'currency', currency: resolvedCurrency }).format(amount);
  } catch {
    return `${resolvedCurrency} ${amount.toLocaleString()}`;
  }
}

export type PlanNoteResponse = {
  id: string;
  planId: string;
  authorUserId: string;
  authorDisplayName: string;
  body: string;
  createdAt: string;
  updatedAt: string;
};

export type ListPlanNotesResponse = {
  notes: PlanNoteResponse[];
};

export type AddPlanNoteRequest = {
  body: string;
};

export type AddPlanNoteResponse = {
  note: PlanNoteResponse;
};

export type PlanVoteResponse = {
  id: string;
  planId: string;
  userId: string;
  userDisplayName: string;
  voteValue: VoteValue;
  priorityPoints: number;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
};

export type PlanVoteSummaryResponse = {
  planId: string;
  totalVotes: number;
  supportCount: number;
  neutralCount: number;
  notNowCount: number;
  totalPriorityPoints: number;
  votes: PlanVoteResponse[];
};

export type ListPlanVotesResponse = {
  voteSummary: PlanVoteSummaryResponse;
};

export type CastPlanVoteRequest = {
  voteValue: VoteValue;
  priorityPoints: number;
  note?: string | null;
};

export type CastPlanVoteResponse = {
  vote: PlanVoteResponse;
};

export type NotificationResponse = {
  id: string;
  familyId: string;
  recipientUserId: string;
  actorUserId?: string | null;
  actorDisplayName?: string | null;
  type: NotificationType;
  title: string;
  body?: string | null;
  relatedPlanId?: string | null;
  relatedCommentId?: string | null;
  relatedRecapId?: string | null;
  relatedJoinRequestId?: string | null;
  readAt?: string | null;
  createdAt: string;
};

export type ListNotificationsResponse = {
  notifications: NotificationResponse[];
  unreadCount: number;
};

export type MarkNotificationReadResponse = {
  notification: NotificationResponse;
};

export type PlanActionResponse = {
  planId: string;
  familyId: string;
  status: PlanStatus;
  progressPercent: number;
  completedAt?: string | null;
  archivedAt?: string | null;
  updatedAt: string;
};

export type UpdatePlanProgressRequest = {
  progressPercent: number;
};

export type UpdatePlanProgressResponse = {
  plan: PlanActionResponse;
};

export type CompletePlanResponse = {
  plan: PlanActionResponse;
};

export type ArchivePlanResponse = {
  plan: PlanActionResponse;
};

export type UpdateMilestoneStepCompletionRequest = {
  isCompleted: boolean;
};

export type UpdateMilestoneStepCompletionResponse = {
  milestone: MilestoneResponse;
};

export type MonthlyRecapStatsResponse = {
  totalPlans: number;
  activePlans: number;
  newPlans: number;
  completedPlans: number;
  completedMilestones: number;
  delayedPlans: number;
  notesAdded: number;
  votesCast: number;
};

export type RecapItemResponse = {
  id: string;
  planId?: string | null;
  memoryId?: string | null;
  itemType: string;
  title: string;
  description?: string | null;
  sortOrder: number;
  createdAt: string;
};

export type MonthlyRecapResponse = {
  id: string;
  familyId: string;
  periodType: RecapPeriodType;
  periodStart: string;
  periodEnd: string;
  title: string;
  summary?: string | null;
  stats: MonthlyRecapStatsResponse;
  generatedByUserId: string;
  publishedAt?: string | null;
  createdAt: string;
  updatedAt: string;
  items: RecapItemResponse[];
};

export type GetMonthlyRecapResponse = {
  recap: MonthlyRecapResponse;
};

export type GenerateMonthlyRecapResponse = {
  recap: MonthlyRecapResponse;
};

export function roleLabel(role: FamilyMemberRole) {
  switch (role) {
    case FamilyMemberRole.Owner:
      return 'Owner';
    case FamilyMemberRole.ParentAdmin:
      return 'Parent';
    case FamilyMemberRole.AdultMember:
      return 'Adult member';
    case FamilyMemberRole.ChildMember:
      return 'Child member';
    case FamilyMemberRole.Viewer:
      return 'Viewer';
    default:
      return 'Family member';
  }
}

export function notificationTypeLabel(type: NotificationType) {
  switch (type) {
    case NotificationType.FamilyPlanCreated:
      return 'Plan created';
    case NotificationType.PlanUpdated:
      return 'Plan updated';
    case NotificationType.CommentAdded:
      return 'New note';
    case NotificationType.VoteCast:
      return 'Vote shared';
    case NotificationType.MilestoneCompleted:
      return 'Milestone completed';
    case NotificationType.MonthlyRecapReady:
      return 'Recap ready';
    case NotificationType.FamilyJoinRequestCreated:
      return 'Join request';
    case NotificationType.FamilyJoinRequestApproved:
      return 'Request approved';
    case NotificationType.FamilyJoinRequestRejected:
      return 'Request updated';
    default:
      return 'Family update';
  }
}

export function joinRequestStatusLabel(status: FamilyJoinRequestStatus) {
  switch (status) {
    case FamilyJoinRequestStatus.Pending:
      return 'Pending review';
    case FamilyJoinRequestStatus.Approved:
      return 'Approved';
    case FamilyJoinRequestStatus.Rejected:
      return 'Not approved';
    case FamilyJoinRequestStatus.Withdrawn:
      return 'Withdrawn';
    case FamilyJoinRequestStatus.Expired:
      return 'Expired';
    case FamilyJoinRequestStatus.Cancelled:
      return 'Cancelled';
    default:
      return 'Updated';
  }
}

export function recapItemLabel(itemType: string) {
  switch (itemType) {
    case 'completed_plan':
      return 'Peace win';
    case 'still_growing':
      return 'Still growing';
    default:
      return 'Family moment';
  }
}

export function planStatusLabel(status: PlanStatus) {
  switch (status) {
    case PlanStatus.Completed:
      return 'Completed';
    case PlanStatus.Archived:
      return 'Archived';
    default:
      return 'Active';
  }
}

export function canCastPlanVotes(role: FamilyMemberRole) {
  return role !== FamilyMemberRole.Viewer;
}

export function canUpdateFamilyPlans(role: FamilyMemberRole) {
  return role === FamilyMemberRole.Owner || role === FamilyMemberRole.ParentAdmin || role === FamilyMemberRole.AdultMember;
}

export function voteValueLabel(voteValue: VoteValue) {
  switch (voteValue) {
    case VoteValue.Support:
      return 'Support';
    case VoteValue.Neutral:
      return 'Neutral';
    case VoteValue.NotNow:
      return 'Not now';
    default:
      return 'Vote';
  }
}

export function milestoneProgress(milestone: MilestoneResponse) {
  if (milestone.steps.length === 0) {
    return {
      completed: 0,
      total: 0,
      percent: milestone.progressPercent,
    };
  }

  const completed = milestone.steps.filter((step) => step.isCompleted).length;

  return {
    completed,
    total: milestone.steps.length,
    percent: Math.round((completed / milestone.steps.length) * 100),
  };
}

export function kindLabel(kind: WantNeedKind) {
  return kind === WantNeedKind.Need ? 'Need' : 'Want';
}

export function scoreLabel(score: ScoreLevel) {
  switch (score) {
    case ScoreLevel.High:
      return 'Now';
    case ScoreLevel.Medium:
      return 'Soon';
    default:
      return 'Someday';
  }
}
