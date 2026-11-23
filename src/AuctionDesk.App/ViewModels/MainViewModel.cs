using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AuctionDesk.App.Data;
using AuctionDesk.App.Models;

namespace AuctionDesk.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly LotRepository _lotRepo = new();
    private readonly CategoryRepository _catRepo = new();
    private readonly UserRepository _userRepo = new();
    private readonly DispatcherTimer _timer;

    public ObservableCollection<Lot> Lots { get; } = new();
    public ObservableCollection<Bid> BidHistory { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<User> Users { get; } = new();

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
    }

    private int? _selectedCategoryId;
    public int? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set { _selectedCategoryId = value; OnPropertyChanged(nameof(SelectedCategoryId)); LoadLots(); }
    }

    private Lot? _selectedLot;
    public Lot? SelectedLot
    {
        get => _selectedLot;
        set { _selectedLot = value; OnPropertyChanged(nameof(SelectedLot)); LoadBidHistory(); }
    }

    private User? _currentUser;
    public User? CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged(nameof(CurrentUser));
            if (value != null) StatusMessage = $"Пользователь: {value.Username}";
        }
    }

    private string _statusMessage = "Нажмите «Подключиться»";
    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
    }

    public ICommand InitializeCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand PlaceBidCommand { get; }
    public ICommand CreateLotCommand { get; }
    public ICommand CompleteLotCommand { get; }

    public MainViewModel()
    {
        InitializeCommand = new RelayCommand(_ => Initialize());
        SearchCommand = new RelayCommand(_ => LoadLots());
        PlaceBidCommand = new RelayCommand(_ => PlaceBid(), _ => SelectedLot?.Status == "active" && CurrentUser != null);
        CreateLotCommand = new RelayCommand(_ => ShowCreateLotDialog());
        CompleteLotCommand = new RelayCommand(_ => CompleteLot(),
            _ => SelectedLot?.Status == "active" && CurrentUser != null && SelectedLot.SellerId == CurrentUser.UserId);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => Tick();
    }

    public void Initialize()
    {
        try
        {
            var userId = CurrentUser?.UserId;
            Users.Clear();
            foreach (var u in _userRepo.GetAll()) Users.Add(u);
            CurrentUser = Users.FirstOrDefault(u => u.UserId == userId) ?? Users.FirstOrDefault();

            Categories.Clear();
            Categories.Add(new Category { CategoryId = 0, CategoryName = "Все категории" });
            foreach (var c in _catRepo.GetAll()) Categories.Add(c);

            _lotRepo.CompleteExpired();
            LoadLots();
            _timer.Start();
            StatusMessage = $"Готово. Пользователь: {CurrentUser?.Username}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
            MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void LoadLots()
    {
        try
        {
            var lotId = SelectedLot?.LotId;
            Lots.Clear();
            int? filter = SelectedCategoryId is > 0 ? SelectedCategoryId : null;
            foreach (var lot in _lotRepo.GetAll(SearchText, filter)) Lots.Add(lot);
            SelectedLot = Lots.FirstOrDefault(l => l.LotId == lotId);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    private void Tick()
    {
        foreach (var lot in Lots) lot.RefreshTime();

        if (Lots.Any(l => l.Status == "active" && l.EndDate <= DateTime.Now))
        {
            int closed = _lotRepo.CompleteExpired();
            LoadLots();
            if (closed > 0) StatusMessage = $"Время истекло. Закрыто лотов: {closed}";
        }
    }

    public void PlaceBid()
    {
        if (SelectedLot == null || CurrentUser == null) return;

        var amount = SelectedLot.CurrentPrice + 500;
        var (success, message) = _lotRepo.PlaceBid(SelectedLot.LotId, CurrentUser.UserId, amount);

        MessageBox.Show($"{message}\n\nУчастник: {CurrentUser.Username}\nЛот: {SelectedLot.LotName}",
            "Ставка", MessageBoxButton.OK,
            success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (success) LoadLots();
    }

    private void LoadBidHistory()
    {
        BidHistory.Clear();
        if (SelectedLot == null) return;
        foreach (var bid in _lotRepo.GetBidHistory(SelectedLot.LotId)) BidHistory.Add(bid);
    }

    public void CompleteLot()
    {
        if (SelectedLot == null || CurrentUser == null) return;

        var answer = MessageBox.Show($"Завершить лот \"{SelectedLot.LotName}\"?",
            "Завершение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes) return;

        var (success, message) = _lotRepo.CompleteLot(SelectedLot.LotId, CurrentUser.UserId);
        MessageBox.Show(message, "Результат", MessageBoxButton.OK,
            success ? MessageBoxImage.Information : MessageBoxImage.Warning);

        if (success) LoadLots();
    }

    public void ShowCreateLotDialog()
    {
        if (CurrentUser == null)
        {
            MessageBox.Show("Сначала подключитесь", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var categories = new ObservableCollection<Category>(Categories.Where(c => c.CategoryId > 0));
        var dialog = new Views.CreateLotWindow(categories, CurrentUser.UserId);
        dialog.Owner = Application.Current.MainWindow;
        if (dialog.ShowDialog() == true) LoadLots();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}