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

  if (__DEV__ && appEnv.apiBaseUrl.includes('.devtunnels.ms')) {
    headers.set('X-Tunnel-Skip-AntiPhishing-Page', 'true');
  }

  const response = await fetchApi(path, options, headers);

  if (!response.ok) {
    throw await createApiError(response);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return (await response.json()) as TResponse;
}

async function fetchApi(path: string, options: ApiFetchOptions, headers: Headers) {
  try {
    return await fetch(`${appEnv.apiBaseUrl}${path}`, {
      ...options,
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
    });
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }

    throw new ApiError(
      0,
      `PeaceNest could not reach the API at ${appEnv.apiBaseUrl}. Make sure the backend is running, then try again.`,
    );
  }
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
