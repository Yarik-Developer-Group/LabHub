---
title: Як зробити коміт
order: 3
summary: Перший коміт і push на прикладі Visual Studio.
---

Покрокова інструкція на прикладі Visual Studio.
## Кроки

1. Змініть щось у репозиторії — наприклад, оновіть вміст **.gitignore**
   з [шаблону для Visual Studio](https://github.com/github/gitignore/blob/main/VisualStudio.gitignore).
2. Напишіть **коментар до коміту** — коротко і по суті, що саме змінили
   (наприклад: `update git ignore`).
3. Натисніть **Commit All** — коміт створюється **локально**.
4. Натисніть **Push** — зміни відправляються у хмарний репозиторій.
   Тільки після push вони видимі викладачу.

> У консолі ті самі кроки: `git add -A`, `git commit -m "update git ignore"`,
> `git push`.
