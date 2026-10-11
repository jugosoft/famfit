import {
  login as loginApi,
  register as registerApi,
  logout as logoutApi,
  getCurrentUser
} from '@/api/client';
import { useAuthStore } from '@/store/authStore';
import type { LoginRequest, RegisterRequest } from '@/api/types';

export function useAuth() {
  const { setUser, setLoading, setError, clearAuth } = useAuthStore();

  // Initialize auth state from localStorage on mount
  if (typeof window !== 'undefined') {
    const user = getCurrentUser();
    if (user) {
      setUser(user);
    }
  }

  const login = async (data: LoginRequest) => {
    setLoading(true);
    setError(null);

    try {
      const response = await loginApi(data);
      setUser(response.user);
      return response;
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Login failed';
      setError(message);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const register = async (data: RegisterRequest) => {
    setLoading(true);
    setError(null);

    try {
      const user = await registerApi(data);
      setUser(user);
      return user;
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Registration failed';
      setError(message);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    logoutApi();
    clearAuth();
  };

  return {
    login,
    register,
    logout,
    isAuthenticated: useAuthStore.getState().isAuthenticated,
    user: useAuthStore.getState().user,
    isLoading: useAuthStore.getState().isLoading,
    error: useAuthStore.getState().error
  };
}
