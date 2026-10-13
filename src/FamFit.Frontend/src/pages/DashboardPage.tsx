import { Dumbbell, TrendingUp, Calendar, Award, ArrowRight } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { StatCard } from '@/components/ui/StatCard';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { useAuth } from '@/contexts/AuthContext';
import { useActivityTypes } from '@/hooks/useQueries';
import { useActivityLogs } from '@/hooks/useQueries';
import { format } from 'date-fns';
import { ru } from 'date-fns/locale';

export function DashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { data: activityTypes } = useActivityTypes();
  const { data: logs, isLoading: logsLoading } = useActivityLogs(user?.id ?? '', 1, 5);

  const stats = [
    {
      title: 'Всего тренировок',
      value: '24',
      description: 'за последний месяц',
      icon: <Dumbbell className="h-5 w-5" />,
      trend: { value: 12, positive: true }
    },
    {
      title: 'Серия дней',
      value: user?.streakCount ?? 0,
      description: 'дней подряд',
      icon: <TrendingUp className="h-5 w-5" />,
      trend: { value: 5, positive: true }
    },
    {
      title: 'Активных типов',
      value: activityTypes?.length ?? 0,
      description: 'доступно в справочнике',
      icon: <Calendar className="h-5 w-5" />
    },
    {
      title: 'Рейтинг',
      value: '#3',
      description: 'в семье',
      icon: <Award className="h-5 w-5" />,
      trend: { value: 2, positive: false }
    }
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Добро пожаловать, {user?.name}! 👋</h1>
          <p className="text-muted-foreground mt-1">
            {format(new Date(), 'EEEE, d MMMM', { locale: ru })}
          </p>
        </div>
        <Button onClick={() => navigate('/activity')}>
          Записать активность
          <ArrowRight className="ml-2 h-4 w-4" />
        </Button>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <StatCard key={stat.title} {...stat} />
        ))}
      </div>

      {/* Quick Actions */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card
          className="cursor-pointer hover:border-primary/50 transition-colors"
          onClick={() => navigate('/activity')}
        >
          <CardHeader>
            <CardTitle className="text-base">Записать тренировку</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">
              Добавьте новую запись об активности
            </p>
          </CardContent>
        </Card>

        <Card
          className="cursor-pointer hover:border-primary/50 transition-colors"
          onClick={() => navigate('/activity')}
        >
          <CardHeader>
            <CardTitle className="text-base">Посмотреть историю</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">
              Просмотр всех записей об активности
            </p>
          </CardContent>
        </Card>

        {user?.role === 'Admin' && (
          <Card
            className="cursor-pointer hover:border-primary/50 transition-colors"
            onClick={() => navigate('/admin')}
          >
            <CardHeader>
              <CardTitle className="text-base">Админ-панель</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">
                Управление типами активностей
              </p>
            </CardContent>
          </Card>
        )}
      </div>

      {/* Recent Activity */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Последняя активность</CardTitle>
          <Button variant="ghost" size="sm" onClick={() => navigate('/activity')}>
            Все записи
            <ArrowRight className="ml-2 h-4 w-4" />
          </Button>
        </CardHeader>
        <CardContent>
          {logsLoading ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <div key={i} className="flex items-center gap-4">
                  <Skeleton className="h-10 w-10 rounded-full" />
                  <div className="space-y-2 flex-1">
                    <Skeleton className="h-4 w-1/3" />
                    <Skeleton className="h-3 w-1/4" />
                  </div>
                </div>
              ))}
            </div>
          ) : logs?.items.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-muted-foreground">Пока нет записей</p>
              <Button variant="link" onClick={() => navigate('/activity')}>
                Записать первую тренировку
              </Button>
            </div>
          ) : logs ? (
            <div className="space-y-3">
              {logs.items.map((log) => (
                <div key={log.id} className="flex items-center justify-between py-2 border-b last:border-0">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
                      <Dumbbell className="h-5 w-5 text-primary" />
                    </div>
                    <div>
                      <p className="font-medium text-sm">
                        {log.activityType?.name ?? 'Неизвестно'}
                      </p>
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
        </CardContent>
      </Card>
    </div>
  );
}
