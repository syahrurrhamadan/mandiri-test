using Microsoft.EntityFrameworkCore;
using Serilog;

public class DBHelper
{
    public static void Postgres(IConfigurationRoot configuration, DbContextOptionsBuilder options)
    {
        options.UseNpgsql(configuration.GetConnectionString("DbConnection"));

        // options.UseNpgsql(configuration.GetConnectionString("DbConnection"))
        //     .EnableSensitiveDataLogging() // Optional: Logs parameter values
        //     .LogTo(message => Log.Information(message),
        //            Microsoft.Extensions.Logging.LogLevel.Information); // Logs SQL queries
    }

    public static void Mysql(IConfigurationRoot configuration, DbContextOptionsBuilder options)
    {
        var conectionString = configuration.GetConnectionString("DbConnection");
        options.UseMySql(conectionString, ServerVersion.AutoDetect(conectionString));
    }

    public static void SqlServer(IConfigurationRoot configuration, DbContextOptionsBuilder options)
    {
        options.UseSqlServer(configuration.GetConnectionString("DbConnection"));

        // options.UseSqlServer(configuration.GetConnectionString("DbConnection"))
        //     .EnableSensitiveDataLogging() // Optional: Logs parameter values
        //     .LogTo(message => Log.Information(message),
        //            Microsoft.Extensions.Logging.LogLevel.Information); // Logs SQL queries
    }
}