import { http, ApiError } from '@/lib/http';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ChangePasswordRequest,
  UserDto,
  ActivityTypeDto,
  CreateActivityTypeRequest,
  ActivityLogDto,
  LogActivityRequest,
  PagedResponse,
  PushSubscriptionDto,
  RegisterPushSubscriptionRequest,
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

export async function changePassword(data: ChangePasswordRequest): Promise<void> {
  await http.put<void>('/auth/change-password', data);
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

export async function getMe(): Promise<UserDto> {
  return http.get<UserDto>('/auth/me');
}

// ==================== Activity Types API ====================

export async function getActivityTypes(): Promise<ActivityTypeDto[]> {
  return http.get<ActivityTypeDto[]>('/activity-types');
}

export async function createActivityType(data: CreateActivityTypeRequest): Promise<ActivityTypeDto> {
  return http.post<ActivityTypeDto>('/activity-types', data);
}

export async function deactivateActivityType(id: string): Promise<void> {
  await http.put<void>(`/activity-types/${id}/deactivate`);
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

export async function logActivity(data: LogActivityRequest): Promise<ActivityLogDto> {
  return http.post<ActivityLogDto>('/activity-logs', data);
}

export async function logActivityWithNotification(data: LogActivityRequest): Promise<ActivityLogDto> {
  return http.post<ActivityLogDto>('/activity-logs/with-notification', data);
}

// ==================== Push Subscriptions API ====================

export async function registerPushSubscription(data: RegisterPushSubscriptionRequest): Promise<PushSubscriptionDto> {
  return http.post<PushSubscriptionDto>('/push-subscriptions', data);
}

export async function deletePushSubscription(id: string): Promise<void> {
  await http.delete<void>(`/push-subscriptions/${id}`);
}

// ==================== VAPID API ====================

export async function getVapidPublicKey(): Promise<VapidPublicKeyResponse> {
  return http.get<VapidPublicKeyResponse>('/vapid/public-key');
}
