import { useState, useCallback } from 'react';
import { format } from 'date-fns';
import { ru } from 'date-fns/locale';
import { Dumbbell, CheckCircle2, Bell } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { EmptyState } from '@/components/ui/EmptyState';
import { Pagination } from '@/components/ui/Pagination';
import { useAuth } from '@/contexts/AuthContext';
import { useActivityTypes, useActivityLogs, useLogActivity, useLogActivityWithNotification } from '@/hooks/useQueries';
import { toast } from '@/components/ui/Toaster';

export function ActivityPage() {
  const { user } = useAuth();
  const [value, setValue] = useState('');

  const setSearch = useCallback((updates: Record<string, string | undefined>) => {
    const params = new URLSearchParams(window.location.search);
    for (const [key, val] of Object.entries(updates)) {
      if (val === undefined || val === '') {
        params.delete(key);
      } else {
        params.set(key, val);
      }
    }
    const query = params.toString();
    window.history.replaceState(
      {},
      '',
      `${window.location.pathname}${query ? `?${query}` : ''}`
    );
  }, []);

  const selectedType = new URLSearchParams(window.location.search).get('type') ?? '';
  const page = parseInt(new URLSearchParams(window.location.search).get('page') ?? '1', 10) || 1;

  const { data: activityTypes } = useActivityTypes();
  const { data: logs, isLoading: logsLoading, refetch } = useActivityLogs(user?.id ?? '', page, 10);
  const logMutation = useLogActivity();
  const logWithNotificationMutation = useLogActivityWithNotification();

  const selectedTypeData = activityTypes?.find((t) => t.id === selectedType);

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
          setSearch({ type: undefined, page: '1' });
          setValue('');
          refetch();
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
          setSearch({ type: undefined, page: '1' });
          setValue('');
          refetch();
        },
        onError: (error: Error) => {
          toast.error('Ошибка', error.message);
        }
      }
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Активность</h1>
        <p className="text-muted-foreground mt-1">Записывайте тренировки и отслеживайте прогресс</p>
      </div>

      {/* Log Activity Form */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Dumbbell className="h-5 w-5 text-primary" />
            Записать активность
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="text-sm font-medium leading-none">Тип активности</label>
              <Select
                placeholder="Выберите тип..."
                items={activityTypes?.map((t) => ({
                  value: t.id,
                  label: `${t.name} (${t.unitType === 'Count' ? 'шт.' : 'мин.'})`
                }))}
                value={selectedType}
                onValueChange={(v) => setSearch({ type: v, page: '1' })}
              />
            </div>

            <div>
              <label className="text-sm font-medium leading-none">Значение</label>
              <Input
                type="number"
                placeholder={selectedTypeData?.unitType === 'Count' ? 'Количество (шт.)' : 'Минуты'}
                value={value}
                onChange={(e) => setValue(e.target.value)}
                min="0.1"
                step="0.1"
                suffix={selectedTypeData ? <span className="text-muted-foreground">{selectedTypeData.unitType === 'Count' ? 'шт.' : 'мин.'}</span> : undefined}
              />
            </div>
          </div>

          <div className="flex flex-col sm:flex-row gap-3">
            <Button
              onClick={handleLog}
              isLoading={logMutation.isPending}
              className="flex-1"
            >
              Записать
            </Button>
            <Button
              onClick={handleLogWithNotification}
              variant="outline"
              isLoading={logWithNotificationMutation.isPending}
              className="flex-1"
            >
              <Bell className="mr-2 h-4 w-4" />
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
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center justify-between p-3 rounded-lg border">
                  <div className="flex items-center gap-3">
                    <div className="h-10 w-10 rounded-full bg-muted animate-pulse" />
                    <div className="space-y-2">
                      <div className="h-4 w-32 bg-muted rounded animate-pulse" />
                      <div className="h-3 w-24 bg-muted rounded animate-pulse" />
                    </div>
                  </div>
                  <div className="h-4 w-16 bg-muted rounded animate-pulse" />
                </div>
              ))}
            </div>
          ) : logs?.items.length === 0 ? (
            <EmptyState
              title="Нет записей"
              description="Запишите свою первую тренировку, чтобы увидеть историю здесь"
              action={
                <Button onClick={() => { setSearch({ type: undefined }); window.scrollTo({ top: 0, behavior: 'smooth' }); }}>
                  Записать активность
                </Button>
              }
            />
          ) : logs ? (
            <div className="space-y-2">
              {logs.items.map((log) => (
                <div
                  key={log.id}
                  className="flex items-center justify-between p-3 rounded-lg border hover:bg-muted/30 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
                      <CheckCircle2 className="h-5 w-5 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium text-sm">{log.activityType?.name ?? 'Неизвестно'}</p>
                      <p className="text-xs text-muted-foreground">
                        {format(new Date(log.timestamp), "d MMM yyyy, HH:mm", { locale: ru })}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant="secondary">
                      {log.activityType?.unitType === 'Count' ? 'шт.' : 'мин.'}
                    </Badge>
                    <span className="font-semibold">{log.value}</span>
                  </div>
                </div>
              ))}
            </div>
          ) : null}

          {/* Pagination */}
          {logs && logs.totalPages > 1 && (
            <Pagination
              currentPage={page}
              totalPages={logs.totalPages}
              onPageChange={(p) => setSearch({ page: String(p) })}
              className="mt-4"
            />
          )}
        </CardContent>
      </Card>
    </div>
  );
}
