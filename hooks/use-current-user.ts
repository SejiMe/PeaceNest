import { useQuery } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { GetMeResponse } from '@/lib/api/contracts';

export function useCurrentUser(enabled: boolean) {
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => apiFetch<GetMeResponse>('/auth/me'),
    enabled,
    staleTime: 60_000,
  });
}
