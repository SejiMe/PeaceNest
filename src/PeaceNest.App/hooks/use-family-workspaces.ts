import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { CreateFamilyRequest, CreateFamilyResponse, ListFamiliesResponse, UpdatePreferredCurrencyRequest, UpdatePreferredCurrencyResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useFamilyWorkspaces(enabled: boolean) {
  return useQuery({
    queryKey: queryKeys.families,
    queryFn: () => apiFetch<ListFamiliesResponse>('/families'),
    enabled,
    staleTime: 60_000,
  });
}

export function useUpdatePreferredCurrency(familyId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdatePreferredCurrencyRequest) =>
      apiFetch<UpdatePreferredCurrencyResponse>(`/families/${familyId}/preferences/currency`, {
        method: 'PUT',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.authMe });
      queryClient.invalidateQueries({ queryKey: queryKeys.families });
    },
  });
}

export function useCreateFamilyWorkspace() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateFamilyRequest) =>
      apiFetch<CreateFamilyResponse>('/families', {
        method: 'POST',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.authMe });
      queryClient.invalidateQueries({ queryKey: queryKeys.families });
    },
  });
}
