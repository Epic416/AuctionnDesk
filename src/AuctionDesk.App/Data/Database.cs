using Microsoft.Data.SqlClient;

namespace AuctionDesk.App.Data;

public static class Database
{
    public static string ConnectionString { get; } =
        @"Server=DESKTOP-MTDRK23;Database=AuctionDesk;Integrated Security=true;TrustServerCertificate=true;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}