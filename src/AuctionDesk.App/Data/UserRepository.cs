using Microsoft.Data.SqlClient;
using AuctionDesk.App.Models;

namespace AuctionDesk.App.Data;

public class UserRepository
{
    public List<User> GetAll()
    {
        var users = new List<User>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT user_id, username FROM users", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1)
            });
        }
        return users;
    }
}