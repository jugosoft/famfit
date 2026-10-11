import { useQueryClient } from '@tanstack/react-query';
import { ApiError } from '@/lib/errors';

/**
 * Custom error class for API errors.
 */
export class ApiError extends Error {
  constructor(
    public status: number,
    public detail: string | null
  ) {
    super(`API Error ${status}: ${detail || 'Unknown error'}`);
    this.name = 'ApiError';
  }
}
