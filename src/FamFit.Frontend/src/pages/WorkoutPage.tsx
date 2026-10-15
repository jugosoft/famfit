import { useState, useEffect } from 'react';
import { format } from 'date-fns';
import { ru } from 'date-fns/locale';
import {
  Dumbbell,
  Plus,
  CheckCircle2,
  Trash2,
  Clock,
  Calendar,
  Zap
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { toast } from '@/components/ui/Toaster';
import {
  useWorkouts,
  useCreateWorkout,
  useAddWorkoutEntry,
  useCompleteWorkout,
  useDeleteWorkoutMutation,
  useActivityTypes,
  useAttributeDefinitions
} from '@/hooks/useQueries';
import type { AttributeInput } from '@/api/types';

interface WorkoutEntryForm {
  activityTypeId: string;
  attributeValues: Record<string, string>;
}

export function WorkoutPage() {
  const [showNewWorkout, setShowNewWorkout] = useState(false);
  const [newWorkoutName, setNewWorkoutName] = useState('');
  const [activeWorkoutId, setActiveWorkoutId] = useState<string | null>(null);
  const [entryForm, setEntryForm] = useState<WorkoutEntryForm>({
    activityTypeId: '',
    attributeValues: {}
  });

  const { data: workouts, isLoading: workoutsLoading, refetch } = useWorkouts();
  const { data: activityTypes } = useActivityTypes();
  const { data: attributeDefinitions } = useAttributeDefinitions();
  const createWorkoutMutation = useCreateWorkout();
  const addEntryMutation = useAddWorkoutEntry();
  const completeWorkoutMutation = useCompleteWorkout();
  const deleteWorkoutMutation = useDeleteWorkoutMutation();

  // Find active (incomplete) workout
  const activeWorkout = workouts?.find(w => !w.isCompleted);

  // Sync activeWorkoutId with the active workout from DB
  useEffect(() => {
    if (activeWorkout?.id) {
      setActiveWorkoutId(activeWorkout.id);
    }
  }, [activeWorkout?.id]);

  // Sync attribute values when activity type changes
  useEffect(() => {
    if (entryForm.activityTypeId && attributeDefinitions) {
      const selectedType = activityTypes?.find(t => t.id === entryForm.activityTypeId);
      const initialValues: Record<string, string> = {};
      if (selectedType) {
        for (const def of attributeDefinitions) {
          const attr = selectedType.attributes.find(a => a.definitionName === def.name);
          initialValues[def.name] = attr && attr.value !== null && attr.value !== undefined
            ? String(attr.value)
            : '';
        }
      }
      setEntryForm(prev => ({ ...prev, attributeValues: initialValues }));
    } else {
      setEntryForm(prev => ({ ...prev, attributeValues: {} }));
    }
  }, [entryForm.activityTypeId, activityTypes, attributeDefinitions]);

  function handleCreateWorkout() {
    if (!newWorkoutName.trim()) {
      toast.error('Ошибка', 'Введите название тренировки');
      return;
    }

    createWorkoutMutation.mutate(
      { name: newWorkoutName.trim() },
      {
        onSuccess: (data) => {
          setActiveWorkoutId(data.id);
          setShowNewWorkout(false);
          setNewWorkoutName('');
          toast.success('Тренировка создана');
          refetch();
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function handleAddEntry() {
    if (!activeWorkout?.id || !entryForm.activityTypeId) {
      toast.error('Ошибка', 'Сначала создайте или выберите тренировку');
      return;
    }

    const attributeValues: AttributeInput[] = attributeDefinitions
      ?.filter(def => entryForm.attributeValues[def.name] !== undefined && entryForm.attributeValues[def.name] !== '')
      .map(def => ({
        definitionName: def.name,
        [def.dataType === 'Integer' ? 'intValue' : 
         def.dataType === 'Decimal' ? 'decimalValue' :
         def.dataType === 'Boolean' ? 'boolValue' : 'stringValue']:
          def.dataType === 'Integer' ? parseInt(entryForm.attributeValues[def.name] || '0') :
          def.dataType === 'Decimal' ? parseFloat(entryForm.attributeValues[def.name] || '0') :
          def.dataType === 'Boolean' ? entryForm.attributeValues[def.name]?.toLowerCase() === 'true' :
          entryForm.attributeValues[def.name]
      })) || [];

    addEntryMutation.mutate(
      {
        workoutId: activeWorkout.id,
        activityTypeId: entryForm.activityTypeId,
        attributeValues
      },
      {
        onSuccess: () => {
          toast.success('Запись добавлена');
          setEntryForm({ activityTypeId: '', attributeValues: {} });
          refetch();
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function handleCompleteWorkout() {
    if (!activeWorkoutId) return;

    completeWorkoutMutation.mutate(activeWorkoutId, {
      onSuccess: () => {
        toast.success('Тренировка завершена! Записи добавлены в историю.');
        setActiveWorkoutId(null);
        refetch();
      },
      onError: (error: Error) => {
        toast.error('Ошибка', error.message);
      }
    });
  }

  function handleDeleteWorkout(id: string) {
    if (!confirm('Удалить эту тренировку?')) return;

    deleteWorkoutMutation.mutate(id, {
      onSuccess: () => {
        toast.success('Тренировка удалена');
        if (activeWorkoutId === id) setActiveWorkoutId(null);
        refetch();
      },
      onError: (error: Error) => {
        toast.error('Ошибка', error.message);
      }
    });
  }

  function clearField(defName: string) {
    setEntryForm(prev => ({
      ...prev,
      attributeValues: { ...prev.attributeValues, [defName]: '' }
    }));
  }

  const selectedActivityType = activityTypes?.find(t => t.id === entryForm.activityTypeId);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Тренировки</h1>
          <p className="text-muted-foreground mt-1">Создавайте и отслеживайте тренировки</p>
        </div>
        {!activeWorkout && (
          <Button onClick={() => setShowNewWorkout(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Новая тренировка
          </Button>
        )}
      </div>

      {/* New Workout Dialog */}
      {showNewWorkout && (
        <Card className="border-primary/30">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Zap className="h-5 w-5 text-primary" />
              Новая тренировка
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Input
              label="Название тренировки"
              placeholder="Например: Утренняя тренировка"
              value={newWorkoutName}
              onChange={(e) => setNewWorkoutName(e.target.value)}
              maxLength={200}
              onKeyDown={(e) => {
                if (e.key === 'Enter') handleCreateWorkout();
              }}
            />
            <div className="flex gap-3 justify-end">
              <Button
                variant="outline"
                onClick={() => { setShowNewWorkout(false); setNewWorkoutName(''); }}
              >
                Отмена
              </Button>
              <Button onClick={handleCreateWorkout} isLoading={createWorkoutMutation.isPending}>
                Создать
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Active Workout */}
      {activeWorkout?.id && (
        <Card className="border-primary/30">
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center gap-2">
                <Dumbbell className="h-5 w-5 text-primary" />
                {activeWorkout.name || 'Тренировка'}
                <Badge variant="secondary" className="ml-2">
                  <Clock className="h-3 w-3 mr-1" />
                  В процессе
                </Badge>
              </CardTitle>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleCompleteWorkout}
                  isLoading={completeWorkoutMutation.isPending}
                >
                  <CheckCircle2 className="h-4 w-4 mr-1" />
                  Завершить
                </Button>
              </div>
            </div>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Add Entry Form */}
            <div className="space-y-4">
              <h3 className="text-sm font-medium text-muted-foreground">Добавить активность</h3>

              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <label className="text-sm font-medium leading-none">Тип активности</label>
                  <Select
                    placeholder="Выберите тип..."
                    items={activityTypes?.map((t) => ({
                      value: t.id,
                      label: `${t.name} (${t.unitType === 'Count' ? 'шт.' : 'мин.'})`
                    }))}
                    value={entryForm.activityTypeId}
                    onValueChange={(v) => setEntryForm(f => ({ ...f, activityTypeId: v }))}
                  />
                </div>
              </div>

              {/* Dynamic fields based on ActivityType attributes */}
              {selectedActivityType && attributeDefinitions && (
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                  {selectedActivityType.attributes.map((attr) => {
                    const def = attributeDefinitions.find(d => d.name === attr.definitionName);
                    if (!def) return null;
                    
                    return (
                      <div key={attr.id} className="relative">
                        <label className="text-sm font-medium leading-none">
                          {attr.displayName}
                          {attr.unit && <span className="text-muted-foreground ml-1">({attr.unit})</span>}
                        </label>
                        <div className="flex gap-2 mt-1">
                          <Input
                            type={def.dataType === 'Integer' || def.dataType === 'Decimal' ? 'number' : 'text'}
                            placeholder={attr.displayName}
                            value={entryForm.attributeValues[attr.definitionName] || ''}
                            onChange={(e) => setEntryForm(f => ({
                              ...f,
                              attributeValues: { ...f.attributeValues, [attr.definitionName]: e.target.value }
                            }))}
                            step={def.dataType === 'Decimal' ? '0.1' : undefined}
                            min={def.dataType === 'Integer' || def.dataType === 'Decimal' ? '0' : undefined}
                          />
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => clearField(attr.definitionName)}
                            className="shrink-0"
                          >
                            <X className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}

              <Button
                onClick={handleAddEntry}
                isLoading={addEntryMutation.isPending}
                className="w-full"
              >
                <Plus className="mr-2 h-4 w-4" />
                Добавить запись
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Workout History */}
      <Card>
        <CardHeader>
          <CardTitle>История тренировок</CardTitle>
        </CardHeader>
        <CardContent>
          {workoutsLoading ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center justify-between p-3 rounded-lg border">
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-full bg-muted animate-pulse" />
                    <div className="space-y-2">
                      <div className="h-4 w-32 bg-muted rounded animate-pulse" />
                      <div className="h-3 w-24 bg-muted rounded animate-pulse" />
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : !workouts || workouts.length === 0 ? (
            <EmptyState
              title="Нет тренировок"
              description="Создайте первую тренировку, чтобы отслеживать прогресс"
              action={
                <Button onClick={() => setShowNewWorkout(true)}>
                  <Plus className="mr-2 h-4 w-4" />
                  Создать тренировку
                </Button>
              }
            />
          ) : (
            <div className="space-y-2">
              {workouts.map((workout) => (
                <div
                  key={workout.id}
                  className="flex items-center justify-between p-3 rounded-lg border hover:bg-muted/30 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <div className={`flex h-10 w-10 items-center justify-center rounded-full ${
                      workout.isCompleted
                        ? 'bg-green-100 dark:bg-green-900/30'
                        : 'bg-primary/10'
                    }`}>
                      {workout.isCompleted ? (
                        <CheckCircle2 className="h-5 w-5 text-green-600" />
                      ) : (
                        <Clock className="h-5 w-5 text-primary" />
                      )}
                    </div>
                    <div>
                      <p className="font-medium text-sm">{workout.name || 'Тренировка'}</p>
                      <p className="text-xs text-muted-foreground flex items-center gap-1">
                        <Calendar className="h-3 w-3" />
                        {format(new Date(workout.startTime), "d MMM yyyy, HH:mm", { locale: ru })}
                        {workout.endTime && (
                          <>
                            {' — '}
                            {format(new Date(workout.endTime), "HH:mm")}
                          </>
                        )}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={workout.isCompleted ? 'default' : 'secondary'}>
                      {workout.isCompleted ? 'Завершена' : 'В процессе'}
                    </Badge>
                    {!workout.isCompleted && (
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDeleteWorkout(workout.id)}
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
