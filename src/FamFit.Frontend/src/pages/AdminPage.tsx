import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Plus, Edit2, Trash2, X, Check } from 'lucide-react';
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

// Types (normally from API)
interface ActivityTypeItem {
  id: string;
  name: string;
  unitType: 'Count' | 'Time';
  isActive: boolean;
}

export function AdminPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [showDialog, setShowDialog] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formName, setFormName] = useState('');
  const [formUnitType, setFormUnitType] = useState<'Count' | 'Time'>('Count');

  // Mock data — replace with API call
  const { data: activityTypes = [], isLoading } = useQuery({
    queryKey: ['admin-activity-types'],
    queryFn: async () => {
      // In production: return await getActivityTypesAdmin();
      return [] as ActivityTypeItem[];
    }
  });

  const isEditing = editingId !== null;

  function openCreateDialog() {
    setEditingId(null);
    setFormName('');
    setFormUnitType('Count');
    setShowDialog(true);
  }

  function openEditDialog(item: ActivityTypeItem) {
    setEditingId(item.id);
    setFormName(item.name);
    setFormUnitType(item.unitType);
    setShowDialog(true);
  }

  function handleSave() {
    if (!formName.trim()) {
      toast.error('Ошибка', 'Введите название типа активности');
      return;
    }

    // In production: call API
    toast.success(isEditing ? 'Тип обновлён' : 'Тип создан');
    setShowDialog(false);
    queryClient.invalidateQueries({ queryKey: ['admin-activity-types'] });
  }

  function handleDelete() {
    if (!confirm('Удалить этот тип активности?')) return;
    // In production: call API
    toast.success('Тип удалён');
    queryClient.invalidateQueries({ queryKey: ['admin-activity-types'] });
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
                      <Badge variant={item.isActive ? 'default' : 'secondary'}>
                        {item.isActive ? 'Активен' : 'Неактивен'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button variant="ghost" size="icon" onClick={() => openEditDialog(item)}>
                          <Edit2 className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" onClick={handleDelete}>
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

      {/* Create/Edit Dialog */}
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
    </div>
  );
}
