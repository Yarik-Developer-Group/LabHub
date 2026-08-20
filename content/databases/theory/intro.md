---
title: Вступ до баз даних
order: 1
summary: Що таке СУБД, які бувають провайдери, з чого почати і що встановити.
---

## Презентація

Основний матеріал вступної лекції — у презентації:
[Вступ до баз даних (Google Slides)](https://docs.google.com/presentation/d/1KVTtGmWPTM9TqIzDbkTKTaxcFHkAnPHY57FFC7sQ81Q/edit).

Нижче — короткий конспект і корисні посилання.

## Провайдери баз даних

Реляційних СУБД багато: **MS SQL Server**, **PostgreSQL**, **MySQL/MariaDB**,
**Oracle**, **SQLite**. Для нашого курсу підійде майже будь-яка «серверна» —
ви обираєте між MS SQL Server та PostgreSQL на початку семестру.

Окремо про **SQLite**: це вбудована однокористувацька база — файл поруч
з додатком, без сервера. Ідеальна для мобільного застосунку чи локальної
програми, але для багатокористувацьких систем (а ми вчимося саме таким)
вона не призначена.

Продуктивність бази впирається не туди, куди здається на перший погляд —
про це окрема сторінка: [Компоненти ПК і що з них важливо для БД](/c/databases/theory/pc-components).

## Що встановити

- **SSMS** (IDE для роботи з MS SQL):
  [завантажити](https://aka.ms/ssms/21/release/vs_SSMS.exe)
- **SQL Server Database Engine**:
  [інструкція зі встановлення](https://learn.microsoft.com/en-us/ssms/install/install)
- **АБО**, як альтернатива, **PostgreSQL**:
  [завантажити для Windows](https://www.postgresql.org/download/windows/)

## Інструменти розробника (для 2 курсу)

Роботи здаються через pull request, тому знадобиться git і хмарний сервіс
для коду:

- **Git for Windows**: [завантажити](https://git-scm.com/download/win)
- **Bitbucket** (сервіс для збереження вашого коду у хмарі):
  [bitbucket.org](https://bitbucket.org/) — або, як альтернатива,
  [GitHub](https://github.com/)

Покрокові інструкції — у розділі [«Загальне»](/c/general):
[як створити воркспейс та репозиторій](/c/general/bitbucket-workspace-repo),
[як завантажити репозиторій локально](/c/general/clone-repo-visual-studio),
[як зробити перший коміт](/c/general/first-commit-visual-studio).
