# ============================================
# Этап 1: Сборка (.NET 9 SDK)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем solution и project файлы
COPY ["FamFit.sln", "./"]
COPY ["src/FamFit.Domain/FamFit.Domain.csproj", "src/FamFit.Domain/"]
COPY ["src/FamFit.Application/FamFit.Application.csproj", "src/FamFit.Application/"]
COPY ["src/FamFit.Infrastructure/FamFit.Infrastructure.csproj", "src/FamFit.Infrastructure/"]
COPY ["src/FamFit.WebApi/FamFit.WebApi.csproj", "src/FamFit.WebApi/"]

# Восстанавливаем зависимости (слои с кэшированием Docker)
RUN dotnet restore "FamFit.WebApi/FamFit.WebApi.csproj"

# Копируем весь исходный код
COPY . .

# Собираем без тестов
WORKDIR "/src/src/FamFit.WebApi"
RUN dotnet publish "FamFit.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ============================================
# Этап 2: Рантайм (маленький образ)
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Устанавливаем tzdata для корректной работы с таймзонами
RUN apk add --no-cache tzdata icu-data-full icu-libs
ENV TZ=Europe/Moscow

# Копируем собранное приложение
COPY --from=build /app/publish .

# Открываем порт
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Пользователь для безопасности
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Точка входа
ENTRYPOINT ["dotnet", "FamFit.WebApi.dll"]
