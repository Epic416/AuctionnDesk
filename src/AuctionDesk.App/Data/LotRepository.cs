using Microsoft.Data.SqlClient;
using AuctionDesk.App.Models;

namespace AuctionDesk.App.Data;

public class LotRepository
{
    private static Lot Map(SqlDataReader r) => new()
    {
        LotId = r.GetInt32(0),
        LotName = r.GetString(1),
        Description = r.IsDBNull(2) ? "" : r.GetString(2),
        StartingPrice = r.GetDecimal(3),
        CurrentPrice = r.GetDecimal(4),
        CategoryId = r.GetInt32(5),
        CategoryName = r.GetString(6),
        SellerId = r.GetInt32(7),
        SellerName = r.GetString(8),
        EndDate = r.GetDateTime(9),
        FinishedDate = r.IsDBNull(10) ? null : r.GetDateTime(10),
        Status = r.GetString(11),
        WinnerName = r.IsDBNull(12) ? "" : r.GetString(12)
    };

    public List<Lot> GetAll(string? search = null, int? categoryId = null)
    {
        var lots = new List<Lot>();
        using var conn = Database.GetConnection();
        conn.Open();

        var sql = @"
            SELECT l.lot_id, l.lot_name, l.description, l.starting_price, l.current_price,
                   l.category_id, c.category_name, l.seller_id, u.username,
                   l.end_date, l.finished_date, l.status, wu.username
            FROM lots l
            JOIN categories c ON l.category_id = c.category_id
            JOIN users u ON l.seller_id = u.user_id
            LEFT JOIN users wu ON l.winner_id = wu.user_id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(search)) sql += " AND l.lot_name LIKE @search";
        if (categoryId.HasValue) sql += " AND l.category_id = @cat";
        sql += " ORDER BY l.status, l.end_date";

        using var cmd = new SqlCommand(sql, conn);
        if (!string.IsNullOrEmpty(search)) cmd.Parameters.AddWithValue("@search", $"%{search}%");
        if (categoryId.HasValue) cmd.Parameters.AddWithValue("@cat", categoryId.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read()) lots.Add(Map(reader));
        return lots;
    }

    public void Create(Lot lot, int sellerId, int durationSeconds)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        var sql = @"
            INSERT INTO lots (lot_name, description, starting_price, current_price,
                              category_id, seller_id, duration_seconds, end_date)
            VALUES (@name, @desc, @price, @price, @cat, @seller, @sec,
                    DATEADD(SECOND, @sec, GETDATE()))";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", lot.LotName);
        cmd.Parameters.AddWithValue("@desc", (object?)lot.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@price", lot.StartingPrice);
        cmd.Parameters.AddWithValue("@cat", lot.CategoryId);
        cmd.Parameters.AddWithValue("@seller", sellerId);
        cmd.Parameters.AddWithValue("@sec", durationSeconds);
        cmd.ExecuteNonQuery();
    }

    public (bool Success, string Message) PlaceBid(int lotId, int userId, decimal amount)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            decimal currentPrice;
            string status;
            int sellerId;
            DateTime endDate;

            var selectSql = @"SELECT current_price, status, seller_id, end_date
                              FROM lots WITH (UPDLOCK) WHERE lot_id = @id";
            using (var cmd = new SqlCommand(selectSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", lotId);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) { tx.Rollback(); return (false, "Лот не найден"); }
                currentPrice = r.GetDecimal(0);
                status = r.GetString(1);
                sellerId = r.GetInt32(2);
                endDate = r.GetDateTime(3);
            }

            if (status != "active") { tx.Rollback(); return (false, "Аукцион завершён, ставки запрещены"); }
            if (DateTime.Now > endDate) { tx.Rollback(); return (false, "Время аукциона истекло"); }
            if (sellerId == userId) { tx.Rollback(); return (false, "Продавец не может ставить на свой лот"); }
            if (amount <= currentPrice) { tx.Rollback(); return (false, $"Ставка должна быть выше {currentPrice:N2} руб."); }

            using (var cmd = new SqlCommand(
                "INSERT INTO bids (lot_id, user_id, bid_amount) VALUES (@id, @user, @sum)", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", lotId);
                cmd.Parameters.AddWithValue("@user", userId);
                cmd.Parameters.AddWithValue("@sum", amount);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand(
                "UPDATE lots SET current_price = @sum WHERE lot_id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@sum", amount);
                cmd.Parameters.AddWithValue("@id", lotId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return (true, $"Ставка {amount:N2} руб. принята!");
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    public (bool Success, string Message) CompleteLot(int lotId, int userId)
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            string status;
            int sellerId;
            using (var cmd = new SqlCommand("SELECT status, seller_id FROM lots WHERE lot_id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", lotId);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) { tx.Rollback(); return (false, "Лот не найден"); }
                status = r.GetString(0);
                sellerId = r.GetInt32(1);
            }

            if (sellerId != userId) { tx.Rollback(); return (false, "Завершить может только продавец лота"); }
            if (status != "active") { tx.Rollback(); return (false, "Лот уже завершён"); }

            int? winnerId = null;
            decimal price = 0;
            string winnerName = "";

            using (var cmd = new SqlCommand(@"
                SELECT TOP 1 b.user_id, b.bid_amount, u.username
                FROM bids b JOIN users u ON b.user_id = u.user_id
                WHERE b.lot_id = @id ORDER BY b.bid_amount DESC", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", lotId);
                using var r = cmd.ExecuteReader();
                if (r.Read()) { winnerId = r.GetInt32(0); price = r.GetDecimal(1); winnerName = r.GetString(2); }
            }

            using (var cmd = new SqlCommand(@"
                UPDATE lots SET status = 'completed', winner_id = @winner, finished_date = GETDATE()
                WHERE lot_id = @id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@winner", (object?)winnerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", lotId);
                cmd.ExecuteNonQuery();
            }

            if (winnerId.HasValue)
                using (var cmd = new SqlCommand(
                    "INSERT INTO winners (lot_id, user_id, final_price) VALUES (@id, @user, @sum)", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@id", lotId);
                    cmd.Parameters.AddWithValue("@user", winnerId.Value);
                    cmd.Parameters.AddWithValue("@sum", price);
                    cmd.ExecuteNonQuery();
                }

            tx.Commit();
            return winnerId.HasValue
                ? (true, $"Лот завершён!\nПобедитель: {winnerName}\nЦена: {price:N2} ₽")
                : (true, "Лот завершён. Ставок не было, победитель не определён.");
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return (false, $"Ошибка: {ex.Message}");
        }
    }

    public int CompleteExpired()
    {
        using var conn = Database.GetConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            int count;
            using (var cmd = new SqlCommand(@"
                UPDATE lots
                SET status = 'completed',
                    finished_date = GETDATE(),
                    winner_id = (SELECT TOP 1 b.user_id FROM bids b
                                 WHERE b.lot_id = lots.lot_id ORDER BY b.bid_amount DESC)
                WHERE status = 'active' AND end_date <= GETDATE()", conn, tx))
                count = cmd.ExecuteNonQuery();

            using (var cmd = new SqlCommand(@"
                INSERT INTO winners (lot_id, user_id, final_price)
                SELECT l.lot_id, l.winner_id, l.current_price
                FROM lots l
                WHERE l.status = 'completed' AND l.winner_id IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM winners w WHERE w.lot_id = l.lot_id)", conn, tx))
                cmd.ExecuteNonQuery();

            tx.Commit();
            return count;
        }
        catch
        {
            tx.Rollback();
            return 0;
        }
    }

    public List<Bid> GetBidHistory(int lotId)
    {
        var bids = new List<Bid>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(@"
            SELECT b.bid_id, b.lot_id, b.user_id, u.username, b.bid_amount, b.bid_time
            FROM bids b JOIN users u ON b.user_id = u.user_id
            WHERE b.lot_id = @id ORDER BY b.bid_amount DESC", conn);
        cmd.Parameters.AddWithValue("@id", lotId);

        using var r = cmd.ExecuteReader();
        while (r.Read())
            bids.Add(new Bid
            {
                BidId = r.GetInt32(0),
                LotId = r.GetInt32(1),
                UserId = r.GetInt32(2),
                Username = r.GetString(3),
                BidAmount = r.GetDecimal(4),
                BidTime = r.GetDateTime(5)
            });
        return bids;
    }
}