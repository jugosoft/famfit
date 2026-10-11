# FamFit — Семейный PWA-трекер активности

Монорепозиторий с Clean Architecture, SOLID и CQRS (MediatR).

## Структура

```
src/
├── FamFit.Domain/          # Ядро: сущности,枚举ы
├── FamFit.Application/     # Бизнес-логика: CQRS, интерфейсы, MediatR
├── FamFit.Infrastructure/  # Техника: EF Core, PostgreSQL, WebPush (VAPID)
├── FamFit.WebApi/          # Презентация: Minimal API, JWT, Swagger
└── FamFit.Frontend/        # React 19 + Vite + TypeScript + Tailwind + Shadcn UI
```

## Быстрый старт

### Backend

```bash
cd src/FamFit.WebApi
dotnet restore
dotnet run
```

API откроется на `http://localhost:5000`
Swagger: `http://localhost:5000/swagger`

### Frontend

```bash
cd src/FamFit.Frontend
npm install
npm run dev
```

Фронтенд откроется на `http://localhost:5173`

### Генерация TypeScript-типов из OpenAPI

```bash
# Запустите бэкенд, затем:
npm run openapi:generate
```

## Архитектура

- **Domain**: чистые бизнес-сущности, без внешних зависимостей
- **Application**: CQRS (MediatR), интерфейсы, валидация
- **Infrastructure**: EF Core + PostgreSQL, WebPush VAPID
- **WebApi**: Minimal API, JWT auth, Swagger/OpenAPI
- **Frontend**: React 19, TypeScript, Tailwind CSS, Shadcn UI, Zustand, React Query

## Роли

- **FamilyAdmin**: управляет ActivityType в админке
- **FamilyMember**: логирует только свою активность

## Web Push (VAPID)

1. Сгенерируйте ключи:
   ```bash
   cd src/FamFit.Infrastructure
   dotnet run -- generate-vapid
   ```
2. Скопируйте ключи в `appsettings.json`
3. Фронтенд получает публичный ключ через `GET /api/vapid/public-key`
4. При записи активности можно отправить push с кнопками действий
