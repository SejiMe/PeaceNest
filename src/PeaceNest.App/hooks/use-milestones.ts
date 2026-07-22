import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  CreateMilestoneRequest,
  CreateMilestoneResponse,
  GetMilestoneResponse,
  ListMilestonesResponse,
  UpdateMilestoneRequest,
  UpdateMilestoneResponse,
  UpdateMilestoneStepCompletionRequest,
  UpdateMilestoneStepCompletionResponse,
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

export function useUpdateMilestone(familyId: string, milestoneId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateMilestoneRequest) =>
      apiFetch<UpdateMilestoneResponse>(`/families/${familyId}/milestones/${milestoneId}`, {
        method: 'PUT',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.milestones(familyId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.milestone(familyId, milestoneId) });
    },
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

export function useUpdateMilestoneStepCompletion(familyId: string, milestoneId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ isCompleted, stepId }: UpdateMilestoneStepCompletionRequest & { stepId: string }) =>
      apiFetch<UpdateMilestoneStepCompletionResponse>(
        `/families/${familyId}/milestones/${milestoneId}/steps/${stepId}/completion`,
        {
          method: 'PUT',
          body: { isCompleted },
        },
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.milestones(familyId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.milestone(familyId, milestoneId) });
    },
  });
}
