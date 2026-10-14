import { useState, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, X, Check, Dumbbell } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Dialog } from '@/components/ui/Dialog';
import { EmptyState } from '@/components/ui/EmptyState';
import { Alert } from '@/components/ui/Alert';
import { useAuth } from '@/contexts/AuthContext';
import { toast } from '@/components/ui/Toaster';
import { getActivityTypes, createActivityType, deactivateActivityType, updateActivityTypeAttributes } from '@/api/client';

// Types
interface ActivityTypeItem {
  id: string;
  name: string;
  unitType: 'Count' | 'Time';
  isActive?: boolean;
  attributes?: {
    sets?: number | null;
    reps?: number | null;
    weight?: number | null;
  };
}

export function AdminPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [showDialog, setShowDialog] = useState(false);
  const [showAttributesDialog, setShowAttributesDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [attributesId, setAttributesId] = useState<string | null>(null);
  const [formName, setFormName] = useState('');
  const [formUnitType, setFormUnitType] = useState<'Count' | 'Time'>('Count');
  const [formSets, setFormSets] = useState<string>('');
  const [formReps, setFormReps] = useState<string>('');
  const [formWeight, setFormWeight] = useState<string>('');

  const { data: activityTypes = [], isLoading } = useQuery({
    queryKey: ['activity-types'],
    queryFn: getActivityTypes
  });

  const isEditing = editingId !== null;
  const isSettingAttributes = attributesId !== null;

  // Sync form with selected item
  useEffect(() => {
    if (editingId) {
      const item = activityTypes.find(t => t.id === editingId);
      if (item) {
        setFormName(item.name);
        setFormUnitType(item.unitType);
      }
    }
  }, [editingId, activityTypes]);

  useEffect(() => {
    if (attributesId) {
      const item = activityTypes.find(t => t.id === attributesId);
      if (item?.attributes) {
        setFormSets(item.attributes.sets?.toString() || '');
        setFormReps(item.attributes.reps?.toString() || '');
        setFormWeight(item.attributes.weight?.toString() || '');
      } else {
        setFormSets('');
        setFormReps('');
        setFormWeight('');
      }
    }
  }, [attributesId, activityTypes]);

  function openCreateDialog() {
    setEditingId(null);
    setFormName('');
    setFormUnitType('Count');
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

    try {
      await createActivityType({
        name: formName.trim(),
        unitType: formUnitType
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

    try {
      await updateActivityTypeAttributes(attributesId, {
        sets: formSets ? parseInt(formSets) : null,
        reps: formReps ? parseInt(formReps) : null,
        weight: formWeight ? parseInt(formWeight) : null
      });

      toast.success('Атрибуты обновлены');
      setShowAttributesDialog(false);
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось обновить атрибуты');
    }
  }

  async function handleDelete(id: string, name: string) {
    if (!confirm(`Удалить тип "${name}"?`)) return;

    try {
      await deactivateActivityType(id);
      toast.success('Тип деактивирован');
      queryClient.invalidateQueries({ queryKey: ['activity-types'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось деактивировать тип');
    }
  }

  function clearField(setter: (v: string) => void) {
    setter('');
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
                {activityTypes.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.name}</TableCell>
                    <TableCell>
                      <Badge variant="secondary">
                        {item.unitType === 'Count' ? 'Штуки' : 'Минуты'}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {item.attributes ? (
                        <div className="text-xs space-y-1">
                          {item.attributes.sets && <div>Подходы: {item.attributes.sets}</div>}
                          {item.attributes.reps && <div>Повторения: {item.attributes.reps}</div>}
                          {item.attributes.weight && <div>Вес: {item.attributes.weight} кг</div>}
                          {!item.attributes.sets && !item.attributes.reps && !item.attributes.weight && (
                            <span className="text-muted-foreground">Не заданы</span>
                          )}
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
                          onClick={() => handleDelete(item.id, item.name)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
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
              items={[
                { value: 'Count', label: 'Штуки (повторения, подходы)' },
                { value: 'Time', label: 'Минуты (время)' }
              ]}
              value={formUnitType}
              onValueChange={(v) => setFormUnitType(v as 'Count' | 'Time')}
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
        description={isSettingAttributes ? 'Настройте подходы, повторения и вес' : ''}
      >
        <div className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Укажите значения для этой активности. Оставьте пустыми, если не применяется.
          </p>

          <div className="grid grid-cols-3 gap-4">
            <div className="relative">
              <label className="text-sm font-medium leading-none">Подходы</label>
              <div className="flex gap-2 mt-1">
                <Input
                  type="number"
                  placeholder="3"
                  value={formSets}
                  onChange={(e) => setFormSets(e.target.value)}
                  min="0"
                />
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => clearField(setFormSets)}
                  className="shrink-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>

            <div className="relative">
              <label className="text-sm font-medium leading-none">Повторения</label>
              <div className="flex gap-2 mt-1">
                <Input
                  type="number"
                  placeholder="15"
                  value={formReps}
                  onChange={(e) => setFormReps(e.target.value)}
                  min="0"
                />
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => clearField(setFormReps)}
                  className="shrink-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>

            <div className="relative">
              <label className="text-sm font-medium leading-none">
                Вес (кг)
              </label>
              <div className="flex gap-2 mt-1">
                <Input
                  type="number"
                  placeholder="10"
                  value={formWeight}
                  onChange={(e) => setFormWeight(e.target.value)}
                  min="0"
                />
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => clearField(setFormWeight)}
                  className="shrink-0"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowAttributesDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleSaveAttributes}>
              <Check className="mr-2 h-4 w-4" />
              Сохранить
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
