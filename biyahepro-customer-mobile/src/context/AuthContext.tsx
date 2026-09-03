import { createContext, useContext, useEffect, useMemo, useState, type PropsWithChildren } from 'react';
import { api } from '@/src/lib/api';
import { clearSession, loadSession, saveSession } from '@/src/lib/session';
import type { AuthResponse, RegisterPayload } from '@/src/types/api';

type AuthContextValue = {
  session: AuthResponse | null;
  isLoading: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  signOut: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadSession().then((stored) => {
      setSession(stored?.role === 'customer' ? stored : null);
      setIsLoading(false);
    });
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    session,
    isLoading,
    async signIn(email, password) {
      const next = await api.login(email.trim(), password);
      if (next.role !== 'customer') throw new Error('Please use a customer account in this app.');
      await saveSession(next);
      setSession(next);
    },
    async register(payload) {
      const next = await api.register(payload);
      if (next.role !== 'customer') throw new Error('Customer registration failed.');
      await saveSession(next);
      setSession(next);
    },
    async signOut() {
      await clearSession();
      setSession(null);
    },
  }), [session, isLoading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth must be used within AuthProvider.');
  return value;
}
