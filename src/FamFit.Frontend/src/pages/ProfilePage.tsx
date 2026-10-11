import { useState } from 'react';
import { Dumbbell, User, Mail, Shield, Loader2, Check, X } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Avatar } from '@/components/ui/Avatar';
import { Tabs } from '@/components/ui/Tabs';
import { Progress } from '@/components/ui/Progress';
import { Alert } from '@/components/ui/Alert';
import { useAuth } from '@/contexts/AuthContext';
import { useAuth as useAuthHook } from '@/hooks/useAuth';
import { toast } from '@/components/ui/Toaster';

export function ProfilePage() {
  const { user } = useAuth();
  const { login } = useAuthHook();
  const [editName, setEditName] = useState(user?.name ?? '');
  const [editEmail, setEditEmail] = useState(user?.email ?? '');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const tabs = [
    { value: 'profile', label: 'Профиль' },
    { value: 'security', label: 'Безопасность' },
    { value: 'stats', label: 'Статистика' }
  ];

  function handleSaveProfile() {
    setIsSaving(true);
    setTimeout(() => {
      toast.success('Профиль обновлён');
      setIsSaving(false);
    }, 1000);
  }

  function handleChangePassword() {
    if (newPassword.length < 8) {
      toast.error('Ошибка', 'Пароль должен быть не менее 8 символов');
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error('Ошибка', 'Пароли не совпадают');
      return;
    }
    toast.success('Пароль изменён');
    setCurrentPassword('');
    setNewPassword('');
    setConfirmPassword('');
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Профиль</h1>
        <p className="text-muted-foreground mt-1">Управление настройками аккаунта</p>
      </div>

      {/* Profile Info Card */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row items-start sm:items-center gap-4">
            <Avatar fallback={user?.name?.[0]?.toUpperCase() ?? '?'} size="lg" />
            <div className="flex-1">
              <h2 className="text-xl font-bold">{user?.name}</h2>
              <p className="text-muted-foreground">{user?.email}</p>
              <div className="flex items-center gap-2 mt-2">
                <Badge variant={user?.role === 'Admin' ? 'default' : 'secondary'}>
                  {user?.role === 'Admin' ? '👑 FamilyAdmin' : '👤 FamilyMember'}
                </Badge>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Tabs */}
      <Tabs
        items={tabs}
        value="profile"
        onValueChange={() => {}}
      />

      {/* Profile Tab */}
      <Card>
        <CardHeader>
          <CardTitle>Редактировать профиль</CardTitle>
          <CardDescription>Измените имя и email</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <Input
              label="Имя"
              placeholder="Ваше имя"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              prefix={<User className="h-4 w-4" />}
            />
            <Input
              label="Email"
              type="email"
              placeholder="email@example.com"
              value={editEmail}
              onChange={(e) => setEditEmail(e.target.value)}
              prefix={<Mail className="h-4 w-4" />}
            />
          </div>
          <div className="flex justify-end">
            <Button onClick={handleSaveProfile} isLoading={isSaving}>
              <Check className="mr-2 h-4 w-4" />
              Сохранить изменения
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Security Tab */}
      <Card>
        <CardHeader>
          <CardTitle>Смена пароля</CardTitle>
          <CardDescription>Обновите пароль для безопасности аккаунта</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <Input
            label="Текущий пароль"
            type="password"
            placeholder="Введите текущий пароль"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            prefix={<Shield className="h-4 w-4" />}
          />
          <Input
            label="Новый пароль"
            type="password"
            placeholder="Минимум 8 символов"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            prefix={<Shield className="h-4 w-4" />}
          />
          <Input
            label="Подтвердите пароль"
            type="password"
            placeholder="Повторите новый пароль"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            prefix={<Shield className="h-4 w-4" />}
          />
          <div className="flex justify-end">
            <Button onClick={handleChangePassword}>
              <Check className="mr-2 h-4 w-4" />
              Изменить пароль
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Stats Tab */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Ваша серия</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="text-center">
              <p className="text-5xl font-bold text-primary">{user?.streakCount ?? 0}</p>
              <p className="text-sm text-muted-foreground mt-1">дней подряд</p>
            </div>
            <Progress value={Math.min(100, (user?.streakCount ?? 0) * 10)} max={100} />
            <p className="text-xs text-muted-foreground text-center">
              {user?.streakCount >= 7 ? '🔥 Отличная серия! Продолжайте!' : 'Записывайте активность каждый день для серии'}
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Безопасность</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Alert variant="success" title="Пароль защищён" description="Ваш пароль хранится в зашифрованном виде" />
            <Alert variant="info" title="Двухфакторная аутентификация" description="Скоро будет доступно" />
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
