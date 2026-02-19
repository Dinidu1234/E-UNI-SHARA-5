using System.Globalization;

namespace KickBlastUltraLight.Helpers;

public static class CurrencyHelper
{
    public static string FormatLkr(decimal value)
    {
        return "LKR " + value.ToString("N2", CultureInfo.InvariantCulture);
    }
}
