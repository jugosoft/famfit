export class ApiError extends Error {
  constructor(
    public status: number,
    public detail: string | null
  ) {
    super(`API Error ${status}: ${detail || 'Unknown error'}`);
    this.name = 'ApiError';
  }
}

/**
 * Base URL for API requests.
 */
const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

/**
 * HTTP client for API requests.
 */
export const http = {
  async request<T>(method: string, url: string, data?: unknown): Promise<T> {
    const token = localStorage.getItem('famfit_token');
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const config: RequestInit = {
      method,
      headers,
    };

    if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
      config.body = JSON.stringify(data);
    }

    const response = await fetch(`${BASE_URL}${url}`, config);

    if (!response.ok) {
      const detail = await response.text().catch(() => null);
      throw new ApiError(response.status, detail);
    }

    const text = await response.text();
    if (!text) return undefined as T;

    try {
      return JSON.parse(text) as T;
    } catch {
      return text as unknown as T;
    }
  },

  get<T>(url: string): Promise<T> {
    return http.request<T>('GET', url);
  },

  post<T>(url: string, data?: unknown): Promise<T> {
    return http.request<T>('POST', url, data);
  },

  put<T>(url: string, data?: unknown): Promise<T> {
    return http.request<T>('PUT', url, data);
  },

  delete<T>(url: string): Promise<T> {
    return http.request<T>('DELETE', url);
  },
};
