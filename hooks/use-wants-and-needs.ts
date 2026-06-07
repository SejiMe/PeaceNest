import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  CreateWantOrNeedRequest,
  CreateWantOrNeedResponse,
  ListWantsAndNeedsResponse,
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
