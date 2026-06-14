import { appEnv } from '@/lib/config/env';
import { supabase } from '@/lib/supabase/client';
import { ApiError, type ApiProblemDetails } from './api-error';

type ApiFetchOptions = Omit<RequestInit, 'body'> & {
  body?: unknown;
};

export async function apiFetch<TResponse>(
  path: string,
  options: ApiFetchOptions = {},
): Promise<TResponse> {
  const {
    data: { session },
  } = await supabase.auth.getSession();
  const headers = new Headers(options.headers);

  headers.set('Accept', 'application/json');

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  if (session?.access_token) {
    headers.set('Authorization', `Bearer ${session.access_token}`);
  }

  const response = await fetch(`${appEnv.apiBaseUrl}${path}`, {
    ...options,
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  });

  if (!response.ok) {
    throw await createApiError(response);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

async function createApiError(response: Response) {
  let problem: ApiProblemDetails | undefined;

  try {
    problem = (await response.json()) as ApiProblemDetails;
  } catch {
    problem = undefined;
  }

  return new ApiError(
    response.status,
    problem?.detail ?? problem?.title ?? 'PeaceNest could not complete that request.',
    problem,
  );
}
