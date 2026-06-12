export const queryKeys = {
  authMe: ['auth', 'me'] as const,
  families: ['families'] as const,
  wantsAndNeeds: (familyId: string) => ['families', familyId, 'wants-needs'] as const,
  wantOrNeed: (familyId: string, planId: string) => ['families', familyId, 'wants-needs', planId] as const,
  milestones: (familyId: string) => ['families', familyId, 'milestones'] as const,
  milestone: (familyId: string, planId: string) => ['families', familyId, 'milestones', planId] as const,
  planNotes: (familyId: string, planId: string) => ['families', familyId, 'plans', planId, 'notes'] as const,
  planVotes: (familyId: string, planId: string) => ['families', familyId, 'plans', planId, 'votes'] as const,
  notifications: (familyId: string) => ['families', familyId, 'notifications'] as const,
  monthlyRecap: (familyId: string, year: number, month: number) =>
    ['families', familyId, 'recaps', 'monthly', year, month] as const,
};
