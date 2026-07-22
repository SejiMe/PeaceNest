import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  LeaveFamilyResponse,
  RecoverFamilyRequest,
  RecoverFamilyResponse,
} from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useLeaveFamily(familyId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      apiFetch<LeaveFamilyResponse>(`/families/${familyId}/leave`, {
        method: 'POST',
      }),
    onSuccess: () => invalidateFamilyAccess(queryClient),
  });
}

export function useRecoverFamily() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: RecoverFamilyRequest) =>
      apiFetch<RecoverFamilyResponse>('/families/recover', {
        method: 'POST',
        body: request,
      }),
    onSuccess: () => invalidateFamilyAccess(queryClient),
  });
}

function invalidateFamilyAccess(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: queryKeys.authMe });
  queryClient.invalidateQueries({ queryKey: queryKeys.families });
  queryClient.invalidateQueries({ queryKey: queryKeys.notifications });
  queryClient.invalidateQueries({ queryKey: queryKeys.myFamilyJoinRequests });
}
