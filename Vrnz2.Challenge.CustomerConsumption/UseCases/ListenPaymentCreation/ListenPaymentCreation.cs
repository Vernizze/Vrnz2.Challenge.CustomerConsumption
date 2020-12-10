using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Challenge.ServiceContracts.Notifications;
using Vrnz2.Infra.CrossCutting.Extensions;
using Vrnz2.Infra.CrossCutting.Types;

namespace Vrnz2.Challenge.CustomerConsumption.UseCases.ListenPaymentCreation
{
    public class ListenPaymentCreation
    {
        public class Consumer
            : IConsumer<PaymentNotification.Created>
        {
            #region Variables

            private readonly IMediator _mediator;

            #endregion

            #region Constructors

            public Consumer(IMediator mediator)
                => _mediator = mediator;

            #endregion

            #region Methods

            public Task Consume(ConsumeContext<PaymentNotification.Created> context)
            {
                _mediator.Publish(context.Message);

                return Task.CompletedTask;
            }

            #endregion 
        }

        public class Handler
            : INotificationHandler<PaymentNotification.Created>
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

            public async Task Handle(PaymentNotification.Created notification, CancellationToken cancellationToken)
            {
                try
                {
                    await InsertCustomerConsumption(notification);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error at processing message! Cpf: {(notification.IsNotNull() ? notification.Cpf : "NULL MESSAGE!")} - Creation Date: {(notification.IsNotNull() ? notification.CreationDate.ToString("yyyy-MM-ddTHH:mm:ss") : "NULL MESSAGE!")} - Value: {(notification.IsNotNull() ? notification.Value.ToString() : "NULL MESSAGE!")} - Error: {ex.Message}";

                    _logger.Error(ex, errorMessage);

                    throw;
                }
            }

            public virtual async Task InsertCustomerConsumption(PaymentNotification.Created notification)
            {
                var cpf = new Cpf(notification.Cpf).Value;

                using (var mongo = new Data.MongoDB.MongoDB(_connectionStringsSettings.MongoDbChallenge, MONGODB_COLLECTION, MONGODB_DATABASE))
                {
                    var name = string.Empty;

                    var registers = await mongo.GetMany<Vrnz2.Challenge.CustomerConsumption.Shared.Entities.CustomerConsumption>(c => c.Cpf == cpf);

                    if (registers.HaveAny())
                        name = registers.First().CustomerName;

                    await mongo.Add(new Vrnz2.Challenge.CustomerConsumption.Shared.Entities.CustomerConsumption
                    {
                        Cpf = new Cpf(notification.Cpf).Value,
                        CustomerName = name,
                        PaymentDate = notification.CreationDate,
                        YearReference = notification.CreationDate.Year,
                        MonthReference = notification.CreationDate.Month,
                        Value = notification.Value
                    });
                }
            }

            #endregion
        }
    }
}
