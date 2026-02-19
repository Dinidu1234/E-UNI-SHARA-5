using System.Windows.Controls;
using KickBlastUltraLight.Data;
using KickBlastUltraLight.Helpers;

namespace KickBlastUltraLight.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    public void Reload()
    {
        try
        {
            var stats = Db.GetDashboardStats();
            AthletesCountText.Text = stats.Athletes.ToString();
            CalcCountText.Text = stats.Calculations.ToString();
            RevenueText.Text = CurrencyHelper.FormatLkr(stats.Revenue);
        }
        catch
        {
            AthletesCountText.Text = "0";
            CalcCountText.Text = "0";
            RevenueText.Text = "LKR 0.00";
        }
    }
}
