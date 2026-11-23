USE AuctionDesk;
GO

DELETE FROM winners;
DELETE FROM bids;
DELETE FROM lots;
DELETE FROM categories;
DELETE FROM users;
GO

INSERT INTO users (username) VALUES
(N'ivan_petrov'), (N'maria_ivanova'), (N'alexey_smirnov'),
(N'elena_kuznetsova'), (N'dmitry_volkov');
GO

INSERT INTO categories (category_name) VALUES
(N'Искусство'), (N'Антиквариат'), (N'Электроника'),
(N'Коллекционирование'), (N'Ювелирные изделия');
GO

INSERT INTO lots (lot_name, description, starting_price, current_price, category_id, seller_id,
                  duration_seconds, start_date, end_date, finished_date, status) VALUES
(N'Картина "Закат"', N'Масло на холсте, 50x70 см', 5000.00, 7500.00, 1, 1,
 600, GETDATE(), DATEADD(SECOND, 600, GETDATE()), NULL, 'active'),
(N'Старинные часы', N'Карманные часы XIX века', 15000.00, 22000.00, 2, 2,
 300, GETDATE(), DATEADD(SECOND, 300, GETDATE()), NULL, 'active'),
(N'iPhone 15 Pro', N'Новый, в заводской упаковке', 80000.00, 95000.00, 3, 3,
 120, GETDATE(), DATEADD(SECOND, 120, GETDATE()), NULL, 'active'),
(N'Коллекция марок', N'50 редких марок СССР', 3000.00, 4500.00, 4, 4,
 60, DATEADD(MINUTE, -10, GETDATE()), DATEADD(MINUTE, -9, GETDATE()), DATEADD(MINUTE, -9, GETDATE()), 'completed'),
(N'Золотое кольцо', N'18 карат, с бриллиантом', 25000.00, 32000.00, 5, 5,
 90, DATEADD(MINUTE, -20, GETDATE()), DATEADD(MINUTE, -18, GETDATE()), DATEADD(MINUTE, -18, GETDATE()), 'completed');
GO

INSERT INTO bids (lot_id, user_id, bid_amount) VALUES
(1, 2, 5500.00), (1, 3, 6000.00), (1, 4, 7500.00),
(2, 1, 16000.00), (2, 3, 18000.00), (2, 5, 22000.00),
(3, 1, 85000.00), (3, 2, 90000.00), (3, 4, 95000.00),
(4, 1, 3500.00), (4, 3, 4500.00),
(5, 2, 27000.00), (5, 4, 32000.00);
GO

UPDATE lots SET winner_id = 3 WHERE lot_id = 4;
UPDATE lots SET winner_id = 4 WHERE lot_id = 5;
GO

INSERT INTO winners (lot_id, user_id, final_price) VALUES
(4, 3, 4500.00), (5, 4, 32000.00);
GO

SELECT N'users' AS [Таблица], COUNT(*) AS [Записей] FROM users
UNION ALL SELECT N'categories', COUNT(*) FROM categories
UNION ALL SELECT N'lots', COUNT(*) FROM lots
UNION ALL SELECT N'bids', COUNT(*) FROM bids
UNION ALL SELECT N'winners', COUNT(*) FROM winners;
GO