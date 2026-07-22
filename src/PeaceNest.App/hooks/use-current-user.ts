import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { CompleteProfileRequest, CompleteProfileResponse, GetMeResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useCurrentUser(enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.authMe,
    queryFn: () => apiFetch<GetMeResponse>('/auth/me'),
    enabled,
    staleTime: 60_000,
  });
}

export function useCompleteProfile() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CompleteProfileRequest) =>
      apiFetch<CompleteProfileResponse>('/auth/profile', {
        method: 'PUT',
        body: request,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.authMe }),
  });
}
