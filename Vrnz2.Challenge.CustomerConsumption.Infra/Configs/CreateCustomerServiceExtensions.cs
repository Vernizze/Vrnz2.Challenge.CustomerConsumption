using GreenPipes;
using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Challenge.CustomerConsumption.UseCases.ListenCustomerCreation;
using Vrnz2.Challenge.CustomerConsumption.UseCases.ListenPaymentCreation;

namespace Vrnz2.Challenge.CustomerConsumption.Infra.Configs
{
    public static class CreateCustomerServiceExtensions
    {
        public static IServiceCollection AddConsumers(this IServiceCollection services, AppSettings appSettings, IMediator mediator)
        {
            var busControl = MassTransit.Bus.Factory.CreateUsingAmazonSqs(cfg =>
            {
                cfg.Host(appSettings.AwsSettings.Region, h =>
                {
                    h.AccessKey(appSettings.AwsSettings.AccessKey);
                    h.SecretKey(appSettings.AwsSettings.SecretKey);
                });

                cfg.ReceiveEndpoint(appSettings.QueuesSettings.CustomerCreatedQueueName, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.PrefetchCount = appSettings.QueuesSettings.PrefetchCount;
                    e.UseConcurrencyLimit(appSettings.QueuesSettings.ConcurrencyLimit);
                    e.UseMessageRetry(x => x.Interval(appSettings.QueuesSettings.RetryNumber, TimeSpan.FromSeconds(appSettings.QueuesSettings.TimeInSecondsToRetry)));
                    e.Consumer(() => new ListenCustomerCreation.Consumer(mediator));
                });

                cfg.ReceiveEndpoint(appSettings.QueuesSettings.PaymentCreatedQueueName, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.PrefetchCount = appSettings.QueuesSettings.PrefetchCount;
                    e.UseConcurrencyLimit(appSettings.QueuesSettings.ConcurrencyLimit);
                    e.UseMessageRetry(x => x.Interval(appSettings.QueuesSettings.RetryNumber, TimeSpan.FromSeconds(appSettings.QueuesSettings.TimeInSecondsToRetry)));
                    e.Consumer(() => new ListenPaymentCreation.Consumer(mediator));
                });
            });
            busControl.Start();

            return services;
        }
    }
}
