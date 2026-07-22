import AsyncStorage from '@react-native-async-storage/async-storage';

const key = 'peacenest.pending-invitation-token';

export const pendingInvitation = {
  get: () => AsyncStorage.getItem(key),
  save: (token: string) => AsyncStorage.setItem(key, token),
  clear: () => AsyncStorage.removeItem(key),
};
