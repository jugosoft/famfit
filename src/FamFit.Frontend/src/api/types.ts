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

export interface ActivityTypeDto {
  id: string;
  name: string;
  unitType: UnitType;
  isActive?: boolean;
}

export interface CreateActivityTypeRequest {
  name: string;
  unitType: UnitType;
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
