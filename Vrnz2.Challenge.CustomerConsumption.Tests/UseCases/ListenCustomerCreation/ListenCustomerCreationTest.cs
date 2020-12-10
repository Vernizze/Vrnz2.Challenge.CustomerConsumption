using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Serilog;
using Vrnz2.Challenge.CustomerConsumption.Shared.Settings;
using Vrnz2.Infra.CrossCutting.Types;
using Xunit;
using Get = Vrnz2.Challenge.CustomerConsumption.UseCases.ListenCustomerCreation;
using Vrnz2.Challenge.ServiceContracts.Notifications;

namespace Vrnz2.Challenge.CustomerConsumption.Tests.UseCases.ListenCustomerCreation
{
    public class ListenCustomerCreationTest
    {
        private IOptions<ConnectionStringsSettings> _connectionStringsOptions;
        private IOptions<QueuesSettings> _queuesOptionsSettings;
        private ILogger _logger;

        public ListenCustomerCreationTest()
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

        private ListenCustomerCreationMock GetInstance()
            => new ListenCustomerCreationMock(_connectionStringsOptions, _queuesOptionsSettings, _logger);

        [Fact]
        public async Task ListenCustomerCreation_Handler_Test()
        {
            //Arrange            
            Cpf cpf = "434.443.474-99";
            var name = "Pedro de Oliveira";
            var creationDate = DateTime.UtcNow;

            var request = new CustomerNotification.Created
            {
                Cpf = cpf.Value,
                Name = name,
                CreationDate = creationDate
            };

            var service = GetInstance();

            //Act
            await service.Handle(request, new System.Threading.CancellationToken());

            //Assert
            Received.InOrder(() =>
            {
                service.UpsertCustomerConsumption(Arg.Any<CustomerNotification.Created>());
            });
        }
    }

    public class ListenCustomerCreationMock 
        : Get.ListenCustomerCreation.Handler
    {
        public ListenCustomerCreationMock(IOptions<ConnectionStringsSettings> connectionStringsOptions, IOptions<QueuesSettings> queuesOptionsSettings, ILogger logger) 
            : base(connectionStringsOptions, queuesOptionsSettings, logger)
        {
        }

        public override Task UpsertCustomerConsumption(CustomerNotification.Created notification)
            => Task.CompletedTask;
    }
}
