using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Vrnz2.Challenge.CustomerConsumption.Infra.Configs
{
    public static class LogsServiceExtensions
    {
        public static ILogger Config()
            => Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(Serilog.Events.LogEventLevel.Verbose, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();

        public static IServiceCollection AddLogsServiceExtensions(this IServiceCollection services)
        {
            Config();

            return services.AddSingleton(_ => Log.Logger.ForContext<ILogger>());
        }
    }
}
