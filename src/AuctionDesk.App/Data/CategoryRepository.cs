using Microsoft.Data.SqlClient;
using AuctionDesk.App.Models;

namespace AuctionDesk.App.Data;

public class CategoryRepository
{
    public List<Category> GetAll()
    {
        var categories = new List<Category>();
        using var conn = Database.GetConnection();
        conn.Open();
        using var cmd = new SqlCommand("SELECT category_id, category_name FROM categories", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                CategoryId = reader.GetInt32(0),
                CategoryName = reader.GetString(1)
            });
        }
        return categories;
    }
}