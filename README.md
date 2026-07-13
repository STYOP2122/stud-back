# StudWork Backend

ASP.NET Core API для маркетплейса StudWork.

Репозиторий фронта: [stud-front](https://github.com/STYOP2122/stud-front)

## Локальный запуск

```bash
cd Studwork.Api
dotnet run
```

API: http://localhost:5000

## Деплой на Render (бесплатно)

GitHub Pages **не умеет** хостить .NET — API нужно поднять отдельно.

1. Зарегистрируйтесь на [render.com](https://render.com)
2. **New → Blueprint** → подключите этот репозиторий (`render.yaml`)
   - или **New → Web Service** → Docker, root = репозиторий
3. После деплоя скопируйте URL вида `https://studwork-api.onrender.com`
4. Во фронте [stud-front](https://github.com/STYOP2122/stud-front):
   - **Settings → Secrets and variables → Actions → Variables**
   - `VITE_API_URL` = `https://studwork-api.onrender.com`
5. Перезапустите workflow Deploy на фронте (Actions → Deploy to GitHub Pages → Run workflow)

### CORS

В переменных окружения Render:

```
Cors__Origins=https://STYOP2122.github.io,http://localhost:5173
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

> На бесплатном плане Render сервис «засыпает» после ~15 мин простоя — первый запрос может занять 30–60 сек.
