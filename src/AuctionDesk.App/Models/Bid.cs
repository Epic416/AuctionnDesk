namespace AuctionDesk.App.Models;

public class Bid
{
    public int BidId { get; set; }
    public int LotId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public decimal BidAmount { get; set; }
    public DateTime BidTime { get; set; }
}