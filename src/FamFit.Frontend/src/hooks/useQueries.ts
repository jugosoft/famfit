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
  deletePushSubscription
} from '@/api/client';
import type { ActivityTypeDto, ActivityLogDto, PagedResponse } from '@/api/types';

// ==================== Activity Types Queries ====================

export function useActivityTypes() {
  return useQuery({
    queryKey: ['activity-types'],
    queryFn: getActivityTypes,
    staleTime: 1000 * 60 * 5, // 5 minutes
    gcTime: 1000 * 60 * 30 // 30 minutes
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
    onSuccess: (data, variables) => {
      // Invalidate activity logs cache
      queryClient.invalidateQueries({ queryKey: ['activity-logs'] });
      return data;
    }
  });
}

export function useLogActivityWithNotification() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: logActivityWithNotification,
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['activity-logs'] });
      return data;
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
