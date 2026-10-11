import {
  useQuery,
  useMutation,
  useQueryClient
} from '@tanstack/react-query';
import {
  getActivityTypes,
  getActivityLogs,
  logActivity,
  logActivityWithNotification,
  registerPushSubscription,
  deletePushSubscription,
  getMe
} from '@/api/client';
import type { ActivityTypeDto, ActivityLogDto, PagedResponse } from '@/api/types';

// ==================== Auth Queries ====================

export function useCurrentUser() {
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: getMe,
    staleTime: 1000 * 60 * 5,
    retry: (failCount: number, error: Error) => {
      // Не ретраим при 401
      return !(error instanceof ApiError && (error as unknown as { status: number }).status === 401);
    }
  });
}

// ==================== Activity Types Queries ====================

export function useActivityTypes() {
  return useQuery({
    queryKey: ['activity-types'],
    queryFn: getActivityTypes,
    staleTime: 1000 * 60 * 5,
    gcTime: 1000 * 60 * 30
  });
}

// ==================== Activity Logs Queries ====================

export function useActivityLogs(userId: string, page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['activity-logs', userId, page, pageSize],
    queryFn: () => getActivityLogs(userId, page, pageSize),
    enabled: !!userId
  });
}

// ==================== Activity Logs Mutations ====================

export function useLogActivity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logActivity,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activity-logs'] });
    }
  });
}

export function useLogActivityWithNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logActivityWithNotification,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activity-logs'] });
    }
  });
}

// ==================== Push Subscription Mutations ====================

export function useRegisterPushSubscription() {
  return useMutation({
    mutationFn: registerPushSubscription
  });
}

export function useDeletePushSubscription() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deletePushSubscription,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['push-subscriptions'] });
    }
  });
}
