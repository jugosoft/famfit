import {
  login as loginApi,
  register as registerApi,
  logout as logoutApi
} from '@/api/client';
import { useAuth as useAuthContext } from '@/contexts/AuthContext';
import type { LoginRequest, RegisterRequest } from '@/api/types';

/**
 * Хук для выполнения действий аутентификации (login, register, logout).
 * Состояние хранится в AuthContext.
 */
export function useAuth() {
  const { setUser, setLoading, setError, clearAuth } = useAuthContext();

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
    logout
  };
}
