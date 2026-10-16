import { useState } from 'react';
import { Dumbbell, X, Check } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/Table';
import { Dialog } from '@/components/ui/Dialog';
import { EmptyState } from '@/components/ui/EmptyState';
import { toast } from '@/components/ui/Toaster';
import {
  updateUser,
  toggleBanUser,
  resetUserPassword,
} from '@/api/client';
import type { UserDto } from '@/api/types';

interface UsersTabProps {
  users: UserDto[];
}

export function UsersTab({ users }: UsersTabProps) {
  const queryClient = useQueryClient();

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

  function openUserEditDialog(user: UserDto) {
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

  async function handleToggleBan(user: UserDto) {
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

  return (
    <>
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
    </>
  );
}
