import { http } from '@/lib/http';

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

// Auth
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

export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: string;
  streakCount: number;
}

// ActivityType
export interface ActivityTypeDto {
  id: string;
  name: string;
  unitType: 'Count' | 'Time';
}

// ActivityLog
export interface ActivityLogDto {
  id: string;
  userId: string;
  activityTypeId: string;
  value: number;
  timestamp: string;
  activityType?: ActivityTypeDto;
}

// PushSubscription
export interface PushSubscriptionDto {
  id: string;
  userId: string;
  endpoint: string;
  p256dh: string;
  auth: string;
  createdAt: string;
}

// VAPID
export interface VapidPublicKeyResponse {
  publicKey: string;
}
