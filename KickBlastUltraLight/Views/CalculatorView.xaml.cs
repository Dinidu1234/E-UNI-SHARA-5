using System.Windows;
using System.Windows.Controls;
using KickBlastUltraLight.Data;
using KickBlastUltraLight.Helpers;
using KickBlastUltraLight.Models;

namespace KickBlastUltraLight.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
    }

    public void Reload()
    {
        AthleteComboBox.ItemsSource = Db.GetAthletes();
        if (AthleteComboBox.Items.Count > 0)
        {
            AthleteComboBox.SelectedIndex = 0;
        }
    }

    public void RunCalculation()
    {
        CalculateAndSave();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculateAndSave();
    }

    private void CalculateAndSave()
    {
        try
        {
            if (AthleteComboBox.SelectedItem is not Athlete athlete)
            {
                MessageBox.Show("Please select an athlete.");
                return;
            }

            if (!int.TryParse(CompetitionsTextBox.Text, out var competitions) || !ValidationHelper.IsValidCompetitions(competitions))
            {
                MessageBox.Show("Competitions must be >= 0.");
                return;
            }

            if (!int.TryParse(HoursTextBox.Text, out var hours) || !ValidationHelper.IsValidHours(hours))
            {
                MessageBox.Show("Coaching hours must be between 0 and 5.");
                return;
            }

            var pricing = Db.GetPricing();
            decimal weeklyFee = athlete.Plan switch
            {
                "Intermediate" => pricing.IntermediateWeeklyFee,
                "Advanced" => pricing.AdvancedWeeklyFee,
                _ => pricing.BeginnerWeeklyFee
            };

            var training = weeklyFee * 4;
            var coaching = hours * 4 * pricing.CoachingHourlyRate;
            var competitionCount = athlete.Plan == "Beginner" ? 0 : competitions;
            var competition = competitionCount * pricing.CompetitionFee;
            var total = training + coaching + competition;

            var weightStatus = athlete.WeightKg > athlete.TargetWeightKg ? "Over Target" :
                               athlete.WeightKg < athlete.TargetWeightKg ? "Under Target" : "On Target";
            var dueDate = DateHelper.GetSecondSaturdayOfCurrentMonth();

            TrainingCostText.Text = "Training: " + CurrencyHelper.FormatLkr(training);
            CoachingCostText.Text = "Coaching: " + CurrencyHelper.FormatLkr(coaching);
            CompetitionCostText.Text = "Competition: " + CurrencyHelper.FormatLkr(competition);
            TotalCostText.Text = "Total: " + CurrencyHelper.FormatLkr(total);
            WeightStatusText.Text = "Weight Status: " + weightStatus;
            DueDateText.Text = "Second Saturday: " + dueDate.ToString("yyyy-MM-dd");

            var model = new MonthlyCalculation
            {
                AthleteId = athlete.Id,
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                Competitions = competitionCount,
                CoachingHours = hours,
                TrainingCost = training,
                CoachingCost = coaching,
                CompetitionCost = competition,
                TotalCost = total,
                WeightStatus = weightStatus,
                PaymentDueDate = dueDate.ToString("yyyy-MM-dd")
            };

            Db.SaveCalculation(model);
            ((MainWindow)Application.Current.MainWindow).ShowToast("Calculation saved");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Calculation failed: " + ex.Message);
        }
    }
}
