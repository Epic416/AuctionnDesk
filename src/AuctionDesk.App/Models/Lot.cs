using System.ComponentModel;

namespace AuctionDesk.App.Models;

public class Lot : INotifyPropertyChanged
{
    public int LotId { get; set; }
    public string LotName { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int SellerId { get; set; }
    public string SellerName { get; set; } = "";
    public DateTime EndDate { get; set; }
    public DateTime? FinishedDate { get; set; }
    public string Status { get; set; } = "active";
    public string WinnerName { get; set; } = "";

    public string TimeLeftText
    {
        get
        {
            if (Status != "active") return "завершён";
            var left = EndDate - DateTime.Now;
            if (left <= TimeSpan.Zero) return "00:00";
            return $"{(int)left.TotalMinutes:D2}:{left.Seconds:D2}";
        }
    }

    public string FinishedText => FinishedDate?.ToString("dd.MM.yyyy HH:mm:ss") ?? "";

    public void RefreshTime() =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeLeftText)));

    public event PropertyChangedEventHandler? PropertyChanged;
}