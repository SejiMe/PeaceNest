import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  ArchivePlanResponse,
  CompletePlanResponse,
  UpdatePlanProgressRequest,
  UpdatePlanProgressResponse,
} from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useFamilyPlanActions(familyId: string, planId: string) {
  const queryClient = useQueryClient();

  function invalidatePlanState() {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;

    queryClient.invalidateQueries({ queryKey: queryKeys.wantsAndNeeds(familyId) });
    queryClient.invalidateQueries({ queryKey: queryKeys.wantOrNeed(familyId, planId) });
    queryClient.invalidateQueries({ queryKey: queryKeys.milestones(familyId) });
    queryClient.invalidateQueries({ queryKey: queryKeys.milestone(familyId, planId) });
    queryClient.invalidateQueries({ queryKey: queryKeys.monthlyRecap(familyId, year, month) });
  }

  const updateProgress = useMutation({
    mutationFn: (request: UpdatePlanProgressRequest) =>
      apiFetch<UpdatePlanProgressResponse>(`/families/${familyId}/plans/${planId}/progress`, {
        method: 'PUT',
        body: request,
      }),
    onSuccess: invalidatePlanState,
  });

  const completePlan = useMutation({
    mutationFn: () =>
      apiFetch<CompletePlanResponse>(`/families/${familyId}/plans/${planId}/complete`, {
        method: 'PUT',
      }),
    onSuccess: invalidatePlanState,
  });

  const archivePlan = useMutation({
    mutationFn: () =>
      apiFetch<ArchivePlanResponse>(`/families/${familyId}/plans/${planId}/archive`, {
        method: 'PUT',
      }),
    onSuccess: invalidatePlanState,
  });

  return {
    archivePlan,
    completePlan,
    updateProgress,
  };
}
