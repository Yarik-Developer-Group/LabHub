---
title: ПР2. Таблиця та 20 запитів
order: 3
summary: Перша таблиця у MS SQL Server: типи, обмеження та два десятки запитів.
---

## Мета

Спроєктувати таблицю для сутності вашого варіанта і навчитися діставати
з неї дані різними способами.

## Перед виконанням

Теми, які варто розібрати:

- Типи даних MS SQL Server: числові (`int`, `bigint`, `decimal`), рядкові
  (`varchar` vs `nvarchar`), дати й час (`date`, `datetime2`), `bit`,
  `uniqueidentifier`. Що шукати: *SQL Server data types best practices*.
- Обмеження цілісності: `NULL`/`NOT NULL`, `DEFAULT`, `PRIMARY KEY`, `UNIQUE`,
  `CHECK`. Що шукати: *SQL Server constraints*.
- Обчислювані колонки. Що шукати: *computed columns SQL Server, persisted*.
- Базові оператори: `SELECT`, `INSERT`, `UPDATE`, `DELETE`, а також `TOP`,
  `OFFSET/FETCH`, `ORDER BY`, `LIKE`, `IN`, `IS NULL`, `BETWEEN`,
  логічні `AND`/`OR`/`NOT`.
- Цикл `WHILE` у T-SQL — для генерації тестових даних.

## Завдання

1. Створити таблицю відповідно до вашого варіанта:
   - коректно обрати **типи даних** для колонок;
   - правильно проставити обмеження **NULL / NOT NULL**;
   - задати **DEFAULT-значення** там, де це доречно;
   - додати **PRIMARY KEY**, **UNIQUE** та **CHECK** обмеження;
   - додати **computed-колонки** за потреби.
2. Написати запити для заповнення таблиці:
   - одним значенням;
   - **N** новими записами (N задається у змінній) з використанням циклу `WHILE`.
3. Написати загалом **20 різних запитів**, використовуючи `SELECT`, `UPDATE`,
   `DELETE` і комбінуючи (де можливо): `TOP`, `OFFSET`, `FETCH`, `ORDER BY`,
   `LIKE`, `IN`, `IS NULL`, `IS NOT NULL`, `AND`, `OR`, `NOT`, `BETWEEN` тощо.

## Як здавати

Pull request зі скриптами: DDL таблиці, скрипти заповнення та всі 20 запитів.
