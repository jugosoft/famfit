import { ReactNode, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  Dumbbell,
  LayoutDashboard,
  PlusCircle,
  Settings,
  User,
  Menu,
  X,
  ChevronRight,
  Calendar
} from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { cn } from '@/lib/utils';
import { Avatar } from '@/components/ui/Avatar';

interface NavItem {
  to: string;
  label: string;
  icon: React.ElementType;
  adminOnly?: boolean;
}

const navItems: NavItem[] = [
  { to: '/dashboard', label: 'Дашборд', icon: LayoutDashboard },
  { to: '/workout', label: 'Тренировки', icon: Calendar },
  { to: '/templates', label: 'Шаблоны', icon: Dumbbell },
  { to: '/activity', label: 'Активность', icon: PlusCircle },
  { to: '/profile', label: 'Профиль', icon: User },
  { to: '/admin', label: 'Админ-панель', icon: Settings, adminOnly: true },
  { to: '/settings', label: 'Настройки', icon: Settings }
];

export function Layout({ children }: { children: ReactNode }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const location = useLocation();
  const { user, isAuthenticated } = useAuth();

  const filteredNavItems = navItems.filter(
    (item) => !item.adminOnly || user?.role === 'Admin'
  );

  return (
    <div className="min-h-screen bg-background">
      {/* Mobile overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-40 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed top-0 left-0 z-50 h-full w-64 bg-card border-r border-border transition-transform duration-200 ease-in-out',
          sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        )}
      >
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="flex items-center gap-3 px-5 py-4 border-b border-border">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/10">
              <Dumbbell className="h-5 w-5 text-primary" />
            </div>
            <span className="text-lg font-bold">FamFit</span>
            <button
              className="ml-auto lg:hidden p-1 rounded hover:bg-accent"
              onClick={() => setSidebarOpen(false)}
            >
              <X className="h-5 w-5" />
            </button>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
            {filteredNavItems.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.to;
              return (
                <Link
                  key={item.to}
                  to={item.to}
                  onClick={() => setSidebarOpen(false)}
                  className={cn(
                    'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all',
                    isActive
                      ? 'bg-primary text-primary-foreground shadow-sm'
                      : 'text-muted-foreground hover:bg-accent hover:text-foreground'
                  )}
                >
                  <Icon className="h-5 w-5 shrink-0" />
                  <span className="truncate">{item.label}</span>
                  {isActive && (
                    <ChevronRight className="ml-auto h-4 w-4 opacity-50" />
                  )}
                </Link>
              );
            })}
          </nav>

          {/* User info */}
          <div className="px-3 py-3 border-t border-border">
            <div className="flex items-center gap-3 px-2 py-1.5 rounded-lg hover:bg-accent transition-colors">
              <Avatar
                fallback={user?.name?.[0]?.toUpperCase() ?? '?'}
                size="sm"
              />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{user?.name}</p>
                <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
              </div>
            </div>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <div className="lg:ml-64 min-h-screen flex flex-col">
        {/* Top bar */}
        <header className="sticky top-0 z-30 flex items-center gap-4 px-4 py-3 bg-background/80 backdrop-blur-md border-b border-border lg:px-6">
          <button
            className="lg:hidden p-2 rounded-lg hover:bg-accent transition-colors"
            onClick={() => setSidebarOpen(true)}
          >
            <Menu className="h-5 w-5" />
          </button>
          <div className="flex-1">
            <h1 className="text-lg font-semibold truncate">
              {filteredNavItems.find((item) => item.to === location.pathname)
                ?.label ?? 'FamFit'}
            </h1>
          </div>
          {/* Mobile user avatar */}
          <div className="lg:hidden">
            <Avatar
              fallback={user?.name?.[0]?.toUpperCase() ?? '?'}
              size="sm"
            />
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 p-4 lg:p-6 max-w-7xl w-full mx-auto">
          {isAuthenticated ? children : null}
        </main>
      </div>
    </div>
  );
}
