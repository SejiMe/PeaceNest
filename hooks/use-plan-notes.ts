import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { AddPlanNoteRequest, AddPlanNoteResponse, ListPlanNotesResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function usePlanNotes(familyId: string | null | undefined, planId: string | null | undefined) {
  return useQuery({
    queryKey: familyId && planId ? queryKeys.planNotes(familyId, planId) : ['families', 'none', 'plans', 'none', 'notes'],
    queryFn: () => apiFetch<ListPlanNotesResponse>(`/families/${familyId}/plans/${planId}/notes`),
    enabled: Boolean(familyId && planId),
    staleTime: 15_000,
  });
}

export function useAddPlanNote(familyId: string, planId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AddPlanNoteRequest) =>
      apiFetch<AddPlanNoteResponse>(`/families/${familyId}/plans/${planId}/notes`, {
        method: 'POST',
        body: request,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.planNotes(familyId, planId) });
    },
  });
}

export function useDeletePlanNote(familyId: string, planId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (noteId: string) =>
      apiFetch<void>(`/families/${familyId}/plans/${planId}/notes/${noteId}`, {
        method: 'DELETE',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.planNotes(familyId, planId) });
    },
  });
}
