namespace KickBlastUltraLight.Models;

public class Pricing
{
    public int Id { get; set; } = 1;
    public decimal BeginnerWeeklyFee { get; set; } = 2500m;
    public decimal IntermediateWeeklyFee { get; set; } = 3500m;
    public decimal AdvancedWeeklyFee { get; set; } = 4500m;
    public decimal CoachingHourlyRate { get; set; } = 1500m;
    public decimal CompetitionFee { get; set; } = 2000m;
}
