import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { CastPlanVoteRequest, CastPlanVoteResponse, ListPlanVotesResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function usePlanVotes(familyId: string | null | undefined, planId: string | null | undefined) {
  return useQuery({
    queryKey: familyId && planId ? queryKeys.planVotes(familyId, planId) : ['families', 'none', 'plans', 'none', 'votes'],
    queryFn: () => apiFetch<ListPlanVotesResponse>(`/families/${familyId}/plans/${planId}/votes`),
    enabled: Boolean(familyId && planId),
    staleTime: 15_000,
  });
}

export function useCastPlanVote(familyId: string, planId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CastPlanVoteRequest) =>
      apiFetch<CastPlanVoteResponse>(`/families/${familyId}/plans/${planId}/vote`, {
        method: 'PUT',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.planVotes(familyId, planId) });
    },
  });
}
