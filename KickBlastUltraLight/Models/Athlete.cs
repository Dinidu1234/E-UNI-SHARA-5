namespace KickBlastUltraLight.Models;

public class Athlete
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Plan { get; set; } = "Beginner";
    public decimal WeightKg { get; set; }
    public decimal TargetWeightKg { get; set; }
    public string Notes { get; set; } = string.Empty;
}
