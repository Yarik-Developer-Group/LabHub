---
title: Способи зберігання даних у додатку
order: 6
summary: Пам'ять, файли, NoSQL та реляційна база — підготовка до ПР7.
---

Теми для вивчення перед [ПР7](/c/databases/practice/pr-07-storage):

- Патерн **Repository** та абстракція над сховищем: чому додаток не повинен
  знати, де саме лежать дані. Що шукати: *repository pattern C#*.
- Серіалізація у **JSON**: `System.Text.Json`. Що шукати: *System.Text.Json serialize*.
- **Redis** як key-value сховище: встановлення на Windows (як windows service),
  базові команди. Що шукати: *Redis on Windows*, *StackExchange.Redis*.
- Бінарна серіалізація **Protobuf**. Що шукати: *protobuf-net C#*.
- Доступ до **SQL Server** з C#: `Microsoft.Data.SqlClient`, і головне —
  **параметризовані запити** (чому конкатенація рядків у SQL — це дорога
  до SQL-ін'єкції). Що шукати: *SqlCommand parameters, SQL injection*.
