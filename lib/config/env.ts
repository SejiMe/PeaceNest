export const appEnv = {
  apiBaseUrl: process.env.EXPO_PUBLIC_API_BASE_URL ?? 'http://localhost:5000',
  supabaseUrl: process.env.EXPO_PUBLIC_SUPABASE_URL ?? '',
  supabasePublishableKey: process.env.EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY ?? '',
  enableDevAuthToken:
    __DEV__ && process.env.EXPO_PUBLIC_ENABLE_DEV_AUTH_TOKEN !== 'false',
};

export const isSupabaseConfigured =
  appEnv.supabaseUrl.startsWith('https://') &&
  appEnv.supabasePublishableKey.startsWith('sb_publishable_');
