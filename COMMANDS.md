# FamFit — Команды запуска

## 🔧 Backend (.NET)

### Локальный запуск
```bash
# Скрипт (рекомендуется)
./run-backend.sh

# Или напрямую
cd src/FamFit.WebApi
dotnet restore
dotnet build
dotnet run --urls "http://localhost:5000"
```

### С параметрами
```bash
# С указанием environment
dotnet run --urls "http://localhost:5000" --environment Development

# С профилем запуска
dotnet run --launch-profile Development

# С hot reload
dotnet watch run --urls "http://localhost:5000"
```

### Сборка
```bash
cd src/FamFit.WebApi
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

---

## 🌐 Frontend (React + Vite)

### Локальная разработка (Backend на localhost)
```bash
# Скрипт (рекомендуется)
./run-frontend.sh

# Или напрямую
cd src/FamFit.Frontend
npm install
npm run dev
```

### С выбором окружения
```bash
# Локальное (Backend: localhost:5000)
./run-frontend-env.sh local

# Удалённый (Backend: 192.168.1.97, CORS обходится)
./run-frontend-env.sh staging

# LAN с Origin rewrite
./run-frontend-env.sh lan

# Продакшен сборка
./run-frontend-env.sh production
```

### Скрипты npm
```bash
cd src/FamFit.Frontend

# Dev-сервер (http://localhost:5173)
npm run dev

# Production сборка
npm run build

# Preview production сборки
npm run preview

# Линтинг
npm run lint

# Генерация типов из OpenAPI
npm run openapi:generate
```

---

## 🐳 Docker

### Запуск всего стека
```bash
# Скрипт (рекомендуется)
./run-docker.sh

# Или напрямую
docker-compose up --build
```

### Команды Docker
```bash
# Запуск в фоне
docker-compose up -d

# Остановка
docker-compose down

# Пересборка
docker-compose up --build

# Логи
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f postgres

# Очистка (удаление образов и volumes)
docker-compose down -v --rmi all
```

---

## 📋 Сервисы и порты

| Сервис | Порт | URL |
|---|---|---|
| PostgreSQL | 5432 | localhost:5432 |
| Backend | 5000 | http://localhost:5000 |
| Swagger | 5000 | http://localhost:5000/swagger |
| Frontend (dev) | 5173 | http://localhost:5173 |
| Frontend (prod) | 80 | http://localhost:80 |
| Docker Backend | 5000 | http://localhost:5000 |
| Docker Frontend | 80 | http://localhost:80 |

---

## 🔗 Сценарии разработки

### Сценарий 1: Полная локальная разработка
```bash
# Терминал 1
./run-backend.sh

# Терминал 2
./run-frontend.sh
# или
./run-frontend-env.sh local
```

### Сценарий 2: Локальный фронт + удалённый бэкенд
```bash
# Только фронтенд (бэкенд на 192.168.1.97)
./run-frontend-env.sh staging
```

### Сценарий 3: Полный стек в Docker
```bash
./run-docker.sh
```

### Сценарий 4: Быстрый перезапуск бэкенда с hot reload
```bash
cd src/FamFit.WebApi
dotnet watch run --urls "http://localhost:5000"
```

---

## 📝 Переменные окружения

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "FamFit": "Host=localhost;Port=5432;Database=famfit;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "FamFit.WebApi",
    "Audience": "FamFitClient"
  }
}
```

### Frontend (.env.local)
```bash
# Для локальной разработки
VITE_API_URL=http://localhost:5000/api
VITE_BACKEND_TARGET=http://localhost:5000

# Для удалённого бэкенда (CORS обходится через proxy)
VITE_API_URL=http://192.168.1.97/api
VITE_BACKEND_TARGET=http://192.168.1.97
```

---

## 🔍 Полезные команды

### Проверка здоровья сервисов
```bash
# Backend health check
curl http://localhost:5000/health

# Frontend health check (production)
curl http://localhost/health
```

### Генерация VAPID ключей
```bash
cd src/FamFit.Infrastructure
dotnet run -- generate-vapid
```

### EF Core миграции
```bash
cd src/FamFit.WebApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Генерация TypeScript типов
```bash
cd src/FamFit.Frontend
npm run openapi:generate
```
