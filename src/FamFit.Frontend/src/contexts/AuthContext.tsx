import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { useAuthStore } from '@/store/authStore';
import { getCurrentUser } from '@/api/client';

interface AuthContextType {
  user: ReturnType<typeof getCurrentUser>;
  isAuthenticated: boolean;
  isLoading: boolean;
  refetch: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading, setIsLoading] = useState(true);
  const setUser = useAuthStore((state) => state.setUser);

  useEffect(() => {
    // Initialize auth state from localStorage
    const user = getCurrentUser();
    if (user) {
      setUser(user);
    }
    setIsLoading(false);
  }, [setUser]);

  const refetch = () => {
    const user = getCurrentUser();
    setUser(user);
  };

  const { isAuthenticated, user } = useAuthStore();

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, isLoading, refetch }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
