import { useState, useEffect } from 'react';
import { Plus, Dumbbell, X, Check } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Dialog } from '@/components/ui/Dialog';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { EmptyState } from '@/components/ui/EmptyState';
import { toast } from '@/components/ui/Toaster';
import {
  createActivityUnit,
  updateActivityUnit,
  deactivateActivityUnit,
} from '@/api/client';
import type { ActivityUnitDto } from '@/api/types';

interface ActivityUnitsTabProps {
  activityUnits: ActivityUnitDto[];
  pendingCreateTab: string | null;
  onCreate: () => void;
}

export function ActivityUnitsTab({
  activityUnits,
  pendingCreateTab,
  onCreate,
}: ActivityUnitsTabProps) {
  const queryClient = useQueryClient();

  const [showUnitDialog, setShowUnitDialog] = useState(false);
  const [showUnitDeleteDialog, setShowUnitDeleteDialog] = useState(false);
  const [unitDeleteId, setUnitDeleteId] = useState<string | null>(null);
  const [unitDeleteName, setUnitDeleteName] = useState('');
  const [isUnitDeleting, setIsUnitDeleting] = useState(false);
  const [unitEditingId, setUnitEditingId] = useState<string | null>(null);
  const [unitFormName, setUnitFormName] = useState('');
  const [unitFormSymbol, setUnitFormSymbol] = useState('');
  const [unitFormDataType, setUnitFormDataType] = useState<'Integer' | 'Decimal' | 'String'>('Integer');
  const [isUnitSaving, setIsUnitSaving] = useState(false);

  const isEditing = unitEditingId !== null;

  // Open create dialog when pendingCreateTab matches
  useEffect(() => {
    if (pendingCreateTab === 'activity-units') {
      openCreateUnitDialog();
      onCreate();
    }
  }, [pendingCreateTab]);

  function openCreateUnitDialog() {
    setUnitEditingId(null);
    setUnitFormName('');
    setUnitFormSymbol('');
    setUnitFormDataType('Integer');
    setShowUnitDialog(true);
  }

  function openEditUnitDialog(unit: ActivityUnitDto) {
    setUnitEditingId(unit.id);
    setUnitFormName(unit.name);
    setUnitFormSymbol(unit.symbol);
    setUnitFormDataType(unit.dataType);
    setShowUnitDialog(true);
  }

  async function handleSaveUnit() {
    if (!unitFormName.trim()) {
      toast.error('Ошибка', 'Введите название единицы измерения');
      return;
    }
    if (!unitFormSymbol.trim()) {
      toast.error('Ошибка', 'Введите символ единицы измерения');
      return;
    }

    try {
      setIsUnitSaving(true);
      if (unitEditingId) {
        await updateActivityUnit(unitEditingId, {
          name: unitFormName.trim(),
          symbol: unitFormSymbol.trim(),
          dataType: unitFormDataType,
        });
        toast.success('Единица обновлена');
      } else {
        await createActivityUnit({
          name: unitFormName.trim(),
          symbol: unitFormSymbol.trim(),
          dataType: unitFormDataType,
          displayOrder: 0,
        });
        toast.success('Единица создана');
      }
      setShowUnitDialog(false);
      queryClient.invalidateQueries({ queryKey: ['activity-units'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось сохранить единицу измерения');
    } finally {
      setIsUnitSaving(false);
    }
  }

  function openUnitDeleteDialog(id: string, name: string) {
    setUnitDeleteId(id);
    setUnitDeleteName(name);
    setShowUnitDeleteDialog(true);
  }

  async function confirmUnitDelete() {
    if (!unitDeleteId) return;
    setIsUnitDeleting(true);
    try {
      await deactivateActivityUnit(unitDeleteId);
      toast.success('Единица деактивирована');
      queryClient.invalidateQueries({ queryKey: ['activity-units'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось деактивировать единицу');
    } finally {
      setIsUnitDeleting(false);
      setShowUnitDeleteDialog(false);
    }
  }

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Единицы измерения</CardTitle>
        </CardHeader>
        <CardContent>
          {activityUnits.length === 0 ? (
            <EmptyState
              title="Нет единиц измерения"
              description="Добавьте первую единицу измерения для начала работы"
              action={
                <Button onClick={openCreateUnitDialog}>
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
                  <TableHead>Символ</TableHead>
                  <TableHead>Тип данных</TableHead>
                  <TableHead>Статус</TableHead>
                  <TableHead className="text-right">Действия</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {activityUnits.map((unit) => (
                  <TableRow key={unit.id}>
                    <TableCell className="font-medium">{unit.name}</TableCell>
                    <TableCell>
                      <Badge variant="secondary">{unit.symbol}</Badge>
                    </TableCell>
                    <TableCell>{unit.dataType}</TableCell>
                    <TableCell>
                      <Badge variant={unit.isActive ? 'default' : 'secondary'}>
                        {unit.isActive ? 'Активна' : 'Неактивна'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openEditUnitDialog(unit)}
                          title="Редактировать"
                        >
                          <Dumbbell className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openUnitDeleteDialog(unit.id, unit.name)}
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

      {/* Unit Create/Edit Dialog */}
      <Dialog
        open={showUnitDialog}
        onOpenChange={setShowUnitDialog}
        title={isEditing ? 'Редактировать единицу' : 'Новая единица измерения'}
        description={isEditing ? 'Измените параметры единицы измерения' : 'Добавьте новую единицу измерения для типов активностей'}
      >
        <div className="space-y-4">
          <Input
            label="Название"
            placeholder="Например: Количество повторений"
            value={unitFormName}
            onChange={(e) => setUnitFormName(e.target.value)}
            maxLength={100}
          />

          <Input
            label="Символ"
            placeholder="Например: раз"
            value={unitFormSymbol}
            onChange={(e) => setUnitFormSymbol(e.target.value)}
            maxLength={20}
          />

          <div>
            <label className="text-sm font-medium leading-none">Тип данных</label>
            <Select
              items={[
                { value: 'Integer', label: 'Целое число' },
                { value: 'Decimal', label: 'Дробное число' },
                { value: 'String', label: 'Строка' },
              ]}
              value={unitFormDataType}
              onValueChange={(v) => setUnitFormDataType(v as 'Integer' | 'Decimal' | 'String')}
            />
          </div>

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowUnitDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleSaveUnit} isLoading={isUnitSaving}>
              <Check className="mr-2 h-4 w-4" />
              {isEditing ? 'Сохранить' : 'Создать'}
            </Button>
          </div>
        </div>
      </Dialog>

      {/* Unit Delete Confirmation Dialog */}
      <ConfirmDialog
        open={showUnitDeleteDialog}
        onOpenChange={setShowUnitDeleteDialog}
        title="Деактивировать единицу измерения"
        description={`Вы уверены, что хотите деактивировать "${unitDeleteName}"? Это действие можно отменить.`}
        confirmText="Деактивировать"
        cancelText="Отмена"
        variant="destructive"
        onConfirm={confirmUnitDelete}
        isLoading={isUnitDeleting}
      />
    </>
  );
}
