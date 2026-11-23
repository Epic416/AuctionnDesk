namespace AuctionDesk.App.Models;

public class Auction
{
    public int AuctionId { get; set; }
    public string AuctionName { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "";
    public string CreatedByName { get; set; } = "";
    public int LotsCount { get; set; }
}