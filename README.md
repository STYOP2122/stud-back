# StudWork Backend

ASP.NET Core API для маркетплейса StudWork.

Репозиторий фронта: [stud-front](https://github.com/STYOP2122/stud-front)

## Локальный запуск

```bash
cd Studwork.Api
dotnet run
```

## Docker локально

```bash
docker build -t studwork-api .
docker run -p 8080:8080 -e Cors__Origins=http://localhost:5173 studwork-api
```

## Тестовые аккаунты

| Email | Пароль | Роль |
|---|---|---|
| `customer@test.com` | `123456` | Заказчик |
| `executor@test.com` | `123456` | Исполнитель (PRO) |
| `writer@test.com` | `123456` | Исполнитель |
| `admin@test.com` | `123456` | Админ |
