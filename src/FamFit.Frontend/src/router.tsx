import { createBrowserRouter, Navigate, Outlet, RouterProvider } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from '@/components/ui/Toaster';
import { AuthProvider, useAuth } from '@/contexts/AuthContext';
import { Layout } from '@/components/Layout';
import { LoginPage } from '@/pages/LoginPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { ActivityPage } from '@/pages/ActivityPage';
import { WorkoutPage } from '@/pages/WorkoutPage';
import { WorkoutTemplatePage } from '@/pages/WorkoutTemplatePage';
import { AdminPage } from '@/pages/AdminPage';
import { ProfilePage } from '@/pages/ProfilePage';
import { SettingsPage } from '@/pages/SettingsPage';

// Create React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 5 * 60 * 1000,
      refetchOnWindowFocus: false
    }
  }
});

// Protected route wrapper
function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}

// Layout wrapper for protected routes
function ProtectedLayout() {
  return (
    <Layout>
      <Outlet />
    </Layout>
  );
}

// Router configuration
export const router = createBrowserRouter([
  // Public routes
  {
    path: '/login',
    element: <LoginPage />
  },

  // Protected routes
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <ProtectedLayout />,
        children: [
          {
            path: '/',
            element: <Navigate to="/dashboard" replace />
          },
          {
            path: '/dashboard',
            element: <DashboardPage />
          },
          {
            path: '/activity',
            element: <ActivityPage />
          },
          {
            path: '/workout',
            element: <WorkoutPage />
          },
          {
            path: '/templates',
            element: <WorkoutTemplatePage />
          },
          {
            path: '/templates/:templateId',
            element: <WorkoutTemplatePage />
          },
          {
            path: '/admin',
            element: <AdminPage />
          },
          {
            path: '/profile',
            element: <ProfilePage />
          },
          {
            path: '/settings',
            element: <SettingsPage />
          }
        ]
      }
    ]
  },

  // Fallback
  {
    path: '*',
    element: <Navigate to="/" replace />
  }
]);

// Root wrapper with providers
export function AppRouter() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
        <Toaster />
      </AuthProvider>
    </QueryClientProvider>
  );
}
