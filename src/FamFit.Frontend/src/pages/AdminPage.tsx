import { useState, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
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
import { Tabs } from '@/components/ui/Tabs';
import { useAuth } from '@/contexts/AuthContext';
import { toast } from '@/components/ui/Toaster';
import {
  getActivityTypes,
  createActivityType,
  getActivityAttributeDefinitions,
  deactivateActivityType,
  getActivityUnits,
  createActivityUnit,
  updateActivityUnit,
  deactivateActivityUnit,
  getUsers,
  updateUser,
  toggleBanUser,
  resetUserPassword,
} from '@/api/client';
import type { ActivityUnitDto } from '@/api/types';
import { useUpdateActivityTypeAttributes } from '@/hooks/useQueries';

// Types
interface ActivityTypeItem {
  id: string;
  name: string;
  activityUnitId?: string;
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

// Helper to map ActivityTypeDto to ActivityTypeItem
function mapActivityType(dto: any): ActivityTypeItem {
  return {
    id: dto.id,
    name: dto.name,
    activityUnitId: dto.activityUnitId || dto.unitId || '',
    unitSymbol: dto.unitSymbol || null,
    isActive: dto.isActive !== false,
    attributes: (dto.attributes || []).map((attr: any) => ({
      id: attr.id || crypto.randomUUID(),
      definitionName: attr.definitionName,
      displayName: attr.displayName,
      dataType: attr.dataType,
      unit: attr.unit || null,
      value: attr.value
    }))
  };
}

export function AdminPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const updateAttributesMutation = useUpdateActivityTypeAttributes();

  const [activeTab, setActiveTab] = useState('activity-types');

  const { data: activityUnits = [] } = useQuery<ActivityUnitDto[]>({
    queryKey: ['activity-units'],
    queryFn: getActivityUnits,
  });

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

  // Activity Units state
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

  // Users state
  const { data: users = [] } = useQuery({
    queryKey: ['users'],
    queryFn: getUsers,
  });
  const [showUserEditDialog, setShowUserEditDialog] = useState(false);
  const [showUserPasswordDialog, setShowUserPasswordDialog] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [selectedUserName, setSelectedUserName] = useState('');
  const [editName, setEditName] = useState('');
  const [editEmail, setEditEmail] = useState('');
  const [editRole, setEditRole] = useState<'Admin' | 'Member'>('Member');
  const [isUserSaving, setIsUserSaving] = useState(false);
  const [passwordValue, setPasswordValue] = useState('');
  const [isPasswordSaving, setIsPasswordSaving] = useState(false);

  const { data: rawActivityTypes, isLoading } = useQuery({
    queryKey: ['activity-types'],
    queryFn: getActivityTypes
  });

  const activityTypes: ActivityTypeItem[] = (rawActivityTypes || []).map(mapActivityType);

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
        setFormActivityUnitId(item.activityUnitId ?? '');
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

  function getValueForApi(_defName: string, inputValue: string, dataType: string) {
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

  function clearField(defName: string) {
    setAttributeValues(prev => ({ ...prev, [defName]: '' }));
  }

  // ---- Activity Units handlers ----

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

  // ---- User handlers ----

  function openUserEditDialog(user: any) {
    setSelectedUserId(user.id);
    setSelectedUserName(user.name);
    setEditName(user.name);
    setEditEmail(user.email);
    setEditRole(user.role);
    setShowUserEditDialog(true);
  }

  async function handleSaveUser() {
    if (!selectedUserId) return;
    if (!editName.trim()) {
      toast.error('Ошибка', 'Введите имя пользователя');
      return;
    }

    try {
      setIsUserSaving(true);
      await updateUser(selectedUserId, {
        name: editName.trim(),
        email: editEmail.trim() || undefined,
        role: editRole,
      });
      toast.success('Пользователь обновлён');
      setShowUserEditDialog(false);
      queryClient.invalidateQueries({ queryKey: ['users'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось обновить пользователя');
    } finally {
      setIsUserSaving(false);
    }
  }

  async function handleToggleBan(user: any) {
    try {
      await toggleBanUser(user.id);
      toast.success(user.isBanned ? 'Пользователь разблокирован' : 'Пользователь заблокирован');
      queryClient.invalidateQueries({ queryKey: ['users'] });
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось изменить статус');
    }
  }

  async function handleResetPassword() {
    if (!selectedUserId) return;
    if (!passwordValue.trim() || passwordValue.length < 8) {
      toast.error('Ошибка', 'Пароль должен содержать минимум 8 символов');
      return;
    }

    try {
      setIsPasswordSaving(true);
      await resetUserPassword(selectedUserId, passwordValue);
      toast.success('Пароль сброшен');
      setShowUserPasswordDialog(false);
      setPasswordValue('');
    } catch (error: any) {
      toast.error('Ошибка', error.message || 'Не удалось сбросить пароль');
    } finally {
      setIsPasswordSaving(false);
    }
  }

  const tabItems = [
    { value: 'activity-types', label: 'Управление типами активностей' },
    { value: 'activity-units', label: 'Управление единицами измерения' },
    { value: 'users', label: 'Реестр пользователей' },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Админ-панель</h1>
          <p className="text-muted-foreground mt-1">Управление системой</p>
        </div>
        {activeTab === 'activity-types' && (
          <Button onClick={openCreateDialog}>
            <Plus className="mr-2 h-4 w-4" />
            Добавить тип
          </Button>
        )}
        {activeTab === 'activity-units' && (
          <Button onClick={openCreateUnitDialog}>
            <Plus className="mr-2 h-4 w-4" />
            Добавить единицу
          </Button>
        )}
      </div>

      {/* Role Check */}
      {user?.role !== 'Admin' && (
        <Alert
          variant="destructive"
          title="Доступ запрещён"
          description="У вас нет прав для доступа к админ-панели."
        />
      )}

      {/* Tabs */}
      <Tabs items={tabItems} value={activeTab} onValueChange={setActiveTab} />

      {/* Activity Types Tab */}
      {activeTab === 'activity-types' && (
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
      )}

      {/* Activity Units Tab */}
      {activeTab === 'activity-units' && (
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
      )}

      {/* Users Tab */}
      {activeTab === 'users' && (
        <Card>
          <CardHeader>
            <CardTitle>Пользователи</CardTitle>
          </CardHeader>
          <CardContent>
            {users.length === 0 ? (
              <EmptyState
                title="Нет пользователей"
                description="Пользователи системы будут отображаться здесь"
              />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Имя</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead>Роль</TableHead>
                    <TableHead>Серия</TableHead>
                    <TableHead>Статус</TableHead>
                    <TableHead className="text-right">Действия</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell className="font-medium">{user.name}</TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>
                        <Badge variant={user.role === 'Admin' ? 'default' : 'secondary'}>
                          {user.role}
                        </Badge>
                      </TableCell>
                      <TableCell>{user.streakCount}</TableCell>
                      <TableCell>
                        <Badge variant={user.isBanned ? 'destructive' : 'default'}>
                          {user.isBanned ? 'Заблокирован' : 'Активен'}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex items-center justify-end gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => openUserEditDialog(user)}
                            title="Редактировать"
                          >
                            <Dumbbell className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleToggleBan(user)}
                            title={user.isBanned ? 'Разблокировать' : 'Заблокировать'}
                          >
                            <Badge
                              variant={user.isBanned ? 'default' : 'destructive'}
                              className="h-4 w-4 rounded-full p-0 !bg-current"
                            >
                              <span className="sr-only">{user.isBanned ? 'Разблокировать' : 'Заблокировать'}</span>
                            </Badge>
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => {
                              setSelectedUserId(user.id);
                              setSelectedUserName(user.name);
                              setPasswordValue('');
                              setShowUserPasswordDialog(true);
                            }}
                            title="Сбросить пароль"
                          >
                            <X className="h-4 w-4" />
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
      )}

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

      {/* Unit Create/Edit Dialog */}
      <Dialog
        open={showUnitDialog}
        onOpenChange={setShowUnitDialog}
        title={unitEditingId ? 'Редактировать единицу' : 'Новая единица измерения'}
        description={unitEditingId ? 'Измените параметры единицы измерения' : 'Добавьте новую единицу измерения для типов активностей'}
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
              {unitEditingId ? 'Сохранить' : 'Создать'}
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

      {/* User Edit Dialog */}
      <Dialog
        open={showUserEditDialog}
        onOpenChange={setShowUserEditDialog}
        title={`Редактировать: ${selectedUserName}`}
        description="Измените имя, email и роль пользователя"
      >
        <div className="space-y-4">
          <Input
            label="Имя"
            placeholder="Имя пользователя"
            value={editName}
            onChange={(e) => setEditName(e.target.value)}
            maxLength={100}
          />

          <Input
            label="Email"
            type="email"
            placeholder="email@example.com"
            value={editEmail}
            onChange={(e) => setEditEmail(e.target.value)}
            maxLength={255}
          />

          <div>
            <label className="text-sm font-medium leading-none">Роль</label>
            <Select
              items={[
                { value: 'Admin', label: 'Администратор' },
                { value: 'Member', label: 'Участник' },
              ]}
              value={editRole}
              onValueChange={(v) => setEditRole(v as 'Admin' | 'Member')}
            />
          </div>

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowUserEditDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleSaveUser} isLoading={isUserSaving}>
              <Check className="mr-2 h-4 w-4" />
              Сохранить
            </Button>
          </div>
        </div>
      </Dialog>

      {/* User Reset Password Dialog */}
      <Dialog
        open={showUserPasswordDialog}
        onOpenChange={setShowUserPasswordDialog}
        title={`Сброс пароля: ${selectedUserName}`}
        description="Установите новый пароль для этого пользователя (минимум 8 символов)"
      >
        <div className="space-y-4">
          <Input
            label="Новый пароль"
            type="password"
            placeholder="Введите новый пароль"
            value={passwordValue}
            onChange={(e) => setPasswordValue(e.target.value)}
            minLength={8}
          />

          <div className="flex gap-3 justify-end">
            <Button variant="outline" onClick={() => setShowUserPasswordDialog(false)}>
              <X className="mr-2 h-4 w-4" />
              Отмена
            </Button>
            <Button onClick={handleResetPassword} isLoading={isPasswordSaving}>
              <Check className="mr-2 h-4 w-4" />
              Сбросить
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
