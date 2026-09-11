using BusinessLogicLayer.Exceptions.Bank;
using BusinessLogicLayer.Exceptions.Client;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using DataAccessLayer.Enums.Logs;
using Moq;

namespace BankSystemAPI.Tests.CreditServiceTests
{
    public class RequestCreditTests : CreditServiceTests
    {
        [Fact]
        public void RequestCredit_ValidData_ShouldCreateUnactivatedCredit()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client client = CreateTestClient();

            _context.Banks.Add(bank);
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            Credit result = _service.RequestCredit(client.UserId, bank.Id, sumOfLoan: 10000m, term: 12, interest: 15m);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10000m, result.LoanAmount);
            Assert.Equal(bank.Id, result.BankId);
            Assert.Equal(client.Id, result.ClientId);
            Assert.Equal(CreditStatus.Unactivated, result.Status);
            Assert.NotNull(_context.Credits.Find(result.Id));

            _loggerMock.Verify(m => m.MakeLog(
                OperationType.CREDIT_REQUESTED,
                nameof(Credit),
                result.Id,
                null,
                CreditStatus.Unactivated.ToString()
                ),
                Times.Once);
        }


        [Fact]
        public void RequestCredit_BankIsNull_ShouldThrowBankNotFoundException()
        {
            // Arrange
            Client client = CreateTestClient();
            _context.Clients.Add(client);
            _context.SaveChanges();

            // Act
            // Assert
            Assert.Throws<BankNotFoundException>(() => _service.RequestCredit(client.UserId, bankId: 999L, sumOfLoan: 5000m, term: 12, interest: 15m)
            );

            _loggerMock.Verify(m => m.MakeLog(
                It.IsAny<OperationType>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Theory]
        [InlineData(0, 12, 15)]    // sumOfLoan = 0
        [InlineData(-500, 12, 15)] // sumOfLoan < 0
        [InlineData(5000, 0, 15)]  // term = 0
        [InlineData(5000, -6, 15)] // term < 0
        [InlineData(5000, 12, 14)] // interest on lowest border of range[14..25] 
        [InlineData(5000, 12, 10)] // interest < 14
        [InlineData(5000, 12, 25)] // interest on highest border of reange [14..25] 
        [InlineData(5000, 12, 30)] // interest > 25
        public void RequestCredit_InvalidArguments_ShouldThrowArgumentOutOfRangeException(decimal sumOfLoan, int term, decimal interest)
        {
            // Arrange
            Bank bank = CreateTestBank();
            _context.Banks.Add(bank);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.RequestCredit(userId: 1L, bank.Id, sumOfLoan, term, interest)
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
        public void RequestCredit_ClientNotExists_ShouldThrowClientNotFoundException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            _context.Banks.Add(bank);
            _context.SaveChanges();

            // Act & Assert 
            Assert.Throws<ClientNotFoundException>(() =>
                _service.RequestCredit(userId: 999L, bank.Id, sumOfLoan: 5000m, term: 12, interest: 15m)
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
        public void RequestCredit_ClientIsIUnactive_ShouldThrowInvalidClientStatusException()
        {
            // Arrange
            Bank bank = CreateTestBank();
            Client inactiveClient = CreateTestClient(status: ClientStatus.Blocked);

            _context.Banks.Add(bank);
            _context.Clients.Add(inactiveClient);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidClientStatusException>(() =>
                _service.RequestCredit(inactiveClient.UserId, bank.Id, sumOfLoan: 5000m, term: 12, interest: 15m)
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
