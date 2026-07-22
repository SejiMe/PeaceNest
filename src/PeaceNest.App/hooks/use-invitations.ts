import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { AcceptInvitationRequest, AcceptInvitationResponse, CreateInvitationRequest, CreateInvitationResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useAcceptInvitation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AcceptInvitationRequest) =>
      apiFetch<AcceptInvitationResponse>('/family-invitations/accept', {
        method: 'POST',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.authMe });
      queryClient.invalidateQueries({ queryKey: queryKeys.families });
    },
  });
}

export function useCreateInvitation(familyId: string) {
  return useMutation({
    mutationFn: (request: CreateInvitationRequest) =>
      apiFetch<CreateInvitationResponse>(`/families/${familyId}/invitations`, {
        method: 'POST',
        body: request,
      }),
  });
}
