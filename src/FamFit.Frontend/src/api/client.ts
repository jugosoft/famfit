import { http } from '@/lib/http';
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ChangePasswordRequest,
  UserDto,
  ActivityTypeDto,
  CreateActivityTypeRequest,
  UpdateActivityTypeAttributesRequest,
  AttributeInput,
  AttributeDefinitionDto,
  ActivityLogDto,
  LogActivityRequest,
  PagedResponse,
  PushSubscriptionDto,
  RegisterPushSubscriptionRequest,
  VapidPublicKeyResponse,
  WorkoutDto,
  WorkoutListDto,
  CreateWorkoutRequest,
  WorkoutTemplateDto,
  WorkoutTemplateListDto,
  CreateWorkoutTemplateRequest,
  AddWorkoutTemplateItemRequest,
  TemplateAttributeInput
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

export async function getActivityAttributeDefinitions(): Promise<AttributeDefinitionDto[]> {
  return http.get<AttributeDefinitionDto[]>('/activity-types/definitions');
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

// ==================== Activity Type Attributes API ====================

export async function updateActivityTypeAttributes(
  activityTypeId: string,
  data: AttributeInput[]
): Promise<void> {
  await http.put<void>(`/activity-types/${activityTypeId}/attributes`, {
    attributeValues: data
  });
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
