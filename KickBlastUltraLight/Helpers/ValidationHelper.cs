namespace KickBlastUltraLight.Helpers;

public static class ValidationHelper
{
    public static bool IsValidHours(int hours)
    {
        return hours >= 0 && hours <= 5;
    }

    public static bool IsValidCompetitions(int competitions)
    {
        return competitions >= 0;
    }
}
