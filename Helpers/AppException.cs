namespace WebApi.Helpers;

using System.Globalization;

// custom exception class for throwing application specific exceptions 
// that can be caught and handled within the application
public class AppException : Exception
{
    public static DateTime TimeUTC = DateTime.UtcNow;
    public static TimeZoneInfo JakartaTZ = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    public static DateTime JakartaTime = TimeZoneInfo.ConvertTimeFromUtc(TimeUTC, JakartaTZ);

    public AppException() : base() { }

    public AppException(string message) : base(message) { }

    public AppException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}