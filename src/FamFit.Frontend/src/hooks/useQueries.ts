import {
  useQuery,
  useMutation,
  useQueryClient
} from '@tanstack/react-query';
import {
  getActivityTypes,
  getActivityAttributeDefinitions,
  deactivateActivityAttributeDefinition,
  createAttributeDefinition,
  updateAttributeDefinition,
  getActivityLogs,
  logActivity,
  logActivityWithNotification,
  registerPushSubscription,
  deletePushSubscription,
  getMe,
  updateActivityTypeAttributes,
  getWorkouts,
  getWorkout,
  createWorkout,
  addWorkoutEntry,
  completeWorkout,
  deleteWorkout,
  getWorkoutTemplates,
  getWorkoutTemplate,
  createWorkoutTemplate,
  addWorkoutTemplateItem,
  removeWorkoutTemplateItem,
  deleteWorkoutTemplate,
} from '@/api/client';
import { ApiError } from '@/lib/errors';

// ==================== Auth Queries ====================

export function useCurrentUser() {
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: getMe,
    staleTime: 1000 * 60 * 5,
    retry: (_failCount, error: unknown) => {
      // Не ретраим при 401
      return !(error instanceof ApiError);
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

export function useAttributeDefinitions() {
  return useQuery({
    queryKey: ['attribute-definitions'],
    queryFn: getActivityAttributeDefinitions,
    staleTime: 1000 * 60 * 60,
    gcTime: 1000 * 60 * 60 * 24
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

// ==================== Activity Type Attributes Mutations ====================

export function useUpdateActivityTypeAttributes() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { activityTypeId: string; attributeValues: { definitionName: string; intValue?: number | null; decimalValue?: number | null; boolValue?: boolean | null; stringValue?: string | null }[] }) =>
      updateActivityTypeAttributes(data.activityTypeId, { attributeValues: data.attributeValues }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    }
  });
}

export function useCreateAttributeDefinitionMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createAttributeDefinition,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attribute-definitions'] });
    }
  });
}

export function useUpdateAttributeDefinitionMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: { name?: string; displayName?: string; unit?: string } }) =>
      updateAttributeDefinition(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attribute-definitions'] });
    }
  });
}

export function useDeactivateAttributeDefinition() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deactivateActivityAttributeDefinition,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attribute-definitions'] });
    }
  });
}

// ==================== Workout Queries ====================

export function useWorkouts() {
  return useQuery({
    queryKey: ['workouts'],
    queryFn: getWorkouts,
    staleTime: 1000 * 60 * 2
  });
}

export function useWorkout(id: string) {
  return useQuery({
    queryKey: ['workout', id],
    queryFn: () => getWorkout(id),
    enabled: !!id,
    staleTime: 1000 * 60 * 2
  });
}

// ==================== Workout Mutations ====================

export function useCreateWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createWorkout,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workouts'] });
    }
  });
}

export function useAddWorkoutEntry() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { workoutId: string; activityTypeId: string; attributeValues: { definitionName: string; intValue?: number | null; decimalValue?: number | null; boolValue?: boolean | null; stringValue?: string | null }[] }) =>
      addWorkoutEntry(data.workoutId, { activityTypeId: data.activityTypeId, attributeValues: data.attributeValues }),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({ queryKey: ['workout', vars.workoutId] });
    }
  });
}

export function useCompleteWorkout() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: completeWorkout,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workouts'] });
    }
  });
}

export function useDeleteWorkoutMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteWorkout,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workouts'] });
    }
  });
}

// ==================== Workout Template Queries ====================

export function useWorkoutTemplates() {
  return useQuery({
    queryKey: ['workout-templates'],
    queryFn: getWorkoutTemplates,
    staleTime: 1000 * 60 * 5
  });
}

export function useWorkoutTemplate(id: string) {
  return useQuery({
    queryKey: ['workout-template', id],
    queryFn: () => getWorkoutTemplate(id),
    enabled: !!id,
    staleTime: 1000 * 60 * 5
  });
}

// ==================== Workout Template Mutations ====================

export function useCreateWorkoutTemplate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createWorkoutTemplate,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
    }
  });
}

export function useAddWorkoutTemplateItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { templateId: string; activityTypeId: string; attributeValues: { definitionName: string; intValue?: number | null; decimalValue?: number | null; boolValue?: boolean | null; stringValue?: string | null }[]; displayOrder?: number }) =>
      addWorkoutTemplateItem(data.templateId, {
        activityTypeId: data.activityTypeId,
        attributeValues: data.attributeValues,
        displayOrder: data.displayOrder ?? 0
      }),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({ queryKey: ['workout-template', vars.templateId] });
    }
  });
}

export function useRemoveWorkoutTemplateItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: { templateId: string; activityTypeId: string }) =>
      removeWorkoutTemplateItem(data.templateId, data.activityTypeId),
    onSuccess: (_, vars) => {
      queryClient.invalidateQueries({ queryKey: ['workout-template', vars.templateId] });
    }
  });
}

export function useDeleteWorkoutTemplateMutation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteWorkoutTemplate,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workout-templates'] });
    }
  });
}
