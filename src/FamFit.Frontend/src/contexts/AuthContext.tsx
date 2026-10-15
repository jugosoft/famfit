import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import { getCurrentUser } from '@/api/client';
import type { UserDto } from '@/api/types';

interface AuthState {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface AuthContextType extends AuthState {
  setUser: (user: UserDto | null) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearAuth: () => void;
  refetch: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
    error: null
  });

  const setUser = useCallback((user: UserDto | null) => {
    setState(prev => ({
      ...prev,
      user,
      isAuthenticated: user !== null,
      error: null
    }));
  }, []);

  const setLoading = useCallback((loading: boolean) => {
    setState(prev => ({ ...prev, isLoading: loading }));
  }, []);

  const setError = useCallback((error: string | null) => {
    setState(prev => ({ ...prev, error }));
  }, []);

  const clearAuth = useCallback(() => {
    setState({
      user: null,
      isAuthenticated: false,
      isLoading: false,
      error: null
    });
  }, []);

  const refetch = useCallback(() => {
    const user = getCurrentUser();
    setUser(user);
  }, [setUser]);

  useEffect(() => {
    // Initialize auth state from localStorage
    const user = getCurrentUser();
    if (user) {
      setUser(user);
    }
    setState(prev => ({ ...prev, isLoading: false }));
  }, [setUser]);

  // Listen for token expiration events from http client
  useEffect(() => {
    const handleUnauthorized = () => {
      clearAuth();
    };

    window.addEventListener('famfit:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('famfit:unauthorized', handleUnauthorized);
  }, [clearAuth]);

  return (
    <AuthContext.Provider value={{ ...state, setUser, setLoading, setError, clearAuth, refetch }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
