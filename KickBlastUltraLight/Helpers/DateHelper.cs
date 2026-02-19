namespace KickBlastUltraLight.Helpers;

public static class DateHelper
{
    public static DateTime GetSecondSaturdayOfCurrentMonth()
    {
        var now = DateTime.Now;
        var firstDay = new DateTime(now.Year, now.Month, 1);
        var dayOffset = ((int)DayOfWeek.Saturday - (int)firstDay.DayOfWeek + 7) % 7;
        var firstSaturday = firstDay.AddDays(dayOffset);
        return firstSaturday.AddDays(7);
    }
}
