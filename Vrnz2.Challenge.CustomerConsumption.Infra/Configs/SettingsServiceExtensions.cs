using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Challenge.ServiceContracts.Settings;

namespace Vrnz2.Challenge.CustomerConsumption.Infra.Configs
{
    public static class SettingsServiceExtensions
    {
        public static IServiceCollection AddSettings(this IServiceCollection services, out AppSettings appSettings)
        {
            appSettings = services
                .AddSettings<AppSettings>()
                .AddSettings<ConnectionStringsSettings>("ConnectionStrings")
                .AddSettings<AwsSettings>("AwsSettings")
                .AddSettings<QueuesSettings>("QueuesSettings")
                .BuildServiceProvider()
                .GetService<IOptions<AppSettings>>().Value;

            return services;
        }
    }
}
