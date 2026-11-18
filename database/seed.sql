-- AuctionDesk: Тестовые данные
-- Автор: Григорьев Никита, группа 9/2-РПО-24/1

USE AuctionDesk;
GO

-- Очищаем таблицы в обратном порядке
DELETE FROM winners;
DELETE FROM bids;
DELETE FROM lots;
DELETE FROM auctions;
DELETE FROM categories;
DELETE FROM users;
GO

-- Пользователи
INSERT INTO users (username, email) VALUES
(N'ivan_petrov',      N'ivan@example.com'),
(N'maria_ivanova',    N'maria@example.com'),
(N'alexey_smirnov',   N'alexey@example.com'),
(N'elena_kuznetsova', N'elena@example.com'),
(N'dmitry_volkov',    N'dmitry@example.com');
GO

-- Категории
INSERT INTO categories (category_name, description) VALUES
(N'Искусство',          N'Картины, скульптуры'),
(N'Антиквариат',        N'Старинные вещи'),
(N'Электроника',        N'Гаджеты и техника'),
(N'Коллекционирование', N'Марки, монеты'),
(N'Ювелирные изделия',  N'Украшения');
GO

-- Аукционы (формат даты: YYYY-MM-DDTHH:MM:SS)
INSERT INTO auctions (auction_name, start_date, end_date, status, created_by) VALUES
(N'Весенний аукцион',  '2026-03-01T10:00:00', '2026-03-15T18:00:00', N'completed', 1),
(N'Летняя распродажа', '2026-06-01T10:00:00', '2026-06-20T18:00:00', N'completed', 2),
(N'Осенний аукцион',   '2026-10-01T10:00:00', '2026-11-30T18:00:00', N'active',    3);
GO

-- Лоты
INSERT INTO lots (lot_name, description, starting_price, current_price, category_id, auction_id, seller_id) VALUES
(N'Картина "Закат"',  N'Масло на холсте',       5000.00,  7500.00, 1, 1, 1),
(N'Старинные часы',   N'19 век',               15000.00, 22000.00, 2, 2, 2),
(N'iPhone 15 Pro',    N'Новый',                80000.00, 95000.00, 3, 3, 3),
(N'Коллекция марок',  N'50 марок СССР',         3000.00,  4500.00, 4, 1, 4),
(N'Золотое кольцо',   N'18 карат',             25000.00, 32000.00, 5, 2, 5);
GO

-- Ставки
INSERT INTO bids (lot_id, user_id, bid_amount, bid_time) VALUES
(1, 2,  5500.00, '2026-03-02T11:00:00'),
(1, 3,  6000.00, '2026-03-03T12:30:00'),
(1, 4,  7500.00, '2026-03-14T15:00:00'),
(2, 1, 16000.00, '2026-06-02T10:15:00'),
(2, 3, 18000.00, '2026-06-05T14:20:00'),
(2, 5, 22000.00, '2026-06-19T17:40:00'),
(3, 1, 85000.00, '2026-10-05T09:00:00'),
(3, 2, 90000.00, '2026-10-12T13:10:00'),
(3, 4, 95000.00, '2026-11-10T16:45:00'),
(4, 1,  3500.00, '2026-03-04T11:30:00'),
(4, 3,  4500.00, '2026-03-13T19:05:00'),
(5, 2, 27000.00, '2026-06-07T12:00:00'),
(5, 4, 32000.00, '2026-06-18T18:25:00');
GO

-- Победители
INSERT INTO winners (lot_id, user_id, final_price, win_date) VALUES
(1, 4,  7500.00, '2026-03-15T18:05:00'),
(2, 5, 22000.00, '2026-06-20T18:05:00');
GO

-- Проверка результата
SELECT N'users'      AS [Таблица], COUNT(*) AS [Записей] FROM users
UNION ALL SELECT N'categories', COUNT(*) FROM categories
UNION ALL SELECT N'auctions',   COUNT(*) FROM auctions
UNION ALL SELECT N'lots',       COUNT(*) FROM lots
UNION ALL SELECT N'bids',       COUNT(*) FROM bids
UNION ALL SELECT N'winners',    COUNT(*) FROM winners;
GO

PRINT N'=== Тестовые данные успешно загружены! ===';
GO
