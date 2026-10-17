import { http } from '@/lib/http';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ChangePasswordRequest,
  UserDto,
  UpdateUserRequest,
  ActivityTypeDto,
  AttributeInput,
  AttributeDefinitionDto,
  ActivityLogDto,
  LogActivityRequest,
  PagedResponse,
  RegisterPushSubscriptionRequest,
  VapidPublicKeyResponse,
  WorkoutDto,
  WorkoutListDto,
  CreateWorkoutRequest,
  WorkoutTemplateDto,
  WorkoutTemplateListDto,
  CreateWorkoutTemplateRequest,
  AddWorkoutTemplateItemRequest
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

// ==================== Admin User Management ====================

export async function getUsers(): Promise<UserDto[]> {
  return http.get<UserDto[]>('/admin/users');
}

export async function updateUser(id: string, data: UpdateUserRequest): Promise<UserDto> {
  return http.put<UserDto>(`/admin/users/${id}`, data);
}

export async function toggleBanUser(id: string): Promise<UserDto> {
  return http.put<UserDto>(`/admin/users/${id}/ban`, {});
}

export async function resetUserPassword(id: string, password: string): Promise<void> {
  await http.put<void>(`/admin/users/${id}/reset-password`, { password });
}

// ==================== Activity Types API ====================

export async function getActivityTypes(): Promise<ActivityTypeDto[]> {
  return http.get<ActivityTypeDto[]>('/activity-types');
}

export async function createActivityType(data: {
  name: string;
  attributeValues?: { definitionName: string; stringValue?: string }[];
}): Promise<ActivityTypeDto> {
  return http.post<ActivityTypeDto>('/activity-types', data);
}

export async function deactivateActivityType(id: string): Promise<void> {
  try {
    await http.put<void>(`/activity-types/${id}/deactivate`);
  } catch (error: any) {
    if (error?.response?.status === 409) {
      const detail = error?.response?.data?.message || 'Тип активности используется и не может быть деактивирован';
      throw new Error(detail);
    }
    throw error;
  }
}

export async function updateActivityType(
  id: string,
  data: { name?: string; attributeValueIds?: string[] }
): Promise<void> {
  await http.put<void>(`/activity-types/${id}`, data);
}

// ==================== Activity Type Attributes API ====================

export async function getActivityAttributeDefinitions(): Promise<AttributeDefinitionDto[]> {
  return http.get<AttributeDefinitionDto[]>('/attributes');
}

export async function deactivateActivityAttributeDefinition(id: string): Promise<void> {
  try {
    await http.put<void>(`/attributes/${id}/deactivate`);
  } catch (error: any) {
    // Если 409 Conflict — передаём сообщение об ошибке
    if (error?.response?.status === 409) {
      const detail = error?.response?.data?.message || 'Атрибут используется в значениях и не может быть деактивирован';
      throw new Error(detail);
    }
    throw error;
  }
}

export async function createAttributeDefinition(data: {
  name: string;
  displayName: string;
  dataType: 'Integer' | 'Decimal' | 'Boolean' | 'String';
  unit?: string;
}): Promise<AttributeDefinitionDto> {
  return http.post<AttributeDefinitionDto>('/attributes', data);
}

export async function updateAttributeDefinition(id: string, data: {
  name?: string;
  displayName?: string;
  unit?: string;
}): Promise<AttributeDefinitionDto> {
  return http.put<AttributeDefinitionDto>(`/attributes/${id}`, data);
}

export async function updateActivityTypeAttributes(
  activityTypeId: string,
  data: { attributeValues: AttributeInput[] }
): Promise<void> {
  await http.put<void>(`/activity-types/${activityTypeId}/attributes`, data);
}

// ==================== Workout API ====================

export async function getWorkouts(): Promise<WorkoutListDto[]> {
  return http.get<WorkoutListDto[]>('/workouts');
}

export async function getWorkout(id: string): Promise<WorkoutDto> {
  return http.get<WorkoutDto>(`/workouts/${id}`);
}

export async function createWorkout(data: CreateWorkoutRequest): Promise<WorkoutDto> {
  return http.post<WorkoutDto>('/workouts', data);
}

export async function addWorkoutEntry(
  workoutId: string,
  data: { activityTypeId: string; attributeValues: AttributeInput[] }
): Promise<void> {
  await http.post<void>(`/workouts/${workoutId}/entries`, {
    activityTypeId: data.activityTypeId,
    attributeValues: data.attributeValues
  });
}

export async function completeWorkout(id: string): Promise<void> {
  await http.post<void>(`/workouts/${id}/complete`);
}

export async function deleteWorkout(id: string): Promise<void> {
  await http.delete<void>(`/workouts/${id}`);
}

// ==================== Workout Template API ====================

export async function getWorkoutTemplates(): Promise<WorkoutTemplateListDto[]> {
  return http.get<WorkoutTemplateListDto[]>('/workout-templates');
}

export async function getWorkoutTemplate(id: string): Promise<WorkoutTemplateDto> {
  return http.get<WorkoutTemplateDto>(`/workout-templates/${id}`);
}

export async function createWorkoutTemplate(data: CreateWorkoutTemplateRequest): Promise<WorkoutTemplateDto> {
  return http.post<WorkoutTemplateDto>('/workout-templates', data);
}

export async function addWorkoutTemplateItem(
  templateId: string,
  data: AddWorkoutTemplateItemRequest
): Promise<void> {
  await http.post<void>(`/workout-templates/${templateId}/items`, data);
}

export async function removeWorkoutTemplateItem(
  templateId: string,
  activityTypeId: string
): Promise<void> {
  await http.delete<void>(`/workout-templates/${templateId}/items/${activityTypeId}`);
}

export async function deleteWorkoutTemplate(id: string): Promise<void> {
  await http.delete<void>(`/workout-templates/${id}`);
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

// ==================== Push Subscription API ====================

export async function registerPushSubscription(data: RegisterPushSubscriptionRequest): Promise<void> {
  await http.post<void>('/push/subscribe', data);
}

export async function deletePushSubscription(subscriptionId: string): Promise<void> {
  await http.delete<void>(`/push/${subscriptionId}`);
}

// ==================== Vapid Public Key API ====================

export async function getVapidPublicKey(): Promise<VapidPublicKeyResponse> {
  return http.get<VapidPublicKeyResponse>('/push/vapid-key');
}
