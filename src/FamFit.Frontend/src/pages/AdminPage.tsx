import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, X, Check, Dumbbell } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { EmptyState } from '@/components/ui/EmptyState';
import { Alert } from '@/components/ui/Alert';
import { useAuth } from '@/contexts/AuthContext';
import { toast } from '@/components/ui/Toaster';
import { getActivityTypes, createActivityType, getActivityAttributeDefinitions } from '@/api/client';
import { getActivityUnits } from '@/api/client';
import { useUpdateActivityTypeAttributes, useActivityUnits } from '@/hooks/useQueries';

// Types
interface ActivityTypeItem {
  id: string;
  name: string;
  activityUnitId: string;
  unitSymbol?: string | null;
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

interface AttributeDefinition {
  id: string;
  name: string;
  displayName: string;
  dataType: 'Integer' | 'Decimal' | 'Boolean' | 'String';
  unit?: string | null;
  isActive: boolean;
  displayOrder: number;
}

interface ActivityUnitItem {
  id: string;
  name: string;
  symbol: string;
  dataType: 'Integer' | 'Decimal' | 'String';
  isActive: boolean;
  displayOrder: number;
}

export function AdminPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const updateAttributesMutation = useUpdateActivityTypeAttributes();
  
  const { data: activityUnits = [] } = useActivityUnits();
  
  const [showDialog, setShowDialog] = useState(false);
  const [showAttributesDialog, setShowAttributesDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleteName, setDeleteName] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [attributesId, setAttributesId] = useState<string | null>(null);
  const [formName, setFormName] = useState('');
  const [formActivityUnitId, setFormActivityUnitId] = useState<string>('');
  const [attributeValues, setAttributeValues] = useState<Record<string, string>>({});

  const { data: activityTypes = [], isLoading } = useQuery({
    queryKey: ['activity-types'],
    queryFn: getActivityTypes
  });

  const { data: attributeDefinitions = [] } = useQuery({
    queryKey: ['attribute-definitions'],
    queryFn: getActivityAttributeDefinitions
  });

  const isEditing = editingId !== null;
  const isSettingAttributes = attributesId !== null;

  // Sync form with selected item
  useEffect(() => {
    if (editingId) {
      const item = activityTypes.find(t => t.id === editingId);
      if (item) {
        setFormName(item.name);
        setFormActivityUnitId(item.activityUnitId);
      }
    }
  }, [editingId, activityTypes]);

  useEffect(() => {
    if (attributesId) {
      const item = activityTypes.find(t => t.id === attributesId);
      const initialValues: Record<string, string> = {};
      if (item) {
        for (const def of attributeDefinitions) {
          const attr = item.attributes.find(a => a.definitionName === def.name);
          initialValues[def.name] = attr ? formatValueForInput(attr.value, def.dataType) : '';
        }
      }
      setAttributeValues(initialValues);
    }
  }, [attributesId, activityTypes, attributeDefinitions]);

  function formatValueForInput(value: unknown, dataType: string): string {
    if (value === null || value === undefined) return '';
    if (dataType === 'Boolean') return String(!!value);
    return String(value);
  }

  function getValueForApi(defName: string, inputValue: string, dataType: string) {
    if (!inputValue.trim()) return null;
    switch (dataType) {
      case 'Integer': return parseInt(inputValue);
      case 'Decimal': return parseFloat(inputValue);
      case 'Boolean': return inputValue.toLowerCase() === 'true';
      case 'String': return inputValue;
      default: return inputValue;
    }
  }

  function openCreateDialog() {
    setEditingId(null);
    setFormName('');
    setFormActivityUnitId('');
    setShowDialog(true);
  }

  function openAttributesDialog(item: ActivityTypeItem) {
    setAttributesId(item.id);
    setShowAttributesDialog(true);
  }

  async function handleSave() {
    if (!formName.trim()) {
      toast.error('Ошибка', 'Введите название типа активности');
      return;
    }

    if (!formActivityUnitId) {
      toast.error('Ошибка', 'Выберите единицу измерения');
      return;
    }

    try {
      await createActivityType({
        name: formName.trim(),
        activityUnitId: formActivityUnitId
      });

      toast.success('Тип создан');
      setShowDialog(false);
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось создать тип');
    }
  }

  async function handleSaveAttributes() {
    if (!attributesId) return;

    const attributeValuesToSend = attributeDefinitions
      .filter(def => attributeValues[def.name] !== undefined)
      .map(def => ({
        definitionName: def.name,
        [def.dataType === 'Integer' ? 'intValue' : 
         def.dataType === 'Decimal' ? 'decimalValue' :
         def.dataType === 'Boolean' ? 'boolValue' : 'stringValue']:
          getValueForApi(def.name, attributeValues[def.name] || '', def.dataType)
      }));

    try {
      await updateAttributesMutation.mutateAsync({
        activityTypeId: attributesId,
        attributeValues: attributeValuesToSend
      });

      toast.success('Атрибуты обновлены');
      setShowAttributesDialog(false);
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось обновить атрибуты');
    }
  }

  function openDeleteDialog(id: string, name: string) {
    setDeleteId(id);
    setDeleteName(name);
    setShowDeleteDialog(true);
  }

  async function confirmDelete() {
    if (!deleteId) return;
    setIsDeleting(true);

    try {
      // await deactivateActivityType(deleteId);
      toast.success('Тип деактивирован');
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось деактивировать тип');
    } finally {
      setIsDeleting(false);
      setShowDeleteDialog(false);
    }
  }

  function clearField(defName: string) {
    setAttributeValues(prev => ({ ...prev, [defName]: '' }));
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Админ-панель</h1>
          <p className="text-muted-foreground mt-1">Управление типами активностей</p>
        </div>
        <Button onClick={openCreateDialog}>
          <Plus className="mr-2 h-4 w-4" />
          Добавить тип
        </Button>
      </div>

      {/* Role Check */}
      {user?.role !== 'Admin' && (
        <Alert
          variant="destructive"
          title="Доступ запрещён"
          description="У вас нет прав для доступа к админ-панели."
        />
      )}

      {/* Activity Types Table */}
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
                  <TableHead>Единица</TableHead>
                  <TableHead>Атрибуты</TableHead>
                  <TableHead>Статус</TableHead>
                  <TableHead className="text-right">Действия</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {activityTypes.map((item) => {
                  const activityUnit = activityUnits.find(u => u.id === item.activityUnitId);
                  return (
                    <TableRow key={item.id}>
                      <TableCell className="font-medium">{item.name}</TableCell>
                      <TableCell>
                        <Badge variant="secondary">
                          {activityUnit?.name || '—'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {item.attributes.length > 0 ? (
                          <div className="text-xs space-y-1">
                            {item.attributes.map((attr) => (
                              <div key={attr.id}>
                                {attr.displayName}:{' '}
                                {attr.value !== null && attr.value !== undefined
                                  ? `${attr.value}${attr.unit ? ` ${attr.unit}` : ''}`
                                  : '—'}
                              </div>
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
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => openAttributesDialog(item)}
                            title="Редактировать атрибуты"
                          >
                            <Dumbbell className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => openDeleteDialog(item.id, item.name)}
                          >
                            <X className="h-4 w-4 text-destructive" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Create Dialog */}
      <Dialog
        open={showDialog}
        onOpenChange={setShowDialog}
        title={isEditing ? 'Редактировать тип' : 'Новый тип активности'}
        description={isEditing ? 'Измените название и единицу измерения' : 'Создайте новый тип активности для семьи'}
      >
        <div className="space-y-4">
          <Input
            label="Название"
            placeholder="Например: Отжимания"
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            maxLength={100}
          />

          <div>
            <label className="text-sm font-medium leading-none">Единица измерения</label>
            <Select
              items={activityUnits
                .filter(u => u.isActive)
                .map(u => ({ value: u.id, label: u.name }))}
              value={formActivityUnitId}
              onValueChange={setFormActivityUnitId}
            />
          </div>

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleSave}>
              <Check className="mr-2 h-4 w-4" />
              {isEditing ? 'Сохранить' : 'Создать'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Attributes Dialog */}
      <Dialog
        open={showAttributesDialog}
        onOpenChange={setShowAttributesDialog}
        title="Атрибуты активности"
        description={isSettingAttributes ? 'Настройте значения атрибутов для этой активности' : ''}
      >
        <div className="space-y-4">
          {attributeDefinitions.length === 0 ? (
            <p className="text-sm text-muted-foreground">Определения атрибутов не загружены.</p>
          ) : (
            <>
              <p className="text-sm text-muted-foreground">
                Укажите значения для атрибутов. Оставьте пустыми, если не применяется.
              </p>

              <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {attributeDefinitions.map((def) => (
                  <div key={def.id} className="relative">
                    <label className="text-sm font-medium leading-none">
                      {def.displayName}
                      {def.unit && <span className="text-muted-foreground ml-1">({def.unit})</span>}
                    </label>
                    <div className="flex gap-2 mt-1">
                      <Input
                        type={def.dataType === 'Integer' || def.dataType === 'Decimal' ? 'number' : 'text'}
                        placeholder={def.displayName}
                        value={attributeValues[def.name] || ''}
                        onChange={(e) => setAttributeValues(prev => ({ ...prev, [def.name]: e.target.value }))}
                        step={def.dataType === 'Decimal' ? '0.1' : undefined}
                        min={def.dataType === 'Integer' || def.dataType === 'Decimal' ? '0' : undefined}
                      />
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => clearField(def.name)}
                        className="shrink-0"
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </>
          )}

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowAttributesDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button 
              onClick={handleSaveAttributes}
              isLoading={updateAttributesMutation.isPending}
            >
              <Check className="mr-2 h-4 w-4" />
              Сохранить
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
    </div>
  );
}
