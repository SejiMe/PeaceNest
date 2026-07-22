import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type {
  CreateFamilyJoinRequestResponse,
  FamilyJoinRequestActionResponse,
  FamilyMemberRole,
  GenerateFamilyJoinCodeResponse,
  GetFamilyJoinCodeResponse,
  ListFamilyJoinRequestsResponse,
} from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useFamilyJoinCode(familyId: string | null | undefined) {
  return useQuery({
    queryKey: familyId ? queryKeys.familyJoinCode(familyId) : ['families', 'none', 'join-code'],
    queryFn: () => apiFetch<GetFamilyJoinCodeResponse>(`/families/${familyId}/join-code`),
    enabled: Boolean(familyId),
  });
}

export function useGenerateFamilyJoinCode(familyId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => apiFetch<GenerateFamilyJoinCodeResponse>(`/families/${familyId}/join-code`, { method: 'POST' }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.familyJoinCode(familyId) }),
  });
}

export function useRevokeFamilyJoinCode(familyId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => apiFetch<void>(`/families/${familyId}/join-code`, { method: 'DELETE' }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.familyJoinCode(familyId) }),
  });
}

export function useFamilyJoinRequests(familyId: string | null | undefined) {
  return useQuery({
    queryKey: familyId ? queryKeys.familyJoinRequests(familyId) : ['families', 'none', 'join-requests'],
    queryFn: () => apiFetch<ListFamilyJoinRequestsResponse>(`/families/${familyId}/join-requests`),
    enabled: Boolean(familyId),
    staleTime: 10_000,
  });
}

export function useMyFamilyJoinRequests(enabled = true) {
  return useQuery({
    queryKey: queryKeys.myFamilyJoinRequests,
    queryFn: () => apiFetch<ListFamilyJoinRequestsResponse>('/family-join-requests/mine'),
    enabled,
    staleTime: 10_000,
  });
}

export function useCreateFamilyJoinRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (code: string) => apiFetch<CreateFamilyJoinRequestResponse>('/family-join-requests', {
      method: 'POST',
      body: { code },
    }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.myFamilyJoinRequests }),
  });
}

export function useWithdrawFamilyJoinRequest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (requestId: string) => apiFetch<FamilyJoinRequestActionResponse>(`/family-join-requests/${requestId}/withdraw`, { method: 'POST' }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.myFamilyJoinRequests }),
  });
}

export function useApproveFamilyJoinRequest(familyId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ requestId, role }: { requestId: string; role: FamilyMemberRole }) =>
      apiFetch<FamilyJoinRequestActionResponse>(`/families/${familyId}/join-requests/${requestId}/approve`, {
        method: 'POST',
        body: { role },
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.familyJoinRequests(familyId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.notifications });
      queryClient.invalidateQueries({ queryKey: queryKeys.families });
    },
  });
}

export function useRejectFamilyJoinRequest(familyId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (requestId: string) => apiFetch<FamilyJoinRequestActionResponse>(`/families/${familyId}/join-requests/${requestId}/reject`, { method: 'POST' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.familyJoinRequests(familyId) });
      queryClient.invalidateQueries({ queryKey: queryKeys.notifications });
    },
  });
}
