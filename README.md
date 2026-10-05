# StudWork API

Backend маркетплейса StudWork: заказчики публикуют задания, исполнители отправляют отклики, а участники обсуждают работу в сообщениях. API также обслуживает профили, отзывы, загрузку файлов и административные операции.

Интерфейс приложения находится в [stud-front](https://github.com/STYOP2122/stud-front).

## Технологии

ASP.NET Core, .NET 10, Entity Framework Core, SQLite, JWT и BCrypt.

## Локальный запуск

Установите .NET 10 SDK.

```bash
git clone https://github.com/STYOP2122/stud-back.git
cd stud-back/Studwork.Api
dotnet restore
dotnet run
```

API запускается по адресу `http://localhost:5000` с профилем из `Properties/launchSettings.json`. Настройки подключения к БД, JWT и CORS находятся в `appsettings.json` и могут переопределяться переменными окружения, например `Jwt__Key` и `Cors__Origins`.

**В режиме Development база данных удаляется и создаётся заново при каждом запуске.** Этот режим предназначен для демонстрационных данных.

## Docker

Из корня репозитория:

```bash
docker build -t studwork-api .
docker run -p 8080:8080 -e Cors__Origins=http://localhost:5173 studwork-api
```

API будет доступен по адресу `http://localhost:8080`. Для повторного использования данных при пересоздании контейнера подключите именованные тома к `/app/data` и `/app/uploads`.

## Демонстрационные аккаунты

| Email | Пароль | Роль |
|---|---|---|
| `customer@test.com` | `123456` | Заказчик |
| `executor@test.com` | `123456` | Исполнитель PRO |
| `writer@test.com` | `123456` | Исполнитель |
| `admin@test.com` | `123456` | Администратор |

Аккаунты добавляет `Data/DbSeeder.cs`.

## Структура

- `Controllers/` — API авторизации, заказов, откликов, сообщений, файлов, отзывов и администрации.
- `Entities/` и `DTOs/` — модели данных и объекты запросов и ответов.
- `Data/` — контекст SQLite и демонстрационные данные.
- `Services/` — токены, файлы, проверки доступа и работа с диалогами.
