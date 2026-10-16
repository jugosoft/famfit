import { useState } from 'react';
import { LogOut, Bell, Moon, Sun, Shield, Trash2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Alert } from '@/components/ui/Alert';
import { ConfirmDialog } from '@/components/ui/ConfirmDialog';
import { Dialog } from '@/components/ui/Dialog';
import { Badge } from '@/components/ui/Badge';
import { Separator } from '@/components/ui/Separator';
import { useAuth as useAuthContext } from '@/contexts/AuthContext';
import { useAuth } from '@/hooks/useAuth';
import { toast } from '@/components/ui/Toaster';

export function SettingsPage() {
  const { user } = useAuthContext();
  const { logout } = useAuth();
  const [showLogoutDialog, setShowLogoutDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [notifications, setNotifications] = useState(true);
  const [darkMode, setDarkMode] = useState(false);

  function handleLogout() {
    logout();
    toast.success('Вы вышли из системы');
    window.location.href = '/login';
  }

  function handleDeleteAccount() {
    toast.error('Функция в разработке');
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Настройки</h1>
        <p className="text-muted-foreground mt-1">Управление приложением и аккаунтом</p>
      </div>

      {/* Appearance */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Sun className="h-5 w-5" />
            Внешний вид
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Тёмная тема</p>
              <p className="text-sm text-muted-foreground">Переключить на тёмное оформление</p>
            </div>
            <Button
              variant={darkMode ? 'default' : 'outline'}
              size="sm"
              onClick={() => setDarkMode(!darkMode)}
            >
              {darkMode ? <Moon className="h-4 w-4" /> : <Sun className="h-4 w-4" />}
              {darkMode ? 'Тёмная' : 'Светлая'}
            </Button>
          </div>

          <Separator />

          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Язык</p>
              <p className="text-sm text-muted-foreground">Выберите язык интерфейса</p>
            </div>
            <Badge variant="secondary">
              Русский
            </Badge>
          </div>
        </CardContent>
      </Card>

      {/* Notifications */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Bell className="h-5 w-5" />
            Уведомления
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Push-уведомления</p>
              <p className="text-sm text-muted-foreground">Получать уведомления о тренировках</p>
            </div>
            <Button
              variant={notifications ? 'default' : 'outline'}
              size="sm"
              onClick={() => setNotifications(!notifications)}
            >
              {notifications ? 'Вкл' : 'Выкл'}
            </Button>
          </div>

          <Separator />

          <Alert
            variant="info"
            title="Интерактивные уведомления"
            description="При записи активности вы получаете push с кнопками: «Сделано, отъебись» и «Напомнить позже»"
          />
        </CardContent>
      </Card>

      {/* Account */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Аккаунт
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Роль</p>
              <p className="text-sm text-muted-foreground">
                {user?.role === 'Admin' ? 'FamilyAdmin — полный доступ' : 'FamilyMember — запись активности'}
              </p>
            </div>
            <Badge variant={user?.role === 'Admin' ? 'default' : 'secondary'}>
              {user?.role ?? ''}
            </Badge>
          </div>

          <Separator />

          <div className="flex flex-col sm:flex-row gap-3">
            <Button
              variant="outline"
              onClick={() => setShowLogoutDialog(true)}
              className="flex-1"
            >
              <LogOut className="mr-2 h-4 w-4" />
              Выйти
            </Button>
            <Button
              variant="destructive"
              onClick={() => setShowDeleteDialog(true)}
              className="flex-1"
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Удалить аккаунт
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Logout Dialog */}
      <Dialog
        open={showLogoutDialog}
        onOpenChange={setShowLogoutDialog}
        title="Выйти из аккаунта"
        description="Вы уверены, что хотите выйти?"
      >
        <div className="flex gap-3 justify-end">
          <Button variant="outline" onClick={() => setShowLogoutDialog(false)}>
            Отмена
          </Button>
          <Button onClick={handleLogout}>
            Выйти
          </Button>
        </div>
      </Dialog>

      {/* Delete Account Dialog */}
      <ConfirmDialog
        open={showDeleteDialog}
        onOpenChange={setShowDeleteDialog}
        title="Удалить аккаунт"
        description="Это действие необратимо. Все ваши данные будут удалены."
        confirmText="Удалить"
        cancelText="Отмена"
        variant="destructive"
        onConfirm={handleDeleteAccount}
      />
    </div>
  );
}
