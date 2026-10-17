import { useState, useEffect } from 'react';
import { Plus, X, Check } from 'lucide-react';
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
  deactivateActivityAttributeDefinition,
  createAttributeDefinition,
  updateAttributeDefinition,
} from '@/api/client';
import type { AttributeDefinitionDto } from '@/api/types';

interface AttributesTabProps {
  attributeDefinitions: AttributeDefinitionDto[];
  pendingCreateTab: string | null;
  onCreate: () => void;
}

const dataTypeLabels: Record<string, { label: string; color: string }> = {
  Integer: { label: 'Целое число', color: 'bg-blue-100 text-blue-800' },
  Decimal: { label: 'Дробное число', color: 'bg-green-100 text-green-800' },
  Boolean: { label: 'Логический', color: 'bg-purple-100 text-purple-800' },
  String: { label: 'Строка', color: 'bg-amber-100 text-amber-800' },
};

function getDataTypeBadge(dataType: string) {
  const config = dataTypeLabels[dataType];
  if (!config) {
    return 'Без расшифровки';
  };
  return config.label;
}

function getDataTypeColor(dataType: string) {
  const config = dataTypeLabels[dataType];
  return config?.color || 'bg-gray-100 text-gray-800';
}

export function AttributesTab({
  attributeDefinitions,
  pendingCreateTab,
  onCreate,
}: AttributesTabProps) {
  const queryClient = useQueryClient();

  const [showDialog, setShowDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [deleteName, setDeleteName] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formName, setFormName] = useState('');
  const [formDisplayName, setFormDisplayName] = useState('');
  const [formDataType, setFormDataType] = useState<'Integer' | 'Decimal' | 'Boolean' | 'String'>('Integer');
  const [formUnit, setFormUnit] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const isEditing = editingId !== null;

  // Open create dialog when pendingCreateTab matches
  useEffect(() => {
    if (pendingCreateTab === 'attributes') {
      openCreateDialog();
      onCreate();
    }
  }, [pendingCreateTab]);

  // Sync form with selected item
  useEffect(() => {
    if (editingId) {
      const item = attributeDefinitions.find(t => t.id === editingId);
      if (item) {
        setFormName(item.name);
        setFormDisplayName(item.displayName);
        setFormDataType(item.dataType);
        setFormUnit(item.unit || '');
      }
    }
  }, [editingId, attributeDefinitions]);

  function openCreateDialog() {
    setEditingId(null);
    setFormName('');
    setFormDisplayName('');
    setFormDataType('Integer');
    setFormUnit('');
    setShowDialog(true);
  }

  function openEditDialog(item: AttributeDefinitionDto) {
    setEditingId(item.id);
    setFormName(item.name);
    setFormDisplayName(item.displayName);
    setFormDataType(item.dataType);
    setFormUnit(item.unit || '');
    setShowDialog(true);
  }

  function openDeleteDialog(id: string, name: string) {
    setDeleteId(id);
    setDeleteName(name);
    setShowDeleteDialog(true);
  }

  async function handleSave() {
    if (!formName.trim()) {
      toast.error('Ошибка', 'Введите название атрибута');
      return;
    }
    if (!formDisplayName.trim()) {
      toast.error('Ошибка', 'Введите отображаемое имя');
      return;
    }

    try {
      setIsSaving(true);
      if (isEditing && editingId) {
        await updateAttributeDefinition(editingId, {
          name: formName.trim(),
          displayName: formDisplayName.trim(),
          unit: formUnit.trim() || undefined,
        });
        toast.success('Атрибут обновлён');
      } else {
        await createAttributeDefinition({
          name: formName.trim(),
          displayName: formDisplayName.trim(),
          dataType: formDataType,
          unit: formUnit.trim() || undefined,
        });
        toast.success('Атрибут создан');
      }
      setShowDialog(false);
      queryClient.invalidateQueries({ queryKey: ['attribute-definitions'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось сохранить атрибут');
    } finally {
      setIsSaving(false);
    }
  }

  async function confirmDelete() {
    if (!deleteId) return;
    setIsDeleting(true);
    try {
      await deactivateActivityAttributeDefinition(deleteId);
      toast.success('Атрибут деактивирован');
      queryClient.invalidateQueries({ queryKey: ['attribute-definitions'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось деактивировать атрибут');
    } finally {
      setIsDeleting(false);
      setShowDeleteDialog(false);
    }
  }

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>Атрибуты</CardTitle>
        </CardHeader>
        <CardContent>
          {attributeDefinitions.length === 0 ? (
            <EmptyState
              title="Нет атрибутов"
              description="Добавьте первый атрибут для начала работы"
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
                  <TableHead>Отображаемое имя</TableHead>
                  <TableHead>Имя</TableHead>
                  <TableHead>Тип данных</TableHead>
                  <TableHead>Символ</TableHead>
                  <TableHead>Статус</TableHead>
                  <TableHead className="text-right">Действия</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {attributeDefinitions.map((def) => (
                  <TableRow
                    key={def.id}
                    onClick={() => openEditDialog(def)}
                    className="cursor-pointer hover:bg-muted/50"
                  >
                    <TableCell className="font-medium">{def.displayName}</TableCell>
                    <TableCell className="text-muted-foreground text-sm">{def.name}</TableCell>
                    <TableCell>
                      <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getDataTypeColor(def.dataType)}`}>
                        {getDataTypeBadge(def.dataType)}
                      </span>
                    </TableCell>
                    <TableCell>
                      {def.unit ? <Badge variant="secondary">{def.unit}</Badge> : '—'}
                    </TableCell>
                    <TableCell>
                      <Badge variant={def.isActive ? 'default' : 'secondary'}>
                        {def.isActive ? 'Активен' : 'Неактивен'}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-right" onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openDeleteDialog(def.id, def.displayName)}
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
        title={isEditing ? 'Редактировать атрибут' : 'Новый атрибут'}
        description={isEditing ? 'Измените параметры атрибута' : 'Добавьте новый атрибут для типов активностей'}
      >
        <div className="space-y-4">
          <Input
            label="Название (ключ)"
            placeholder="Например: reps"
            value={formName}
            onChange={(e) => setFormName(e.target.value)}
            maxLength={100}
          />

          <Input
            label="Отображаемое имя"
            placeholder="Например: Повторения"
            value={formDisplayName}
            onChange={(e) => setFormDisplayName(e.target.value)}
            maxLength={100}
          />

          <div>
            <label className="text-sm font-medium leading-none">Тип данных</label>
            <Select
              items={[
                { value: 'Integer', label: 'Целое число' },
                { value: 'Decimal', label: 'Дробное число' },
                { value: 'Boolean', label: 'Логический' },
                { value: 'String', label: 'Строка' },
              ]}
              value={formDataType}
              onValueChange={(v) => setFormDataType(v as 'Integer' | 'Decimal' | 'Boolean' | 'String')}
            />
          </div>

          <Input
            label="Символ"
            placeholder="Например: раз"
            value={formUnit}
            onChange={(e) => setFormUnit(e.target.value)}
            maxLength={20}
          />

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleSave} isLoading={isSaving}>
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
        title="Деактивировать атрибут"
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
