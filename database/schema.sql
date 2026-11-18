-- AuctionDesk: Создание схемы базы данных
-- Автор: Григорьев Никита, группа 9/2-РПО-24/1

USE master;
GO

-- Удаляем старую базу если есть
IF DB_ID(N'AuctionDesk') IS NOT NULL
BEGIN
    ALTER DATABASE AuctionDesk SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AuctionDesk;
END
GO

-- Создаём базу
CREATE DATABASE AuctionDesk;
GO

USE AuctionDesk;
GO

PRINT N'База данных AuctionDesk создана.';
GO

-- Таблица пользователей
IF OBJECT_ID('users', 'U') IS NOT NULL DROP TABLE users;
GO

CREATE TABLE users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    username NVARCHAR(100) NOT NULL UNIQUE,
    email NVARCHAR(150) NOT NULL UNIQUE,
    registration_date DATETIME NOT NULL DEFAULT GETDATE(),
    is_active BIT NOT NULL DEFAULT 1
);
GO

-- Таблица категорий
IF OBJECT_ID('categories', 'U') IS NOT NULL DROP TABLE categories;
GO

CREATE TABLE categories (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    category_name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(500)
);
GO

-- Таблица аукционов
IF OBJECT_ID('auctions', 'U') IS NOT NULL DROP TABLE auctions;
GO

CREATE TABLE auctions (
    auction_id INT PRIMARY KEY IDENTITY(1,1),
    auction_name NVARCHAR(200) NOT NULL,
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    status NVARCHAR(20) NOT NULL CHECK (status IN ('active', 'completed', 'cancelled')),
    created_by INT NOT NULL,
    FOREIGN KEY (created_by) REFERENCES users(user_id)
);
GO

-- Таблица лотов
IF OBJECT_ID('lots', 'U') IS NOT NULL DROP TABLE lots;
GO

CREATE TABLE lots (
    lot_id INT PRIMARY KEY IDENTITY(1,1),
    lot_name NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000),
    starting_price DECIMAL(10,2) NOT NULL CHECK (starting_price > 0),
    current_price DECIMAL(10,2),
    category_id INT NOT NULL,
    auction_id INT NOT NULL,
    seller_id INT NOT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(category_id),
    FOREIGN KEY (auction_id) REFERENCES auctions(auction_id),
    FOREIGN KEY (seller_id) REFERENCES users(user_id)
);
GO

-- Таблица ставок
IF OBJECT_ID('bids', 'U') IS NOT NULL DROP TABLE bids;
GO

CREATE TABLE bids (
    bid_id INT PRIMARY KEY IDENTITY(1,1),
    lot_id INT NOT NULL,
    user_id INT NOT NULL,
    bid_amount DECIMAL(10,2) NOT NULL CHECK (bid_amount > 0),
    bid_time DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (lot_id) REFERENCES lots(lot_id),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
GO

-- Таблица победителей
IF OBJECT_ID('winners', 'U') IS NOT NULL DROP TABLE winners;
GO

CREATE TABLE winners (
    winner_id INT PRIMARY KEY IDENTITY(1,1),
    lot_id INT NOT NULL UNIQUE,
    user_id INT NOT NULL,
    final_price DECIMAL(10,2) NOT NULL,
    win_date DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (lot_id) REFERENCES lots(lot_id),
    FOREIGN KEY (user_id) REFERENCES users(user_id)
);
GO

-- Индексы
CREATE INDEX IX_lots_auction_id ON lots(auction_id);
GO
CREATE INDEX IX_lots_category_id ON lots(category_id);
GO
CREATE INDEX IX_bids_lot_id ON bids(lot_id);
GO
CREATE INDEX IX_bids_user_id ON bids(user_id);
GO
CREATE INDEX IX_auctions_status ON auctions(status);
GO

PRINT N'Таблицы и индексы созданы.';
GO

-- Представление для отчёта
IF OBJECT_ID('vw_auction_report', 'V') IS NOT NULL DROP VIEW vw_auction_report;
GO

CREATE VIEW vw_auction_report AS
SELECT 
    a.auction_name,
    COUNT(DISTINCT l.lot_id) AS total_lots,
    COUNT(w.winner_id) AS sold_lots,
    ISNULL(SUM(w.final_price), 0) AS total_revenue
FROM auctions a
LEFT JOIN lots l ON a.auction_id = l.auction_id
LEFT JOIN winners w ON l.lot_id = w.lot_id
WHERE a.status = 'completed'
GROUP BY a.auction_name;
GO

PRINT N'Представление отчёта создано.';
GO

PRINT N'=== Схема базы данных AuctionDesk готова! ===';
GO
