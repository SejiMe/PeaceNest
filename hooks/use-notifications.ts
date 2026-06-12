import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { ListNotificationsResponse, MarkNotificationReadResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export function useNotifications(familyId: string | null | undefined) {
  return useQuery({
    queryKey: familyId ? queryKeys.notifications(familyId) : ['families', 'none', 'notifications'],
    queryFn: () => apiFetch<ListNotificationsResponse>(`/families/${familyId}/notifications`),
    enabled: Boolean(familyId),
    staleTime: 15_000,
  });
}

export function useMarkNotificationRead(familyId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (notificationId: string) =>
      apiFetch<MarkNotificationReadResponse>(`/families/${familyId}/notifications/${notificationId}/read`, {
        method: 'PUT',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.notifications(familyId) });
    },
  });
}
