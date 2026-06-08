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

export type CurrentUserResponse = {
  id: string;
  supabaseUserId: string;
  email: string;
  displayName: string;
  avatarUrl?: string | null;
  timezone?: string | null;
  lastSeenAt?: string | null;
};

export type FamilyMembershipResponse = {
  familyId: string;
  familyName: string;
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
};

export type CreateFamilyResponse = FamilyWorkspaceResponse;

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

export function canCastPlanVotes(role: FamilyMemberRole) {
  return role !== FamilyMemberRole.Viewer;
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
