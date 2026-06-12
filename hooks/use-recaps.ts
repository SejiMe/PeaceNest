import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '@/lib/api/client';
import type { GenerateMonthlyRecapResponse, GetMonthlyRecapResponse } from '@/lib/api/contracts';
import { queryKeys } from '@/lib/api/query-keys';

export type MonthlyRecapPeriod = {
  year: number;
  month: number;
};

export function currentMonthlyRecapPeriod(date = new Date()): MonthlyRecapPeriod {
  return {
    year: date.getFullYear(),
    month: date.getMonth() + 1,
  };
}

export function useMonthlyRecap(
  familyId: string | null | undefined,
  year: number,
  month: number,
) {
  return useQuery({
    queryKey: familyId ? queryKeys.monthlyRecap(familyId, year, month) : ['families', 'none', 'recaps', 'monthly'],
    queryFn: () => apiFetch<GetMonthlyRecapResponse>(`/families/${familyId}/recaps/monthly/${year}/${month}`),
    enabled: Boolean(familyId),
    staleTime: 30_000,
    retry: (failureCount, error) => {
      if (error instanceof Error && 'status' in error && error.status === 404) {
        return false;
      }

      return failureCount < 2;
    },
  });
}

export function useGenerateMonthlyRecap(familyId: string, year: number, month: number) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () =>
      apiFetch<GenerateMonthlyRecapResponse>(`/families/${familyId}/recaps/monthly/${year}/${month}/generate`, {
        method: 'POST',
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.monthlyRecap(familyId, year, month) });
      queryClient.invalidateQueries({ queryKey: queryKeys.notifications(familyId) });
    },
  });
}
