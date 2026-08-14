# LabHub

Сайт лабораторных и учебных материалов: ASP.NET Core 8 + Markdown-контент в git.
Публичная часть открыта всем, админка — по Google-входу для преподавателей из allowlist.
Рассчитан на бесплатный Azure App Service F1: контент живёт в памяти, обновляется
из GitHub **без рестарта** приложения.

## Архитектура в трёх абзацах

Контент — папка `content/` в этом же репозитории: дерево «курс → раздел → лаба»
любой вложенности, каждая страница — markdown с front-matter (`title`, `order`,
`summary`, `track`). Картинки лежат рядом с лабой и раздаются через `/assets/…`
с проверкой пути и allowlist-ом расширений. Base данных нет — и не нужна:
версионирование, история и бэкап контента бесплатно даёт git.

При старте приложение читает `content/`, рендерит markdown (Markdig, сырой HTML
экранируется) и держит всё в иммутабельном снимке в памяти — страницы отдаются
за миллисекунды. Обновление: GitHub-webhook дёргает `POST /api/content/refresh`
с HMAC-подписью → приложение скачивает свежий tarball контента → собирает новый
снимок → атомарно подменяет старый. Ни рестарта, ни простоя. Подстраховка —
фоновая сверка раз в `RefreshMinutes` минут.

Безопасность: Google OAuth на встроенном OAuth-хендлере (пакетов не нужно),
allowlist преподавателей в конфиге, строгий CSP без inline-скриптов, security
headers на каждом ответе, антифорджери на формах, проверка подписи webhook за
постоянное время, защита от path traversal. Любая ошибка — аккуратная страница
или 5xx, приложение не падает: битый md-файл пропускается, недоступный GitHub
не мешает старту с локальной копией.

## Локальный запуск

```bash
dotnet run --project src/LabHub.Web        # http://localhost:5204
dotnet run --project tests/LabHub.Tests    # 56 тестов, ненулевой код = провал
python3 tools/check_style.py               # правила: ≤3 параметров, ≤30 строк
```

Контент правится в `content/`, после рестарта (или кнопки «Перезагрузить
контент» в `/admin`) изменения видны.

## Структура контента

```
content/
  networks/                 курс
    index.md                титул курса (title, order, summary + текст)
    articles/               раздел
      index.md
      osi-model.md          статья (leaf-файл)
    labs/
      index.md
      lab-01-wireshark/     лаба с картинками
        index.md
        img/topology.svg    в md: ![схема](img/topology.svg)
  databases/
    labs/lab-02-sql/extra/  экстра-задания по трекам
      developer.md          front-matter: track: Разработчик
      dba.md                track: Администратор БД
```

Front-matter:

```markdown
---
title: Лаба 2. SQL
order: 2
summary: Короткое описание для списка.
track: Разработчик        # только для экстра-заданий, группирует по трекам
---
```

Папки с `_` или `.` в начале имени игнорируются (черновики).

## Деплой на Azure (тариф F1, $0/мес)

1. Создайте Web App: Linux, .NET 8, план **F1 (Free)**. Имя = адрес
   `<имя>.azurewebsites.net`.
2. Скачайте publish profile (кнопка *Get publish profile* в Overview).
3. В GitHub-репозитории: *Settings → Secrets and variables → Actions*:
   - secret `AZURE_WEBAPP_PUBLISH_PROFILE` — содержимое publish profile;
   - variable `AZURE_WEBAPP_NAME` — имя Web App.
4. Пуш в `main` → workflow прогоняет тесты и деплоит.

Workflow настроен так, что правки только в `content/` **не** запускают деплой —
их доставляет webhook (ниже).

### App Settings (Configuration → Application settings)

| Ключ | Значение |
|---|---|
| `LabHub__WebhookSecret` | случайная строка 32+ символов (`openssl rand -hex 32`) |
| `LabHub__GitHub__Repo` | `владелец/репозиторий` |
| `LabHub__GitHub__Branch` | `main` (по умолчанию) |
| `LabHub__GitHub__Token` | PAT с правом чтения (опционально, для приватного репо и щедрого rate-limit) |
| `Authentication__Google__ClientId` | из Google Cloud Console |
| `Authentication__Google__ClientSecret` | оттуда же |

Секреты живут только здесь (и в user-secrets локально) — в git им не место.

### Google OAuth

1. [Google Cloud Console](https://console.cloud.google.com/) → APIs & Services →
   Credentials → Create OAuth client ID (Web application).
2. Authorized redirect URI: `https://<имя>.azurewebsites.net/signin-google`.
3. ClientId/ClientSecret → в App Settings.
4. Свой email — в `LabHub:Teachers` в `appsettings.json` (это не секрет).

Локально: `dotnet user-secrets set "Authentication:Google:ClientId" "..."`
(в папке `src/LabHub.Web`, потребуется `dotnet user-secrets init`).

### Webhook горячего обновления контента

GitHub-репозиторий → Settings → Webhooks → Add webhook:

- Payload URL: `https://<имя>.azurewebsites.net/api/content/refresh`
- Content type: `application/json`
- Secret: то же значение, что в `LabHub__WebhookSecret`
- Events: *Just the push event*

Коммит в `content/` → лаба на сайте через ~10 секунд, без рестарта.

### Пинг против засыпания F1

F1 останавливает приложение после ~20 минут без запросов. Заведите бесплатный
монитор [UptimeRobot](https://uptimerobot.com) на
`https://<имя>.azurewebsites.net/healthz` с интервалом 5 минут — это и
keep-alive, и алерт «сайт упал». Редкие холодные старты при обслуживании
платформы всё равно возможны — это принятый компромисс тарифа за $0.

## Тесты

Собственный мини-раннер (`tests/LabHub.Tests/MicroTest.cs`, ~100 строк) вместо
xunit — осознанно: проект собирается и тестируется полностью офлайн, без NuGet.
Юнит-тесты покрывают парсер front-matter, рендер markdown (включая экранирование
HTML и вычистку `javascript:`), HMAC-подпись, allowlist преподавателей, защиту
от path traversal, загрузчик дерева. Интеграционные — поднимают настоящее
приложение на локальном порту и проверяют страницы, 404, security headers,
webhook (включая горячую подмену контента) и редирект админки. Захотите xunit —
атрибуты `[Test]` меняются на `[Fact]`, `Check` на `Assert`, полчаса работы.

## Вендоренный Markdig

`third_party/Markdig` — исходники [Markdig](https://github.com/xoofx/markdig)
0.37.0 (BSD-2-Clause, лицензия приложена). Вкомпилирован исходниками, чтобы
сборка не зависела от NuGet. Можно заменить на `PackageReference` в одну строку.

## Куда расти

Отзывы студентов — [giscus](https://giscus.app) (комментарии на GitHub
Discussions, ноль инфраструктуры). Поиск — индекс по снимку уже в памяти,
осталась только страница. Переезд с F1 куда угодно — приложение самодостаточно,
Dockerfile добавляется тривиально.
