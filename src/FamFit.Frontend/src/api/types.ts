/**
 * Универсальные типы ответов API.
 * Синхронизированы с бэкенд-моделями FamFit.WebApi.Models.
 */

export interface ApiResponse<T> {
  data: T | null;
  error: string | null;
  success: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * ==================== Auth ====================
 * Синхронизировано с AuthEndpoints.UserDto, LoginResponse, RegisterRequest
 */

export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: 'Admin' | 'Member';
  streakCount: number;
  isBanned: boolean;
}

export interface UpdateUserRequest {
  name?: string;
  email?: string;
  role?: 'Admin' | 'Member';
}

export interface ResetPasswordRequest {
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  tokenType: string;
  expiresIn: number;
  user: UserDto;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

/**
 * ==================== ActivityType ====================
 * Синхронизировано с FamFit.Domain.Entities.ActivityType
 */

export type UnitType = 'Count' | 'Time';
export type AttributeDataType = 'Integer' | 'Decimal' | 'Boolean' | 'String';

export interface AttributeValueDto {
  id: string;
  definitionName: string;
  displayName: string;
  dataType: AttributeDataType;
  unit?: string | null;
  value: unknown;
}

export interface ActivityTypeDto {
  id: string;
  name: string;
  unitType: UnitType;
  isActive?: boolean;
  attributes: AttributeValueDto[];
}

export interface AttributeDefinitionDto {
  id: string;
  name: string;
  displayName: string;
  dataType: AttributeDataType;
  unit?: string | null;
  isActive: boolean;
  displayOrder: number;
}

export interface AttributeInput {
  definitionName: string;
  intValue?: number | null;
  decimalValue?: number | null;
  boolValue?: boolean | null;
  stringValue?: string | null;
}

export interface UpdateActivityTypeAttributesRequest {
  attributeValues: AttributeInput[];
}

export interface CreateActivityTypeRequest {
  name: string;
}

export interface UpdateActivityTypeRequest {
  name?: string;
}

export interface CreateAttributeDefinitionRequest {
  name: string;
  displayName: string;
  dataType: 'Integer' | 'Decimal' | 'Boolean' | 'String';
  unit?: string;
  displayOrder: number;
}

export interface UpdateAttributeDefinitionRequest {
  name?: string;
  displayName?: string;
  unit?: string;
  displayOrder?: number;
}

/**
 * ==================== ActivityUnit ====================
 * Единицы измерения активностей
 */

export interface ActivityUnitDto {
  id: string;
  name: string;
  symbol: string;
  dataType: 'Integer' | 'Decimal' | 'String';
  isActive: boolean;
  displayOrder: number;
}

export interface CreateActivityUnitRequest {
  name: string;
  symbol: string;
  dataType: 'Integer' | 'Decimal' | 'String';
  displayOrder: number;
}

export interface UpdateActivityUnitRequest {
  name?: string;
  symbol?: string;
  dataType?: 'Integer' | 'Decimal' | 'String';
  isActive?: boolean;
  displayOrder?: number;
}

/**
 * ==================== ActivityLog ====================
 * Синхронизировано с FamFit.Domain.Entities.ActivityLog
 */

export interface ActivityLogDto {
  id: string;
  userId: string;
  activityTypeId: string;
  value: number;
  timestamp: string;
  activityType?: ActivityTypeDto;
}

export interface LogActivityRequest {
  activityTypeId: string;
  value: number;
  timestamp?: string;
}

/**
 * ==================== PushSubscription ====================
 * Синхронизировано с FamFit.Domain.Entities.PushSubscription
 */

export interface PushSubscriptionDto {
  id: string;
  userId: string;
  endpoint: string;
  p256dh: string;
  auth: string;
  createdAt: string;
}

export interface RegisterPushSubscriptionRequest {
  endpoint: string;
  p256dh: string;
  auth: string;
}

/**
 * ==================== VAPID ====================
 * Синхронизировано с VapidEndpoints.VapidPublicKeyResponse
 */

export interface VapidPublicKeyResponse {
  publicKey: string;
}

/**
 * ==================== Workout ====================
 */

export interface WorkoutEntryAttributeValueDto {
  id: string;
  definitionName: string;
  displayName: string;
  dataType: AttributeDataType;
  unit?: string | null;
  value: unknown;
}

export interface WorkoutEntryDto {
  id: string;
  activityTypeId: string;
  attributes: WorkoutEntryAttributeValueDto[];
  activityType?: ActivityTypeDto;
}

export interface WorkoutDto {
  id: string;
  userId: string;
  name?: string | null;
  startTime: string;
  endTime?: string | null;
  isCompleted: boolean;
  entries: WorkoutEntryDto[];
}

export interface WorkoutListDto {
  id: string;
  name?: string | null;
  startTime: string;
  endTime?: string | null;
  isCompleted: boolean;
}

export interface CreateWorkoutRequest {
  name?: string | null;
}

/**
 * ==================== WorkoutTemplate ====================
 */

export interface WorkoutTemplateItemAttributeValueDto {
  id: string;
  definitionName: string;
  displayName: string;
  dataType: AttributeDataType;
  unit?: string | null;
  value: unknown;
}

export interface WorkoutTemplateItemDto {
  id: string;
  activityTypeId: string;
  displayOrder: number;
  attributes: WorkoutTemplateItemAttributeValueDto[];
  activityType?: ActivityTypeDto;
}

export interface WorkoutTemplateDto {
  id: string;
  userId: string;
  name: string;
  description?: string | null;
  items: WorkoutTemplateItemDto[];
}

export interface WorkoutTemplateListDto {
  id: string;
  name: string;
  description?: string | null;
  itemsCount: number;
}

export interface CreateWorkoutTemplateRequest {
  name: string;
  description?: string | null;
}

export interface TemplateAttributeInput {
  definitionName: string;
  intValue?: number | null;
  decimalValue?: number | null;
  boolValue?: boolean | null;
  stringValue?: string | null;
}

export interface AddWorkoutTemplateItemRequest {
  activityTypeId: string;
  attributeValues: TemplateAttributeInput[];
  displayOrder?: number;
}
