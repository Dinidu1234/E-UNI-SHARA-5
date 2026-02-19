using System.Windows;
using System.Windows.Controls;
using KickBlastUltraLight.Data;
using KickBlastUltraLight.Helpers;
using KickBlastUltraLight.Models;

namespace KickBlastUltraLight.Views;

public partial class HistoryView : UserControl
{
    public event Action<string, UIElement>? RequestOpenDrawer;

    public HistoryView()
    {
        InitializeComponent();
    }

    public void Reload()
    {
        var athletes = Db.GetAthletes();
        athletes.Insert(0, new Athlete { Id = 0, FullName = "All Athletes" });
        AthleteFilterComboBox.ItemsSource = athletes;
        AthleteFilterComboBox.SelectedIndex = 0;

        MonthTextBox.Text = DateTime.Now.Month.ToString();
        YearTextBox.Text = DateTime.Now.Year.ToString();

        RefreshHistory();
    }

    private void RefreshHistory()
    {
        try
        {
            var athleteId = (AthleteFilterComboBox.SelectedItem as Athlete)?.Id;
            int? month = int.TryParse(MonthTextBox.Text, out var m) ? m : null;
            int? year = int.TryParse(YearTextBox.Text, out var y) ? y : null;
            HistoryDataGrid.ItemsSource = Db.GetHistory(athleteId, month, year);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load history: " + ex.Message);
        }
    }

    private void OpenDetails()
    {
        if (HistoryDataGrid.SelectedItem is not MonthlyCalculation selected)
        {
            MessageBox.Show("Select a history row first.");
            return;
        }

        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = "Athlete: " + selected.AthleteName, Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Period: " + selected.Year + "-" + selected.Month, Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Training: " + CurrencyHelper.FormatLkr(selected.TrainingCost), Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Coaching: " + CurrencyHelper.FormatLkr(selected.CoachingCost), Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Competition: " + CurrencyHelper.FormatLkr(selected.CompetitionCost), Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Total: " + CurrencyHelper.FormatLkr(selected.TotalCost), FontWeight = FontWeights.Bold, Margin = new Thickness(0, 4, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Weight Status: " + selected.WeightStatus, Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Payment Due: " + selected.PaymentDueDate, Margin = new Thickness(0, 2, 0, 6) });
        panel.Children.Add(new TextBlock { Text = "Saved At: " + selected.CreatedAt, Margin = new Thickness(0, 2, 0, 6) });

        RequestOpenDrawer?.Invoke("Calculation Details", panel);
    }

    private void FiltersChanged(object sender, RoutedEventArgs e)
    {
        RefreshHistory();
    }

    private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        OpenDetails();
    }
}
