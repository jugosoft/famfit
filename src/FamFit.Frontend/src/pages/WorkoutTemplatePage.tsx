import { useState } from 'react';
import {
  Dumbbell,
  Plus,
  Trash2,
  X,
  FileText,
  Play
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { toast } from '@/components/ui/Toaster';
import {
  useWorkoutTemplates,
  useWorkoutTemplate,
  useCreateWorkoutTemplate,
  useAddWorkoutTemplateItem,
  useRemoveWorkoutTemplateItem,
  useDeleteWorkoutTemplateMutation,
  useCreateWorkout,
  useActivityTypes
} from '@/hooks/useQueries';

export function WorkoutTemplatePage() {
  const [showNewTemplate, setShowNewTemplate] = useState(false);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
  const [newTemplateName, setNewTemplateName] = useState('');
  const [newTemplateDesc, setNewTemplateDesc] = useState('');
  const [selectedActivityTypeId, setSelectedActivityTypeId] = useState('');
  const [templateSets, setTemplateSets] = useState('');
  const [templateReps, setTemplateReps] = useState('');
  const [templateWeight, setTemplateWeight] = useState('');

  const { data: templates, isLoading: templatesLoading, refetch: refetchTemplates } = useWorkoutTemplates();
  const { data: template, isLoading: templateLoading } = useWorkoutTemplate(selectedTemplateId ?? '');
  const { data: activityTypes } = useActivityTypes();
  const createTemplateMutation = useCreateWorkoutTemplate();
  const addTemplateItemMutation = useAddWorkoutTemplateItem();
  const removeTemplateItemMutation = useRemoveWorkoutTemplateItem();
  const deleteTemplateMutation = useDeleteWorkoutTemplateMutation();
  const createWorkoutMutation = useCreateWorkout();

  const selectedActivityType = activityTypes?.find(t => t.id === selectedActivityTypeId);

  function handleCreateTemplate() {
    if (!newTemplateName.trim()) {
      toast.error('Ошибка', 'Введите название шаблона');
      return;
    }

    createTemplateMutation.mutate(
      { name: newTemplateName.trim(), description: newTemplateDesc.trim() || undefined },
      {
        onSuccess: (data) => {
          setSelectedTemplateId(data.id);
          setShowNewTemplate(false);
          setNewTemplateName('');
          setNewTemplateDesc('');
          toast.success('Шаблон создан');
          refetchTemplates();
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function handleAddItem() {
    if (!selectedTemplateId || !selectedActivityTypeId) {
      toast.error('Ошибка', 'Выберите тип активности');
      return;
    }

    addTemplateItemMutation.mutate(
      {
        templateId: selectedTemplateId,
        activityTypeId: selectedActivityTypeId,
        sets: templateSets ? parseInt(templateSets) : null,
        reps: templateReps ? parseInt(templateReps) : null,
        weight: templateWeight ? parseInt(templateWeight) : null
      },
      {
        onSuccess: () => {
          toast.success('Элемент добавлен');
          setSelectedActivityTypeId('');
          setTemplateSets('');
          setTemplateReps('');
          setTemplateWeight('');
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function handleRemoveItem(activityTypeId: string) {
    if (!selectedTemplateId) return;

    removeTemplateItemMutation.mutate(
      { templateId: selectedTemplateId, activityTypeId },
      {
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function handleDeleteTemplate(id: string) {
    if (!confirm('Удалить этот шаблон?')) return;

    deleteTemplateMutation.mutate(id, {
      onSuccess: () => {
        toast.success('Шаблон удалён');
        if (selectedTemplateId === id) setSelectedTemplateId(null);
        refetchTemplates();
      },
      onError: (error: Error) => {
        toast.error('Ошибка', error.message);
      }
    });
  }

  function handleCreateWorkoutFromTemplate() {
    if (!template) return;

    createWorkoutMutation.mutate(
      { name: template.name },
      {
        onSuccess: () => {
          toast.success(`Тренировка "${template.name}" создана!`);
          // Navigate to workout page
          window.location.hash = '#/workout';
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function clearField(setter: (v: string) => void) {
    setter('');
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Шаблоны тренировок</h1>
          <p className="text-muted-foreground mt-1">Создавайте шаблоны для быстрых тренировок</p>
        </div>
        <Button onClick={() => setShowNewTemplate(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Новый шаблон
        </Button>
      </div>

      {/* New Template Dialog */}
      {showNewTemplate && (
        <Card className="border-primary/30">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5 text-primary" />
              Новый шаблон
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Input
              label="Название шаблона"
              placeholder="Например: Утренняя тренировка"
              value={newTemplateName}
              onChange={(e) => setNewTemplateName(e.target.value)}
              maxLength={200}
              onKeyDown={(e) => {
                if (e.key === 'Enter') handleCreateTemplate();
              }}
            />
            <Input
              label="Описание (необязательно)"
              placeholder="Краткое описание шаблона"
              value={newTemplateDesc}
              onChange={(e) => setNewTemplateDesc(e.target.value)}
              maxLength={1000}
            />
            <div className="flex gap-3 justify-end">
              <Button
                variant="outline"
                onClick={() => { setShowNewTemplate(false); setNewTemplateName(''); setNewTemplateDesc(''); }}
              >
                Отмена
              </Button>
              <Button onClick={handleCreateTemplate} isLoading={createTemplateMutation.isPending}>
                Создать
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Templates List */}
        <div className="lg:col-span-1">
          <Card>
            <CardHeader>
              <CardTitle>Шаблоны</CardTitle>
            </CardHeader>
            <CardContent>
              {templatesLoading ? (
                <div className="space-y-2">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="h-12 bg-muted rounded animate-pulse" />
                  ))}
                </div>
              ) : !templates || templates.length === 0 ? (
                <EmptyState
                  title="Нет шаблонов"
                  description="Создайте первый шаблон для быстрых тренировок"
                />
              ) : (
                <div className="space-y-2">
                  {templates.map((t) => (
                    <div
                      key={t.id}
                      className={`flex items-center justify-between p-3 rounded-lg border cursor-pointer transition-colors ${
                        selectedTemplateId === t.id
                          ? 'border-primary bg-primary/5'
                          : 'hover:bg-muted/30'
                      }`}
                      onClick={() => setSelectedTemplateId(t.id)}
                    >
                      <div className="flex items-center gap-3 flex-1 min-w-0">
                        <Dumbbell className="h-4 w-4 text-muted-foreground shrink-0" />
                        <div className="min-w-0">
                          <p className="font-medium text-sm truncate">{t.name}</p>
                          <p className="text-xs text-muted-foreground">{t.itemsCount} активностей</p>
                        </div>
                      </div>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={(e) => { e.stopPropagation(); handleDeleteTemplate(t.id); }}
                        className="shrink-0"
                      >
                        <Trash2 className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Template Detail */}
        <div className="lg:col-span-2">
          {templateLoading ? (
            <Card>
              <CardContent className="space-y-3 p-6">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="h-12 bg-muted rounded animate-pulse" />
                ))}
              </CardContent>
            </Card>
          ) : template ? (
            <Card>
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div>
                    <CardTitle className="flex items-center gap-2">
                      <FileText className="h-5 w-5 text-primary" />
                      {template.name}
                    </CardTitle>
                    {template.description && (
                      <p className="text-sm text-muted-foreground mt-1">{template.description}</p>
                    )}
                  </div>
                  <Button onClick={handleCreateWorkoutFromTemplate}>
                    <Play className="h-4 w-4 mr-1" />
                    Начать тренировку
                  </Button>
                </div>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Template Items */}
                <div>
                  <h3 className="text-sm font-medium text-muted-foreground mb-3">Элементы шаблона</h3>
                  {template.items.length === 0 ? (
                    <EmptyState
                      title="Нет элементов"
                      description="Добавьте типы активностей в шаблон"
                    />
                  ) : (
                    <div className="space-y-2">
                      {template.items
                        .sort((a, b) => a.displayOrder - b.displayOrder)
                        .map((item) => (
                          <div
                            key={item.id}
                            className="flex items-center justify-between p-3 rounded-lg border"
                          >
                            <div className="flex items-center gap-3">
                              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-sm font-medium text-primary">
                                {item.displayOrder}
                              </div>
                              <div>
                                <p className="font-medium text-sm">{item.activityType?.name ?? 'Неизвестно'}</p>
                                <div className="flex gap-2 text-xs text-muted-foreground">
                                  {item.sets && <Badge variant="secondary">Подходы: {item.sets}</Badge>}
                                  {item.reps && <Badge variant="secondary">Повторения: {item.reps}</Badge>}
                                  {item.weight && <Badge variant="secondary">Вес: {item.weight} кг</Badge>}
                                </div>
                              </div>
                            </div>
                            <Button
                              variant="ghost"
                              size="icon"
                              onClick={() => handleRemoveItem(item.activityTypeId)}
                            >
                              <X className="h-4 w-4" />
                            </Button>
                          </div>
                        ))}
                    </div>
                  )}
                </div>

                {/* Add Item Form */}
                <div className="space-y-4 pt-4 border-t">
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
                        value={selectedActivityTypeId}
                        onValueChange={setSelectedActivityTypeId}
                      />
                    </div>
                  </div>

                  {/* Dynamic fields based on ActivityTypeAttributes */}
                  {selectedActivityType?.attributes &&
                    (selectedActivityType.attributes.sets || selectedActivityType.attributes.reps || selectedActivityType.attributes.weight) && (
                      <div className="grid gap-4 md:grid-cols-3">
                        {selectedActivityType.attributes.sets && (
                          <div className="relative">
                            <label className="text-sm font-medium leading-none">Подходы</label>
                            <div className="flex gap-2 mt-1">
                              <Input
                                type="number"
                                placeholder={selectedActivityType.attributes.sets?.toString() || ''}
                                value={templateSets}
                                onChange={(e) => setTemplateSets(e.target.value)}
                                min="0"
                              />
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => clearField(setTemplateSets)}
                                className="shrink-0"
                              >
                                <X className="h-4 w-4" />
                              </Button>
                            </div>
                          </div>
                        )}
                        {selectedActivityType.attributes.reps && (
                          <div className="relative">
                            <label className="text-sm font-medium leading-none">Повторения</label>
                            <div className="flex gap-2 mt-1">
                              <Input
                                type="number"
                                placeholder={selectedActivityType.attributes.reps?.toString() || ''}
                                value={templateReps}
                                onChange={(e) => setTemplateReps(e.target.value)}
                                min="0"
                              />
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => clearField(setTemplateReps)}
                                className="shrink-0"
                              >
                                <X className="h-4 w-4" />
                              </Button>
                            </div>
                          </div>
                        )}
                        {selectedActivityType.attributes.weight && (
                          <div className="relative">
                            <label className="text-sm font-medium leading-none">Вес (кг)</label>
                            <div className="flex gap-2 mt-1">
                              <Input
                                type="number"
                                placeholder={selectedActivityType.attributes.weight?.toString() || ''}
                                value={templateWeight}
                                onChange={(e) => setTemplateWeight(e.target.value)}
                                min="0"
                              />
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => clearField(setTemplateWeight)}
                                className="shrink-0"
                              >
                                <X className="h-4 w-4" />
                              </Button>
                            </div>
                          </div>
                        )}
                      </div>
                    )}

                  <Button
                    onClick={handleAddItem}
                    isLoading={addTemplateItemMutation.isPending}
                    className="w-full"
                  >
                    <Plus className="mr-2 h-4 w-4" />
                    Добавить в шаблон
                  </Button>
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="flex items-center justify-center py-12">
                <div className="text-center">
                  <Dumbbell className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <p className="text-muted-foreground">Выберите шаблон или создайте новый</p>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
