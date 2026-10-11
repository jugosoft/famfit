import { useQuery } from '@tanstack/react-query';
import { Dumbbell, TrendingUp, Calendar, Award, PlusCircle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/contexts/AuthContext';
import { useActivityTypes } from '@/hooks/useQueries';
import { useNavigate } from 'react-router-dom';

export function DashboardPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { data: activityTypes } = useActivityTypes();

  // Mock data for demo (replace with real API calls)
  const stats = [
    {
      title: 'Всего тренировок',
      value: '24',
      icon: Dumbbell,
      description: 'за последний месяц'
    },
    {
      title: 'Серия дней',
      value: `${user?.streakCount ?? 0}`,
      icon: TrendingUp,
      description: 'дней подряд'
    },
    {
      title: 'Активных типов',
      value: String(activityTypes?.length ?? 0),
      icon: Calendar,
      description: 'доступно в справочнике'
    },
    {
      title: 'Рейтинг',
      value: '#3',
      icon: Award,
      description: 'в семье'
    }
  ];

  return (
    <div className="space-y-6">
      {/* Welcome */}
      <div>
        <h2 className="text-2xl font-bold">Добро пожаловать, {user?.name}! 👋</h2>
        <p className="text-muted-foreground">Вот ваша сводка за сегодня</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <Card key={stat.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{stat.title}</CardTitle>
                <Icon className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{stat.value}</div>
                <p className="text-xs text-muted-foreground">{stat.description}</p>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Быстрые действия</CardTitle>
        </CardHeader>
        <CardContent className="flex gap-3">
          <Button onClick={() => navigate('/activity')}>
            <PlusCircle className="mr-2 h-4 w-4" />
            Записать активность
          </Button>
        </CardContent>
      </Card>

      {/* Recent Activity */}
      <Card>
        <CardHeader>
          <CardTitle>Последняя активность</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-sm text-muted-foreground text-center py-8">
            Пока нет записей. Запишите свою первую тренировку!
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
