import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
  useActivityTypes,
  useAttributeDefinitions
} from '@/hooks/useQueries';
import type { AttributeInput } from '@/api/types';

export function WorkoutTemplatePage() {
  const { templateId } = useParams<{ templateId: string }>();
  const navigate = useNavigate();
  const [showNewTemplate, setShowNewTemplate] = useState(false);
  const [newTemplateName, setNewTemplateName] = useState('');
  const [newTemplateDesc, setNewTemplateDesc] = useState('');
  const [selectedActivityTypeId, setSelectedActivityTypeId] = useState('');
  const [attributeValues, setAttributeValues] = useState<Record<string, string>>({});

  const { data: templates, isLoading: templatesLoading, refetch: refetchTemplates } = useWorkoutTemplates();
  const { data: template, isLoading: templateLoading } = useWorkoutTemplate(templateId ?? '');
  const { data: activityTypes } = useActivityTypes();
  const { data: attributeDefinitions } = useAttributeDefinitions();
  const createTemplateMutation = useCreateWorkoutTemplate();
  const addTemplateItemMutation = useAddWorkoutTemplateItem();
  const removeTemplateItemMutation = useRemoveWorkoutTemplateItem();
  const deleteTemplateMutation = useDeleteWorkoutTemplateMutation();
  const createWorkoutMutation = useCreateWorkout();

  const selectedActivityType = activityTypes?.find(t => t.id === selectedActivityTypeId);

  // Sync attribute values when activity type changes
  useEffect(() => {
    if (selectedActivityTypeId && attributeDefinitions) {
      const selectedType = activityTypes?.find(t => t.id === selectedActivityTypeId);
      const initialValues: Record<string, string> = {};
      if (selectedType) {
        for (const def of attributeDefinitions) {
          const attr = selectedType.attributes.find(a => a.definitionName === def.name);
          initialValues[def.name] = attr && attr.value !== null && attr.value !== undefined
            ? String(attr.value)
            : '';
        }
      }
      setAttributeValues(initialValues);
    } else {
      setAttributeValues({});
    }
  }, [selectedActivityTypeId, activityTypes, attributeDefinitions]);

  function handleCreateTemplate() {
    if (!newTemplateName.trim()) {
      toast.error('Ошибка', 'Введите название шаблона');
      return;
    }

    createTemplateMutation.mutate(
      { name: newTemplateName.trim(), description: newTemplateDesc.trim() || undefined },
      {
        onSuccess: (data) => {
          navigate(`/templates/${data.id}`);
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

    const attributeValuesToSend: AttributeInput[] = attributeDefinitions
      ?.filter(def => attributeValues[def.name] !== undefined && attributeValues[def.name] !== '')
      .map(def => ({
        definitionName: def.name,
        [def.dataType === 'Integer' ? 'intValue' : 
         def.dataType === 'Decimal' ? 'decimalValue' :
         def.dataType === 'Boolean' ? 'boolValue' : 'stringValue']:
          def.dataType === 'Integer' ? parseInt(attributeValues[def.name] || '0') :
          def.dataType === 'Decimal' ? parseFloat(attributeValues[def.name] || '0') :
          def.dataType === 'Boolean' ? attributeValues[def.name]?.toLowerCase() === 'true' :
          attributeValues[def.name]
      })) || [];

    addTemplateItemMutation.mutate(
      {
        templateId: selectedTemplateId,
        activityTypeId: selectedActivityTypeId,
        attributeValues: attributeValuesToSend,
        displayOrder: (template?.items.length ?? 0) + 1
      },
      {
        onSuccess: () => {
          toast.success('Элемент добавлен');
          setSelectedActivityTypeId('');
          setAttributeValues({});
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
        if (templateId === id) navigate('/templates');
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
          navigate('/workout');
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function clearField(defName: string) {
    setAttributeValues(prev => ({ ...prev, [defName]: '' }));
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
                        templateId === t.id
                          ? 'border-primary bg-primary/5'
                          : 'hover:bg-muted/30'
                      }`}
                      onClick={() => navigate(`/templates/${t.id}`)}
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
                                <div className="flex flex-wrap gap-2 text-xs text-muted-foreground">
                                  {item.attributes.map((attr) => (
                                    <Badge key={attr.id} variant="secondary">
                                      {attr.displayName}: {attr.value !== null && attr.value !== undefined
                                        ? `${attr.value}${attr.unit ? ` ${attr.unit}` : ''}`
                                        : '—'}
                                    </Badge>
                                  ))}
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
                                value={attributeValues[attr.definitionName] || ''}
                                onChange={(e) => setAttributeValues(prev => ({ ...prev, [attr.definitionName]: e.target.value }))}
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
