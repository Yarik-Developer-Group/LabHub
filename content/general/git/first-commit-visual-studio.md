---
title: Як зробити коміт
order: 3
summary: Перший коміт і push на прикладі Visual Studio.
---

Покрокова інструкція на прикладі Visual Studio.

## 1. Змініть щось у репозиторії

Наприклад, оновіть вміст **.gitignore** з
[шаблону для Visual Studio](https://github.com/github/gitignore/blob/main/VisualStudio.gitignore).

## 2. Напишіть коментар до коміту

Коротко і по суті — що саме змінили (наприклад: `update git ignore`).

![Коментар до коміту](img/vs-05-commit-message.png)

## 3. Натисніть Commit All

Коміт створюється **локально** — на сервері його ще ніхто не бачить.

![Кнопка Commit All](img/vs-06-commit-all.png)

## 4. Натисніть Push

Зміни відправляються у хмарний репозиторій. **Тільки після push**
вони видимі викладачу.

![Кнопка Push](img/vs-07-push.png)

> У консолі ті самі кроки: `git add -A`,
> `git commit -m "update git ignore"`, `git push`.
