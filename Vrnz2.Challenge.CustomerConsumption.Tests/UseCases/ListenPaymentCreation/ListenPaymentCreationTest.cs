using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Serilog;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Infra.CrossCutting.Types;
using Xunit;
using Get = Vrnz2.Challenge.CustomerConsumption.UseCases.ListenPaymentCreation;
using Vrnz2.Challenge.ServiceContracts.Notifications;

namespace Vrnz2.Challenge.CustomerConsumption.Tests.UseCases.ListenPaymentCreation
{
    public class ListenPaymentCreationTest
    {
        private IOptions<ConnectionStringsSettings> _connectionStringsOptions;
        private IOptions<QueuesSettings> _queuesOptionsSettings;
        private ILogger _logger;

        public ListenPaymentCreationTest()
        {
            _connectionStringsOptions = Options.Create(new ConnectionStringsSettings
            {
                MongoDbChallenge = string.Empty
            });

            _queuesOptionsSettings = Options.Create(new QueuesSettings
            {
                CustomerCreatedQueueName = "fila-teste"
            });

            _logger = Substitute.For<ILogger>();
        }

        private ListenPaymentCreationMock GetInstance()
            => new ListenPaymentCreationMock(_connectionStringsOptions, _queuesOptionsSettings, _logger);

        [Fact]
        public async Task ListenPaymentCreation_Handler_Test()
        {
            //Arrange            
            Cpf cpf = "434.443.474-99";
            var value = 20;
            var creationDate = DateTime.UtcNow;

            var request = new PaymentNotification.Created
            {
                Cpf = cpf.Value,
                Value = value,
                CreationDate = creationDate
            };

            var service = GetInstance();

            //Act
            await service.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Received.InOrder(() =>
            {
                service.InsertCustomerConsumption(Arg.Any<PaymentNotification.Created>());
            });
        }
    }

    public class ListenPaymentCreationMock
        : Get.ListenPaymentCreation.Handler
    {
        public ListenPaymentCreationMock(IOptions<ConnectionStringsSettings> connectionStringsOptions, IOptions<QueuesSettings> queuesOptionsSettings, ILogger logger)
            : base(connectionStringsOptions, queuesOptionsSettings, logger)
        {
        }

        public override Task InsertCustomerConsumption(PaymentNotification.Created notification)
            => Task.CompletedTask;
    }
}
