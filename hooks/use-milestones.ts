import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  CreateMilestoneRequest,
  CreateMilestoneResponse,
  GetMilestoneResponse,
  ListMilestonesResponse,
} from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useMilestones(familyId: string | null | undefined) {
  return useQuery({
    queryKey: familyId ? queryKeys.milestones(familyId) : ['families', 'none', 'milestones'],
    queryFn: () => apiFetch<ListMilestonesResponse>(`/families/${familyId}/milestones`),
    enabled: Boolean(familyId),
    staleTime: 30_000,
  });
}

export function useMilestone(familyId: string | null | undefined, planId: string | null | undefined) {
  return useQuery({
    queryKey: familyId && planId ? queryKeys.milestone(familyId, planId) : ['families', 'none', 'milestones', 'none'],
    queryFn: () => apiFetch<GetMilestoneResponse>(`/families/${familyId}/milestones/${planId}`),
    enabled: Boolean(familyId && planId),
    staleTime: 30_000,
  });
}

export function useCreateMilestone(familyId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateMilestoneRequest) =>
      apiFetch<CreateMilestoneResponse>(`/families/${familyId}/milestones`, {
        method: 'POST',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.milestones(familyId) });
    },
  });
}
