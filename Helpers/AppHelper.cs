namespace WebApi.Helpers;

// custom exception class for throwing application specific exceptions 
// that can be caught and handled within the application
public class AppHelper
{
    public static DateTime JakartaTime()
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var currentUtcTime = DateTime.UtcNow;
        var currentTimeInTimeZone = TimeZoneInfo.ConvertTimeFromUtc(currentUtcTime, timeZoneInfo);
        return currentTimeInTimeZone;
    }
}