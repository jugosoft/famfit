import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Dumbbell, Eye, EyeOff, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/Card';
import { Alert } from '@/components/ui/Alert';
import { useAuth } from '@/hooks/useAuth';
import { toast } from '@/components/ui/Toaster';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isRegister, setIsRegister] = useState(false);
  const { login, register, isLoading } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();

    if (isRegister) {
      try {
        await register({ name, email, password });
        toast.success('Регистрация успешна!');
        navigate('/dashboard');
      } catch (error) {
        const msg = error instanceof Error ? error.message : 'Ошибка регистрации';
        toast.error('Ошибка', msg);
      }
    } else {
      try {
        await login({ email, password });
        toast.success('Вход выполнен!');
        navigate('/dashboard');
      } catch (error) {
        const msg = error instanceof Error ? error.message : 'Неверный email или пароль';
        toast.error('Ошибка входа', msg);
      }
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md space-y-6">
        {/* Logo */}
        <div className="text-center space-y-2">
          <div className="inline-flex items-center justify-center gap-2 mb-2">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
              <Dumbbell className="h-6 w-6 text-primary" />
            </div>
          </div>
          <h1 className="text-3xl font-bold tracking-tight">FamFit</h1>
          <p className="text-muted-foreground">Семейный трекер активности</p>
        </div>

        <Card>
          <CardHeader className="space-y-1">
            <CardTitle>{isRegister ? 'Создать аккаунт' : 'Вход в аккаунт'}</CardTitle>
            <CardDescription>
              {isRegister
                ? 'Зарегистрируйтесь, чтобы начать тренироваться'
                : 'Введите данные для входа в систему'}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              {isRegister && (
                <Input
                  label="Имя"
                  placeholder="Ваше имя"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                  minLength={2}
                />
              )}

              <Input
                label="Email"
                type="email"
                placeholder="family@famfit.local"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />

              <Input
                label="Пароль"
                type={showPassword ? 'text' : 'password'}
                placeholder="Минимум 8 символов"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={8}
                suffix={
                  <button
                    type="button"
                    className="text-muted-foreground hover:text-foreground transition-colors"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                }
              />

              <Button type="submit" className="w-full" isLoading={isLoading}>
                {isLoading ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
                {isRegister ? 'Зарегистрироваться' : 'Войти'}
              </Button>
            </form>

            <div className="mt-4 text-center text-sm">
              <button
                type="button"
                className="text-primary hover:underline font-medium"
                onClick={() => setIsRegister(!isRegister)}
              >
                {isRegister ? 'Уже есть аккаунт? Войти' : 'Нет аккаунта? Зарегистрироваться'}
              </button>
            </div>
          </CardContent>
        </Card>

        <p className="text-center text-xs text-muted-foreground">
          Нажимая «Войти», вы соглашаетесь с условиями использования FamFit
        </p>
      </div>
    </div>
  );
}
