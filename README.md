# FamFit — Семейный PWA-трекер активности

Монорепозиторий с Clean Architecture, SOLID и CQRS (MediatR).

```
FamFit/
├── FamFit.sln
├── Directory.Build.props
├── docker-compose.yml
├── .dockerignore
├── .gitignore
├── README.md
├── COMMANDS.md                 # Все команды запуска
├── run-backend.sh              # Запуск бэкенда
├── run-frontend.sh             # Запуск фронтенда (локально)
├── run-frontend-env.sh         # Запуск фронтенда с выбором окружения
├── run-all.sh                  # Запуск всего стека
├── run-docker.sh               # Запуск через Docker Compose
└── src/
    ├── FamFit.Domain/          # Ядро: сущности, enum'ы
    ├── FamFit.Application/     # Бизнес-логика: CQRS, MediatR
    ├── FamFit.Infrastructure/  # EF Core, PostgreSQL, WebPush VAPID
    ├── FamFit.WebApi/          # Minimal API, JWT, Swagger
    │   ├── Dockerfile
    │   └── appsettings.json
    └── FamFit.Frontend/        # React 19, Vite, Tailwind, Shadcn UI
        ├── Dockerfile
        ├── nginx.conf
        ├── vite.config.ts
        └── src/
            ├── api/            # Типы + API client
            ├── components/     # UI Kit (12+ компонентов)
            ├── contexts/       # Auth context
            ├── hooks/          # React Query hooks
            ├── lib/            # http client, utils
            ├── pages/          # 6 страниц (Login, Dashboard, Activity, Admin, Profile, Settings)
            └── router.tsx      # React Router v6
```

## 🚀 Быстрый старт

### Вариант 1: Docker (рекомендуется)

```bash
# Запуск всего стека (PostgreSQL + Backend + Frontend)
./run-docker.sh

# Бэкенд: http://localhost:5000
# Фронтенд: http://localhost:80
# Swagger: http://localhost:5000/swagger
```

### Вариант 2: Локальная разработка

```bash
# Терминал 1 — Backend
./run-backend.sh

# Терминал 2 — Frontend (локально)
./run-frontend.sh
# или с выбором окружения:
./run-frontend-env.sh local    # Backend: localhost:5000
./run-frontend-env.sh staging  # Backend: 192.168.1.97 (CORS обходится)
./run-frontend-env.sh lan      # Backend: 192.168.1.97 (Origin rewrite)
```

> 📖 Полный список команд: [COMMANDS.md](./COMMANDS.md)

## 📐 Архитектура

| Слой | Технология | Описание |
|---|---|---|
| **Domain** | Чистый C# | Сущности, enum'ы, бизнес-правила |
| **Application** | MediatR | CQRS (Commands/Queries), интерфейсы |
| **Infrastructure** | EF Core + NPgsql | DbContext, VAPID Push, репозитории |
| **WebApi** | Minimal API | JWT auth, Swagger, rate limiting |
| **Frontend** | React 19 + TS | Tailwind CSS, Shadcn UI, Zustand, React Query |

## 🔐 Безопасность

- **JWT Bearer** с валидацией issuer/audience/lifetime
- **BCrypt** (cost=12) для хеширования паролей
- **Rate limiting** на auth endpoints (5 запросов / 15 мин)
- **CORS** с whitelist доменов
- **Security headers** в production (HSTS, X-Frame-Options, CSP)
- **Минимум 32 символа** для JWT secret
- **Password validation** на регистрации (мин. 8 символов)
- **Смена пароля** с проверкой текущего

## 📱 Страницы Frontend

| Страница | Маршрут | Описание |
|---|---|---|
| **Login** | `/login` | Вход / Регистрация |
| **Dashboard** | `/dashboard` | Статистика, серия, quick actions |
| **Activity** | `/activity` | Форма записи + история с пагинацией |
| **Admin** | `/admin` | CRUD типов активностей (только Admin) |
| **Profile** | `/profile` | Редактирование профиля + смена пароля |
| **Settings** | `/settings` | Настройки, уведомления, выход |

## 🎨 UI Kit (12+ компонентов)

- `Button` — с variant, size, isLoading
- `Input` — с label, error, hint, prefix, suffix
- `Card` — Card, CardHeader, CardTitle, CardContent, CardFooter
- `Select` — кастомный select с placeholder
- `Dialog` — модальное окно с backdrop
- `Badge` — default, secondary, destructive, outline
- `Avatar` — sm/md/lg с fallback
- `Table` — Table, TableHeader, TableBody, TableRow, TableHead, TableCell
- `Tabs` — горизонтальные табы
- `Progress` — прогресс-бар
- `Skeleton` — skeleton loading
- `Alert` — default, destructive, success, info
- `Pagination` — с ellipsis
- `StatCard` — карточка статистики с trend
- `EmptyState` — пустое состояние с action
- `Separator` — горизонтальный/вертикальный
- `DropdownMenu` — выпадающее меню
- `Stepper` — пошаговый индикатор

## 🐳 Docker

### Backend Dockerfile
- Multi-stage build: SDK → ASP.NET Core Alpine
- Размер образа ~200MB
- Автоматический seed при старте

### Frontend Dockerfile
- Multi-stage build: Node 22 → Nginx Alpine
- Gzip сжатие, SPA routing
- Proxy /api к бэкенду

### docker-compose.yml
- PostgreSQL 16 (volume)
- Backend (port 5000)
- Frontend (port 80)
- Health checks для всех сервисов

## 📡 API Endpoints

| Method | Endpoint | Auth | Описание |
|---|---|---|---|
| `POST` | `/api/auth/login` | Public | Вход → JWT |
| `POST` | `/api/auth/register` | Public | Регистрация |
| `PUT` | `/api/auth/change-password` | User | Смена пароля |
| `GET` | `/api/auth/me` | User | Текущий пользователь |
| `GET` | `/api/activity-types/` | Public | Список типов |
| `POST` | `/api/activity-types/` | Admin | Создать тип |
| `PUT` | `/api/activity-types/{id}/deactivate` | Admin | Деактивировать |
| `POST` | `/api/activity-logs/` | User | Записать активность |
| `POST` | `/api/activity-logs/with-notification` | User | Записать + push |
| `GET` | `/api/activity-logs/` | User | История (пагинация) |
| `POST` | `/api/push-subscriptions/` | User | Регистрация подписки |
| `DELETE` | `/api/push-subscriptions/{id}` | User | Удаление подписки |
| `GET` | `/api/vapid/public-key` | Public | VAPID public key |
| `GET` | `/health` | Public | Health check |

## 🏗️ Генерация типов

```bash
# После запуска бэкенда:
cd src/FamFit.Frontend
npm run openapi:generate
# Генерирует src/api/types/api.ts из swagger.json
```

## 📝 Роли

- **FamilyAdmin** — полный доступ, управление ActivityTypes
- **FamilyMember** — запись активности, просмотр своей истории

## 📦 Зависимости

### Backend
- .NET 9, EF Core 9, Npgsql
- MediatR 12, WebPush, BCrypt.Net-Next
- Microsoft.AspNetCore.OpenApi, Swashbuckle

### Frontend
- React 19, Vite 6, TypeScript 5.7
- Tailwind CSS 4, React Router 6
- Zustand 5, React Query 5
- Vite PWA Plugin
- date-fns, lucide-react
