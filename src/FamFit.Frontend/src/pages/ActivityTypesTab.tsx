import { useState, useEffect } from 'react';
import { Plus, X, Check, ChevronDown } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { EmptyState } from '@/components/ui/EmptyState';
import { toast } from '@/components/ui/Toaster';
import {
  createActivityType,
  deactivateActivityType,
  updateActivityType,
  updateActivityTypeAttributes,
} from '@/api/client';
import type { AttributeDefinitionDto } from '@/api/types';

// Types
interface ActivityTypeItem {
  id: string;
  name: string;
  isActive?: boolean;
  attributes: {
    id: string;
    definitionName: string;
    displayName: string;
    dataType: 'Integer' | 'Decimal' | 'Boolean' | 'String';
    unit?: string | null;
    value: unknown;
  }[];
}

interface ActivityTypesTabProps {
  activityTypes: ActivityTypeItem[];
  attributeDefinitions: AttributeDefinitionDto[];
  isLoading: boolean;
  pendingCreateTab: string | null;
  onCreate: () => void;
}

interface SelectedAttribute {
  definitionName: string;
  displayName: string;
  dataType: 'Integer' | 'Decimal' | 'Boolean' | 'String';
  unit?: string | null;
}

export function ActivityTypesTab({
  activityTypes,
  attributeDefinitions,
  isLoading,
  pendingCreateTab,
  onCreate,
}: ActivityTypesTabProps) {
  const queryClient = useQueryClient();

  const [showDialog, setShowDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleteName, setDeleteName] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formName, setFormName] = useState('');
  const [selectedAttributes, setSelectedAttributes] = useState<SelectedAttribute[]>([]);
  const [isCreating, setIsCreating] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [showAttributeSelector, setShowAttributeSelector] = useState(false);

  const isEditing = editingId !== null;

  // Open create dialog when pendingCreateTab matches
  useEffect(() => {
    if (pendingCreateTab === 'activity-types') {
      openCreateDialog();
      onCreate();
    }
  }, [pendingCreateTab]);

  // Sync form with selected item when editing
  useEffect(() => {
    if (editingId) {
      const item = activityTypes.find(t => t.id === editingId);
      if (item) {
        setFormName(item.name);
        setSelectedAttributes(
          item.attributes.map(attr => ({
            definitionName: attr.definitionName,
            displayName: attr.displayName,
            dataType: attr.dataType,
            unit: attr.unit,
          }))
        );
      }
    }
  }, [editingId, activityTypes]);

  function openCreateDialog() {
    setEditingId(null);
    setFormName('');
    setSelectedAttributes([]);
    setShowDialog(true);
    setShowAttributeSelector(false);
  }

  function openEditDialog(item: ActivityTypeItem) {
    setEditingId(item.id);
    setFormName(item.name);
    setSelectedAttributes(
      item.attributes.map(attr => ({
        definitionName: attr.definitionName,
        displayName: attr.displayName,
        dataType: attr.dataType,
        unit: attr.unit,
      }))
    );
    setShowDialog(true);
    setShowAttributeSelector(false);
  }

  function openDeleteDialog(id: string, name: string) {
    setDeleteId(id);
    setDeleteName(name);
    setShowDeleteDialog(true);
  }

  function getAvailableDefinitions() {
    return attributeDefinitions.filter(
      def => !selectedAttributes.some(attr => attr.definitionName === def.name)
    );
  }

  function addAttribute(definition: AttributeDefinitionDto) {
    setSelectedAttributes(prev => [
      ...prev,
      {
        definitionName: definition.name,
        displayName: definition.displayName,
        dataType: definition.dataType,
        unit: definition.unit,
      },
    ]);
    setShowAttributeSelector(false);
  }

  function removeAttribute(definitionName: string) {
    setSelectedAttributes(prev => prev.filter(attr => attr.definitionName !== definitionName));
  }

  async function handleSave() {
    if (!formName.trim()) {
      toast.error('Ошибка', 'Введите название типа активности');
      return;
    }

    try {
      if (isEditing && editingId) {
        // Update name
        setIsSaving(true);
        await updateActivityType(editingId, { name: formName.trim() });

        // Update attributes
        await updateActivityTypeAttributes(editingId, {
          attributeValues: selectedAttributes.map(attr => ({
            definitionName: attr.definitionName,
            stringValue: '',
          })),
        });

        toast.success('Тип обновлён');
      } else {
        // Create new type
        setIsCreating(true);

        // Add attributes if any
        if (selectedAttributes.length > 0) {
          await createActivityTypeWithAttributes(formName.trim(), selectedAttributes);
        } else {
          await createActivityType({ name: formName.trim() });
        }

        toast.success('Тип создан');
      }

      setShowDialog(false);
      setSelectedAttributes([]);
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось сохранить тип');
    } finally {
      setIsCreating(false);
      setIsSaving(false);
    }
  }

  async function createActivityTypeWithAttributes(name: string, attributes: SelectedAttribute[]) {
    // Create the type first
    const response = await createActivityType({ name });

    // Then add attributes
    const attributeValues = attributes.map(attr => ({
      definitionName: attr.definitionName,
      stringValue: '',
    }));

    await updateActivityTypeAttributes(response.id, { attributeValues });
  }

  async function confirmDelete() {
    if (!deleteId) return;
    setIsDeleting(true);

    try {
      await deactivateActivityType(deleteId);
      toast.success('Тип деактивирован');
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось деактивировать тип');
    } finally {
      setIsDeleting(false);
      setShowDeleteDialog(false);
    }
  }

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Типы активностей</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-2">
              {[1, 2, 3].map((i) => (
                <div key={i} className="h-12 bg-muted rounded animate-pulse" />
              ))}
            </div>
          ) : activityTypes.length === 0 ? (
            <EmptyState
              title="Нет типов активностей"
              description="Добавьте первый тип активности для начала работы"
              action={
                <Button onClick={openCreateDialog}>
                  <Plus className="mr-2 h-4 w-4" />
                  Добавить
                </Button>
              }
            />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Название</TableHead>
                  <TableHead>Атрибуты</TableHead>
                  <TableHead>Статус</TableHead>
                  <TableHead className="text-right">Действия</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {activityTypes.map((item) => (
                  <TableRow
                    key={item.id}
                    onClick={() => openEditDialog(item)}
                    className="cursor-pointer hover:bg-muted/50"
                  >
                    <TableCell className="font-medium">{item.name}</TableCell>
                    <TableCell>
                      {item.attributes.length > 0 ? (
                        <div className="flex flex-wrap gap-1">
                          {item.attributes.map((attr) => (
                            <Badge key={attr.id} variant="secondary" className="text-xs">
                              {attr.displayName}
                            </Badge>
                          ))}
                        </div>
                      ) : (
                        <span className="text-muted-foreground">Не заданы</span>
                      )}
                    </TableCell>
                    <TableCell>
                      <Badge variant={item.isActive !== false ? 'default' : 'secondary'}>
                        {item.isActive !== false ? 'Активен' : 'Неактивен'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openDeleteDialog(item.id, item.name)}
                          title="Деактивировать"
                        >
                          <X className="h-4 w-4 text-destructive" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <Dialog
        open={showDialog}
        onOpenChange={setShowDialog}
        title={isEditing ? 'Редактировать тип' : 'Новый тип активности'}
        description={isEditing ? 'Измените название и атрибуты' : 'Создайте новый тип активности для семьи'}
      >
        <div className="space-y-4">
          <Input
            label="Название"
            placeholder="Например: Отжимания"
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            maxLength={100}
          />

          {/* Selected attributes */}
          <div className="border border-border rounded-lg p-3 space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium leading-none">Атрибуты</label>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setShowAttributeSelector(!showAttributeSelector)}
                disabled={getAvailableDefinitions().length === 0}
              >
                <Plus className="mr-1 h-3 w-3" />
                Добавить атрибут
                <ChevronDown className={`ml-1 h-3 w-3 transition-transform ${showAttributeSelector ? 'rotate-180' : ''}`} />
              </Button>
            </div>

            {/* Attribute selector dropdown */}
            {showAttributeSelector && (
              <div className="space-y-1 mt-2 border border-border rounded-md p-2 max-h-40 overflow-y-auto">
                {getAvailableDefinitions().length === 0 ? (
                  <p className="text-xs text-muted-foreground">Все атрибуты уже добавлены</p>
                ) : (
                  getAvailableDefinitions().map((def) => (
                    <button
                      key={def.id}
                      className="w-full text-left px-2 py-1.5 text-sm rounded hover:bg-muted flex items-center justify-between"
                      onClick={() => addAttribute(def)}
                    >
                      <span>{def.displayName}</span>
                      <span className="text-xs text-muted-foreground">
                        {def.unit && `(${def.unit})`}
                      </span>
                    </button>
                  ))
                )}
              </div>
            )}

            {/* Selected attributes list */}
            {selectedAttributes.length > 0 && (
              <div className="flex flex-wrap gap-1 mt-2">
                {selectedAttributes.map((attr) => (
                  <Badge key={attr.definitionName} variant="secondary" className="text-xs flex items-center gap-1">
                    {attr.displayName}
                    <button
                      onClick={() => removeAttribute(attr.definitionName)}
                      className="ml-1 hover:text-destructive"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            )}
          </div>

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleSave} isLoading={isCreating || isSaving}>
              <Check className="mr-2 h-4 w-4" />
              {isEditing ? 'Сохранить' : 'Создать'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={showDeleteDialog}
        onOpenChange={setShowDeleteDialog}
        title="Деактивировать тип активности"
        description={`Вы уверены, что хотите деактивировать "${deleteName}"? Это действие можно отменить.`}
        confirmText="Деактивировать"
        cancelText="Отмена"
        variant="destructive"
        onConfirm={confirmDelete}
        isLoading={isDeleting}
      />
    </>
  );
}
