using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class ActivateClientTests : EmployeeServiceTests
    {
        [Fact]
        public void ActivateClient_ValidInactiveClient_ShouldActivateAndLog()
        {
            // Arrange
            // Create an inactive client (assuming ClientStatus.Unactivated causes LocalValidator.IsActive to return false)
            Client client = CreateTestClient(id: 10L, status: ClientStatus.Unactive);

            _context.Clients.Add(client);
            _context.SaveChanges();

            string expectedOldStatus = ClientStatus.Unactive.ToString();
            string expectedNewStatus = ClientStatus.Active.ToString();

            // Act
            _employeeService.ActivateClient(client.Id);

            // Assert
            // Verify that the status inside the tracked entity object changed to Active
            Assert.Equal(ClientStatus.Active, client.Status);

            // Verify that the state was physically updated and persisted inside the database
            Client clientFromDb = _context.Clients.Find(client.Id);
            Assert.NotNull(clientFromDb);
            Assert.Equal(ClientStatus.Active, clientFromDb.Status);

            // Verify that the logger was invoked exactly once with matching parameters
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.CLIENT_ACTIVATED,
                nameof(Client),
                client.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Fact]
        public void ActivateClient_ClientAlreadyActive_ShouldReturnEarlyWithoutChangesOrLogs()
        {
            // Arrange
            // Create a client who is already Active (LocalValidator.IsActive returns true)
            Client activeClient = CreateTestClient(id: 10L, status: ClientStatus.Active);

            _context.Clients.Add(activeClient);
            _context.SaveChanges();

            // Act
            _employeeService.ActivateClient(activeClient.Id);

            // Assert
            // Verify that the status remains Active
            Assert.Equal(ClientStatus.Active, activeClient.Status);

            // Verify that the logger was NEVER called because of the early return statement
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }

        [Fact]
        public void ActivateClient_ClientDoesNotExist_ShouldThrowClientNotFoundException()
        {
            // Act & Assert (Database is empty, searching for ID 999 must trigger the exception)
            Assert.Throws<ClientNotFoundException>(() =>
                _employeeService.ActivateClient(clientId: 999L)
            );

            // Verify that execution was aborted before touching the logger infrastructure
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }
    }
}
