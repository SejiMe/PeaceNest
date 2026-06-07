export const queryKeys = {
  authMe: ['auth', 'me'] as const,
  families: ['families'] as const,
  wantsAndNeeds: (familyId: string) => ['families', familyId, 'wants-needs'] as const,
  milestones: (familyId: string) => ['families', familyId, 'milestones'] as const,
};
