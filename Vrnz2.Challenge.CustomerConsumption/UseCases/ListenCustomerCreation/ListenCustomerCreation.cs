using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Challenge.ServiceContracts.Notifications;
using Vrnz2.Infra.CrossCutting.Extensions;
using Vrnz2.Infra.CrossCutting.Types;

namespace Vrnz2.Challenge.CustomerConsumption.UseCases.ListenCustomerCreation
{
    public class ListenCustomerCreation         
    {
        public class Consumer
            : IConsumer<CustomerNotification.Created>
        {
            #region Variables

            private readonly IMediator _mediator;

            #endregion

            #region Constructors

            public Consumer(IMediator mediator)
                => _mediator = mediator;

            #endregion

            #region Methods

            public Task Consume(ConsumeContext<CustomerNotification.Created> context)
            {
                _mediator.Publish(context.Message);

                return Task.CompletedTask;
            }

            #endregion 
        }

        public class Handler
            : INotificationHandler<CustomerNotification.Created>
        {
            #region Variables

            private const string MONGODB_COLLECTION = "CustomerPayments";
            private const string MONGODB_DATABASE = "Challenge";

            #endregion

            #region Variables

            private readonly ConnectionStringsSettings _connectionStringsSettings;
            private readonly QueuesSettings _queuesSettings;
            private readonly ILogger _logger;

            #endregion

            #region Constructors

            public Handler(IOptions<ConnectionStringsSettings> connectionStringsOptions, IOptions<QueuesSettings> queuesOptionsSettings, ILogger logger)
            {
                _connectionStringsSettings = connectionStringsOptions.Value;
                _queuesSettings = queuesOptionsSettings.Value;
                _logger = logger;
            }

            #endregion

            #region Methods

            public async Task Handle(CustomerNotification.Created notification, CancellationToken cancellationToken)
            {
                try
                {
                    var cpf = new Cpf(notification.Cpf).Value;

                    using (var mongo = new Data.MongoDB.MongoDB(_connectionStringsSettings.MongoDbChallenge, MONGODB_COLLECTION, MONGODB_DATABASE))
                    {
                        var register = await mongo.GetMany<Vrnz2.Challenge.CustomerConsumption.Shared.Entities.CustomerConsumption>(c => c.Cpf == cpf);

                        if (register.HaveAny())
                        {
                            await mongo.Update<Vrnz2.Challenge.CustomerConsumption.Shared.Entities.CustomerConsumption, string>((c => c.Cpf == cpf), (c => c.CustomerName), notification.Name);
                        }
                        else
                        {
                            await mongo.Add(new Vrnz2.Challenge.CustomerConsumption.Shared.Entities.CustomerConsumption
                            {
                                Cpf = cpf,
                                CustomerName = notification.Name,
                                PaymentDate = notification.CreationDate,
                                YearReference = notification.CreationDate.Year,
                                MonthReference = notification.CreationDate.Month,
                                Value = 0
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error at processing message! Cpf: {(notification.IsNotNull() ? notification.Cpf : "NULL MESSAGE!")} - Creation Date: {(notification.IsNotNull() ? notification.CreationDate.ToString("yyyy-MM-ddTHH:mm:ss") : "NULL MESSAGE!")} - Value: {(notification.IsNotNull() ? notification.Name.ToString() : "NULL MESSAGE!")} - Error: {ex.Message}";

                    _logger.Error(ex, errorMessage);

                    throw;
                }
            }

            #endregion
        }
    }
}
