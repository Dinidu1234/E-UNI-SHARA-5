using System.Windows;
using System.Windows.Controls;
using KickBlastUltraLight.Data;
using KickBlastUltraLight.Models;

namespace KickBlastUltraLight.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    public void Reload()
    {
        try
        {
            var pricing = Db.GetPricing();
            BeginnerFeeTextBox.Text = pricing.BeginnerWeeklyFee.ToString();
            IntermediateFeeTextBox.Text = pricing.IntermediateWeeklyFee.ToString();
            AdvancedFeeTextBox.Text = pricing.AdvancedWeeklyFee.ToString();
            CoachingRateTextBox.Text = pricing.CoachingHourlyRate.ToString();
            CompetitionFeeTextBox.Text = pricing.CompetitionFee.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load pricing: " + ex.Message);
        }
    }

    public void Save()
    {
        SavePricing();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SavePricing();
    }

    private void SavePricing()
    {
        try
        {
            var pricing = new Pricing
            {
                Id = 1,
                BeginnerWeeklyFee = decimal.TryParse(BeginnerFeeTextBox.Text, out var b) ? b : 0,
                IntermediateWeeklyFee = decimal.TryParse(IntermediateFeeTextBox.Text, out var i) ? i : 0,
                AdvancedWeeklyFee = decimal.TryParse(AdvancedFeeTextBox.Text, out var a) ? a : 0,
                CoachingHourlyRate = decimal.TryParse(CoachingRateTextBox.Text, out var c) ? c : 0,
                CompetitionFee = decimal.TryParse(CompetitionFeeTextBox.Text, out var f) ? f : 0
            };

            Db.SavePricing(pricing);
            Reload();
            ((MainWindow)Application.Current.MainWindow).ShowToast("Pricing saved");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to save pricing: " + ex.Message);
        }
    }
}
