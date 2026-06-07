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
