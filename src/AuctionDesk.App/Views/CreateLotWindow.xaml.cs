using System.Collections.ObjectModel;
using System.Windows;
using AuctionDesk.App.Data;
using AuctionDesk.App.Models;

namespace AuctionDesk.App.Views;

public partial class CreateLotWindow : Window
{
    private readonly int _sellerId;

    public CreateLotWindow(ObservableCollection<Category> categories, int sellerId)
    {
        InitializeComponent();
        _sellerId = sellerId;
        CategoryComboBox.ItemsSource = categories;
        if (categories.Count > 0) CategoryComboBox.SelectedIndex = 0;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LotNameTextBox.Text))
        {
            MessageBox.Show("Введите название лота", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
        {
            MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (CategoryComboBox.SelectedValue == null)
        {
            MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(MinutesTextBox.Text, out int minutes) || minutes < 0 ||
            !int.TryParse(SecondsTextBox.Text, out int seconds) || seconds < 0 || seconds > 59)
        {
            MessageBox.Show("Минуты — целое число от 0, секунды — от 0 до 59",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int total = minutes * 60 + seconds;
        if (total <= 0)
        {
            MessageBox.Show("Длительность должна быть больше нуля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var lot = new Lot
            {
                LotName = LotNameTextBox.Text,
                Description = DescriptionTextBox.Text,
                StartingPrice = price,
                CategoryId = (int)CategoryComboBox.SelectedValue
            };

            new LotRepository().Create(lot, _sellerId, total);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}