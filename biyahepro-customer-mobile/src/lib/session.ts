import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';
import type { AuthResponse } from '@/src/types/api';

const SESSION_KEY = 'biyahepro_customer_session';

export async function saveSession(session: AuthResponse) {
  const value = JSON.stringify(session);
  if (Platform.OS === 'web') {
    localStorage.setItem(SESSION_KEY, value);
    return;
  }
  await SecureStore.setItemAsync(SESSION_KEY, value);
}

export async function loadSession(): Promise<AuthResponse | null> {
  try {
    const value = Platform.OS === 'web'
      ? localStorage.getItem(SESSION_KEY)
      : await SecureStore.getItemAsync(SESSION_KEY);
    return value ? (JSON.parse(value) as AuthResponse) : null;
  } catch {
    return null;
  }
}

export async function clearSession() {
  if (Platform.OS === 'web') {
    localStorage.removeItem(SESSION_KEY);
    return;
  }
  await SecureStore.deleteItemAsync(SESSION_KEY);
}
