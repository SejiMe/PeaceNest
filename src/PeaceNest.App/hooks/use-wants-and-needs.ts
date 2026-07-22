import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  CreateWantOrNeedRequest,
  CreateWantOrNeedResponse,
  GetWantOrNeedResponse,
  ListWantsAndNeedsResponse,
  UpdateWantOrNeedRequest,
  UpdateWantOrNeedResponse,
} from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useWantsAndNeeds(familyId: string | null | undefined) {
  return useQuery({
    queryKey: familyId ? queryKeys.wantsAndNeeds(familyId) : ['families', 'none', 'wants-needs'],
    queryFn: () => apiFetch<ListWantsAndNeedsResponse>(`/families/${familyId}/wants-needs`),
    enabled: Boolean(familyId),
    staleTime: 30_000,
  });
}

export function useUpdateWantOrNeed(familyId: string, planId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateWantOrNeedRequest) =>
      apiFetch<UpdateWantOrNeedResponse>(`/families/${familyId}/wants-needs/${planId}`, {
        method: 'PUT',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.wantsAndNeeds(familyId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.wantOrNeed(familyId, planId) });
    },
  });
}

export function useWantOrNeed(familyId: string | null | undefined, planId: string | null | undefined) {
  return useQuery({
    queryKey: familyId && planId ? queryKeys.wantOrNeed(familyId, planId) : ['families', 'none', 'wants-needs', 'none'],
    queryFn: () => apiFetch<GetWantOrNeedResponse>(`/families/${familyId}/wants-needs/${planId}`),
    enabled: Boolean(familyId && planId),
    staleTime: 30_000,
  });
}

export function useCreateWantOrNeed(familyId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateWantOrNeedRequest) =>
      apiFetch<CreateWantOrNeedResponse>(`/families/${familyId}/wants-needs`, {
        method: 'POST',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.wantsAndNeeds(familyId) });
    },
  });
}
