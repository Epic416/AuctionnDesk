USE master;
GO

IF DB_ID(N'AuctionDesk') IS NOT NULL
BEGIN
    ALTER DATABASE AuctionDesk SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AuctionDesk;
END
GO

CREATE DATABASE AuctionDesk;
GO

USE AuctionDesk;
GO

CREATE TABLE users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    username NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE categories (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    category_name NVARCHAR(100) NOT NULL UNIQUE
);
GO

CREATE TABLE lots (
    lot_id INT PRIMARY KEY IDENTITY(1,1),
    lot_name NVARCHAR(200) NOT NULL,
    description NVARCHAR(1000),
    starting_price DECIMAL(10,2) NOT NULL CHECK (starting_price > 0),
    current_price DECIMAL(10,2) NOT NULL,
    category_id INT NOT NULL,
    seller_id INT NOT NULL,
    duration_seconds INT NOT NULL CHECK (duration_seconds > 0),
    start_date DATETIME NOT NULL DEFAULT GETDATE(),
    end_date DATETIME NOT NULL,
    finished_date DATETIME NULL,
    status NVARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active','completed')),
    winner_id INT NULL,
    FOREIGN KEY (category_id) REFERENCES categories(category_id),
    FOREIGN KEY (seller_id) REFERENCES users(user_id),
    FOREIGN KEY (winner_id) REFERENCES users(user_id)
);
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

CREATE INDEX IX_lots_status ON lots(status);
CREATE INDEX IX_bids_lot ON bids(lot_id);
GO

CREATE VIEW vw_lot_report AS
SELECT
    c.category_name,
    COUNT(l.lot_id) AS total_lots,
    SUM(CASE WHEN l.status = 'completed' THEN 1 ELSE 0 END) AS completed_lots,
    ISNULL(SUM(w.final_price), 0) AS total_revenue
FROM categories c
LEFT JOIN lots l ON l.category_id = c.category_id
LEFT JOIN winners w ON w.lot_id = l.lot_id
GROUP BY c.category_name;
GO

PRINT N'Схема базы данных AuctionDesk создана.';
GO