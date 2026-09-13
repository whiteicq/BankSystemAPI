using BusinessLogicLayer.Exceptions.Bank;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.DepositServiceTests
{
    public class RequestDepositTests : DepositServiceTests
    {
        [Fact]
        public void RequestDeposit_ValidData_ShouldCreateDepositSuccessfully()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient();

            _context.Banks.Add(bank);
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            Deposit result = _depositService.RequestDeposit(client.UserId, bank.Id, sumOfDeposit: 5000m, term: 24, interest: 8.5m);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5000m, result.DepositAmount);
            Assert.Equal(24, result.DepositTerm);
            Assert.Equal(8.5m, result.DepositInterest);
            Assert.Equal(bank.Id, result.BankId);
            Assert.Equal(client.Id, result.ClientId);

            // Verify that the entity was physically written into the InMemory Database
            Assert.NotNull(_context.Deposits.Find(result.Id));

            // Verify the logger service call
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.DEPOSIT_REQUESTED,
                nameof(Deposit),
                result.Id,
                null,
                result.Status.ToString()
                ),
                Times.Once);
        }

        [Fact]
        public void RequestDeposit_BankNotExists_ShouldThrowBankNotFoundException()
        {
            // Arrange
            Client client = CreateTestClient();
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act & Assert (DB contains no banks, so ID 999 will trigger the exception)
            Assert.Throws<BankNotFoundException>(() =>
                _depositService.RequestDeposit(client.UserId, bankId: 999L, sumOfDeposit: 1000m, term: 12, interest: 7m)
            );

            // Verify that logger was never called due to the exception
            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(0, 12, 7)]    // Deposit amount is exactly 0
        [InlineData(-100, 12, 7)] // Deposit amount is negative
        [InlineData(1000, 0, 7)]  // Deposit term is exactly 0
        [InlineData(1000, -6, 7)] // Deposit term is negative
        [InlineData(1000, 12, 5)] // Interest is exactly 5 (triggers error due to interest <= 5)
        [InlineData(1000, 12, 3)] // Interest is less than 5
        [InlineData(1000, 12, 11)]// Interest is exactly 11 (triggers error due to interest >= 11)
        [InlineData(1000, 12, 15)]// Interest is greater than 11
        public void RequestDeposit_InvalidArguments_ShouldThrowArgumentOutOfRangeException(decimal sumOfDeposit, int term, decimal interest)
        {
            // Arrange
            Bank bank = CreateTestBank();
            _context.Banks.Add(bank);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _depositService.RequestDeposit(userId: 1L, bank.Id, sumOfDeposit, term, interest)
            );

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void RequestDeposit_ClientNotExists_ShouldThrowClientNotFoundException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            _context.Banks.Add(bank);
            _context.SaveChanges();

            // Act & Assert (Bank exists, but there is no client with UserId = 888 in DB)
            Assert.Throws<ClientNotFoundException>(() =>
                _depositService.RequestDeposit(userId: 888L, bank.Id, sumOfDeposit: 2000m, term: 12, interest: 6m)
            );

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void RequestDeposit_ClientIsInactive_ShouldThrowInvalidClientStatusException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            // Create an inactive client (e.g., Blocked) to trigger validation failure
            Client inactiveClient = CreateTestClient(status: ClientStatus.Blocked);

            _context.Banks.Add(bank);
            _context.Clients.Add(inactiveClient);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidClientStatusException>(() =>
                _depositService.RequestDeposit(inactiveClient.UserId, bank.Id, sumOfDeposit: 2000m, term: 12, interest: 6m)
            );

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }
    }
}
