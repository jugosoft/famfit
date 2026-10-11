import { http, ApiError } from '@/lib/http';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  UserDto,
  ActivityTypeDto,
  ActivityLogDto,
  PagedResponse,
  PushSubscriptionDto,
  VapidPublicKeyResponse
} from '@/api/types';

// ==================== Auth API ====================

export async function login(data: LoginRequest): Promise<LoginResponse> {
  const response = await http.post<LoginResponse>('/auth/login', data);
  localStorage.setItem('famfit_token', response.token);
  localStorage.setItem('famfit_user', JSON.stringify(response.user));
  return response;
}

export async function register(data: RegisterRequest): Promise<UserDto> {
  const response = await http.post<UserDto>('/auth/register', data);
  return response;
}

export function logout(): void {
  localStorage.removeItem('famfit_token');
  localStorage.removeItem('famfit_user');
}

export function getCurrentUser(): UserDto | null {
  const userStr = localStorage.getItem('famfit_user');
  if (!userStr) return null;
  try {
    return JSON.parse(userStr) as UserDto;
  } catch {
    return null;
  }
}

// ==================== Activity Types API ====================

export async function getActivityTypes(): Promise<ActivityTypeDto[]> {
  return http.get<ActivityTypeDto[]>('/activity-types');
}

// ==================== Activity Logs API ====================

export async function getActivityLogs(
  userId: string,
  page = 1,
  pageSize = 20
): Promise<PagedResponse<ActivityLogDto>> {
  return http.get<PagedResponse<ActivityLogDto>>(
    `/activity-logs?userId=${userId}&page=${page}&pageSize=${pageSize}`
  );
}

export async function logActivity(data: {
  activityTypeId: string;
  value: number;
  timestamp?: string;
}): Promise<ActivityLogDto> {
  return http.post<ActivityLogDto>('/activity-logs', data);
}

export async function logActivityWithNotification(data: {
  activityTypeId: string;
  value: number;
  timestamp?: string;
}): Promise<ActivityLogDto> {
  return http.post<ActivityLogDto>('/activity-logs/with-notification', data);
}

// ==================== Push Subscriptions API ====================

export async function registerPushSubscription(data: {
  endpoint: string;
  p256dh: string;
  auth: string;
}): Promise<PushSubscriptionDto> {
  return http.post<PushSubscriptionDto>('/push-subscriptions', data);
}

export async function deletePushSubscription(id: string): Promise<void> {
  return http.delete<void>(`/push-subscriptions/${id}`);
}

// ==================== VAPID API ====================

export async function getVapidPublicKey(): Promise<VapidPublicKeyResponse> {
  return http.get<VapidPublicKeyResponse>('/vapid/public-key');
}
