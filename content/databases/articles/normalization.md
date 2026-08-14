---
title: Нормализация без боли
order: 1
summary: 1НФ, 2НФ, 3НФ на одном сквозном примере.
---

Нормализация — это не ритуал, а способ не хранить один факт дважды.

## Ненормализованная таблица

```sql
CREATE TABLE orders_bad (
    order_id INT,
    customer_name NVARCHAR(100),
    customer_phone NVARCHAR(20),
    items NVARCHAR(MAX) -- 'молоко;хлеб;сыр' — боль
);
```

Телефон клиента повторяется в каждом заказе, список товаров — строкой.
Обновить телефон = обновить все строки клиента. Забыли одну — данные разъехались.

## После нормализации

```sql
CREATE TABLE customers (id INT PRIMARY KEY, name NVARCHAR(100), phone NVARCHAR(20));
CREATE TABLE orders (id INT PRIMARY KEY, customer_id INT REFERENCES customers(id));
CREATE TABLE order_items (
    order_id INT REFERENCES orders(id),
    product_id INT REFERENCES products(id),
    qty INT,
    PRIMARY KEY (order_id, product_id)
);
```

> Правило запоминания 3НФ: каждый неключевой атрибут зависит
> от ключа, от всего ключа и ни от чего, кроме ключа.
