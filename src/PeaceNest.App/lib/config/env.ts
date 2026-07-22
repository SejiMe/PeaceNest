import Constants from 'expo-constants';
import { Platform } from 'react-native';

const defaultApiBaseUrl = 'http://localhost:5000';

export const appEnv = {
  apiBaseUrl: resolveApiBaseUrl(process.env.EXPO_PUBLIC_API_BASE_URL),
  supabaseUrl: process.env.EXPO_PUBLIC_SUPABASE_URL ?? '',
  supabasePublishableKey: process.env.EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY ?? '',
  enableDevAuthToken:
    __DEV__ && process.env.EXPO_PUBLIC_ENABLE_DEV_AUTH_TOKEN !== 'false',
};

export const isSupabaseConfigured =
  appEnv.supabaseUrl.startsWith('https://') &&
  appEnv.supabasePublishableKey.startsWith('sb_publishable_');

function resolveApiBaseUrl(configuredValue?: string) {
  const baseUrl = configuredValue?.trim() || defaultApiBaseUrl;

  if (Platform.OS === 'web') {
    return trimTrailingSlash(baseUrl);
  }

  try {
    const url = new URL(baseUrl);
    const isLocalhost = ['localhost', '127.0.0.1', '[::1]'].includes(url.hostname);

    if (!isLocalhost) {
      return trimTrailingSlash(baseUrl);
    }

    const expoHost = getExpoHost();
    url.hostname = expoHost ?? (Platform.OS === 'android' ? '10.0.2.2' : url.hostname);

    return trimTrailingSlash(url.toString());
  } catch {
    return trimTrailingSlash(baseUrl);
  }
}

function getExpoHost() {
  const hostUri = Constants.expoConfig?.hostUri;

  if (!hostUri) {
    return null;
  }

  const [host] = hostUri.split(':');

  if (!host || host === 'localhost' || host === '127.0.0.1' || host.endsWith('.exp.direct')) {
    return null;
  }

  return host;
}

function trimTrailingSlash(value: string) {
  return value.endsWith('/') ? value.slice(0, -1) : value;
}
