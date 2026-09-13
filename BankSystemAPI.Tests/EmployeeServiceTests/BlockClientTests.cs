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
    public class BlockClientTests : EmployeeServiceTests
    {
        [Fact]
        public void BlockClient_ValidActiveClient_ShouldBlockAndLog()
        {
            // Arrange
            // Create an active client that we want to block
            Client client = CreateTestClient(id: 10L, status: ClientStatus.Active);

            _context.Clients.Add(client);
            _context.SaveChanges();

            string expectedOldStatus = ClientStatus.Active.ToString();
            string expectedNewStatus = ClientStatus.Blocked.ToString();

            // Act
            _employeeService.BlockClient(client.Id);

            // Assert
            // Verify that the status inside the tracked entity object changed to Blocked
            Assert.Equal(ClientStatus.Blocked, client.Status);

            // Verify that the state was physically updated and saved inside the InMemory database (now happens BEFORE logging)
            Client clientFromDb = _context.Clients.Find(client.Id);
            Assert.NotNull(clientFromDb);
            Assert.Equal(ClientStatus.Blocked, clientFromDb.Status);

            // Verify that the logger was called exactly once with proper status strings (executed AFTER SaveChanges)
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.CLIENT_BLOCKED,
                nameof(Client),
                client.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Fact]
        public void BlockClient_ClientAlreadyBlocked_ShouldReturnEarlyWithoutChangesOrLogs()
        {
            // Arrange
            // Create a client who is already Blocked to trigger the early return statement
            Client blockedClient = CreateTestClient(id: 10L, status: ClientStatus.Blocked);

            _context.Clients.Add(blockedClient);
            _context.SaveChanges();

            // Act
            _employeeService.BlockClient(blockedClient.Id);

            // Assert
            // Verify that the status securely remains Blocked
            Assert.Equal(ClientStatus.Blocked, blockedClient.Status);

            // Verify that the logger was NEVER called due to the early return condition
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
        public void BlockClient_ClientDoesNotExist_ShouldThrowClientNotFoundException()
        {
            // Act & Assert (Database is empty, searching for ID 999 must throw the expected exception)
            Assert.Throws<ClientNotFoundException>(() =>
                _employeeService.BlockClient(clientId: 999L)
            );

            // Verify that execution was aborted before touching the logging infrastructure
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
