import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { format } from 'date-fns';
import { ru } from 'date-fns/locale';
import { Loader2, CheckCircle2, XCircle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Label } from '@/components/ui/Label';
import { useAuth } from '@/contexts/AuthContext';
import { useActivityTypes, useLogActivity, useLogActivityWithNotification } from '@/hooks/useQueries';
import { useActivityLogs } from '@/hooks/useQueries';
import { toast } from '@/components/ui/Toaster';

export function ActivityPage() {
  const { user } = useAuth();
  const [selectedType, setSelectedType] = useState('');
  const [value, setValue] = useState('');
  const [page, setPage] = useState(1);

  const { data: activityTypes } = useActivityTypes();
  const { data: logs, isLoading: logsLoading } = useActivityLogs(user?.id ?? '', page);
  const logMutation = useLogActivity();
  const logWithNotificationMutation = useLogActivityWithNotification();

  function handleLog() {
    if (!selectedType || !value) {
      toast.error('Ошибка', 'Заполните все поля');
      return;
    }

    logMutation.mutate(
      {
        activityTypeId: selectedType,
        value: parseFloat(value),
        timestamp: new Date().toISOString()
      },
      {
        onSuccess: () => {
          toast.success('Активность записана!');
          setSelectedType('');
          setValue('');
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  function handleLogWithNotification() {
    if (!selectedType || !value) {
      toast.error('Ошибка', 'Заполните все поля');
      return;
    }

    logWithNotificationMutation.mutate(
      {
        activityTypeId: selectedType,
        value: parseFloat(value),
        timestamp: new Date().toISOString()
      },
      {
        onSuccess: () => {
          toast.success('Активность записана + push отправлен!');
          setSelectedType('');
          setValue('');
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  return (
    <div className="space-y-6">
      {/* Log Activity Form */}
      <Card>
        <CardHeader>
          <CardTitle>Записать активность</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="activityType">Тип активности</Label>
            <select
              id="activityType"
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              value={selectedType}
              onChange={(e) => setSelectedType(e.target.value)}
              disabled={!activityTypes}
            >
              <option value="">Выберите тип...</option>
              {activityTypes?.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name} ({type.unitType === 'Count' ? 'шт.' : 'мин.'})
                </option>
              ))}
            </select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="value">Значение</Label>
            <Input
              id="value"
              type="number"
              placeholder={activityTypes?.find((t) => t.id === selectedType)?.unitType === 'Count' ? 'Количество' : 'Минуты'}
              value={value}
              onChange={(e) => setValue(e.target.value)}
              min="0.1"
              step="0.1"
            />
          </div>

          <div className="flex gap-3">
            <Button
              onClick={handleLog}
              disabled={logMutation.isPending}
              className="flex-1"
            >
              {logMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Записать
            </Button>
            <Button
              onClick={handleLogWithNotification}
              variant="outline"
              disabled={logWithNotificationMutation.isPending}
            >
              {logWithNotificationMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              С push-уведомлением
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Activity History */}
      <Card>
        <CardHeader>
          <CardTitle>История активности</CardTitle>
        </CardHeader>
        <CardContent>
          {logsLoading ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          ) : logs?.items.length === 0 ? (
            <div className="text-sm text-muted-foreground text-center py-8">
              Пока нет записей
            </div>
          ) : (
            <div className="space-y-3">
              {logs?.items.map((log) => (
                <div
                  key={log.id}
                  className="flex items-center justify-between p-3 rounded-lg border bg-card"
                >
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-primary" />
                    <div>
                      <p className="font-medium">
                        {log.activityType?.name ?? 'Неизвестно'}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {format(new Date(log.timestamp), "d MMM yyyy, HH:mm", { locale: ru })}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="font-semibold">
                      {log.value} {log.activityType?.unitType === 'Count' ? 'шт.' : 'мин.'}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Pagination */}
          {logs && logs.totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={page === 1}
                onClick={() => setPage((p) => p - 1)}
              >
                Назад
              </Button>
              <span className="text-sm text-muted-foreground">
                Страница {page} из {logs.totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page === logs.totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Вперёд
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
