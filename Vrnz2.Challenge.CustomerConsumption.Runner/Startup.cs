using AutoMapper;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Vrnz2.Challenge.CustomerConsumption.Infra.Configs;
using Vrnz2.Challenge.CustomerConsumption.Infra.Factories;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Challenge.CustomerConsumption.UseCases.ListenCustomerCreation;

namespace Vrnz2.Challenge.CustomerConsumption
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices()
            =>  new ServiceCollection()
                    .AddSettings(out AppSettings appSettings)
                    .AddLogsServiceExtensions()
                    .AddAutoMapper(AssembliesFactory.GetAssemblies())
                    .AddMediatR(AssembliesFactory.GetAssemblies<ListenCustomerCreation>())
                    .AddIServiceColletion()
                    .AddIServiceColletion()
                    .MakeServiceProvider()
                    .AddConsumers(appSettings, GetService<IMediator>());

        private static ServiceProvider GetServiceProvider { get; set; }

        public static T GetService<T>()
            => GetServiceProvider.GetService<T>();

        private static IServiceCollection MakeServiceProvider(this IServiceCollection services)
        {
            GetServiceProvider = services.BuildServiceProvider();

            return services;
        }
    }
}
