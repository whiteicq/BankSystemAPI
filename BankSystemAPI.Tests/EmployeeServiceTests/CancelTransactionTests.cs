using BusinessLogicLayer.Exceptions.Transaction;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Logs;
using Moq;
using DataAccessLayer.Enums.Transaction;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class CancelTransactionTests : EmployeeServiceTests
    {
        [Fact]
        public void CancelTransaction_ValidConfirmedTransaction_ShouldRefundBalancesAndCancel()
        {
            // Arrange
            // 1. Create sender and receiver accounts with initial balances
            BankAccount sender = CreateTestBankAccount(id: 10L, moneyBalance: 1000m);
            BankAccount receiver = CreateTestBankAccount(id: 20L, moneyBalance: 2000m);

            // 2. Create a confirmed transaction of 500m from sender to receiver
            decimal transactionAmount = 500m;
            Transaction transaction = CreateTestTransaction(id: 100L, sender: sender, receiver: receiver, amount: transactionAmount, status: TransactionStatus.Confirmed);

            _context.BankAccounts.AddRange(sender, receiver);
            _context.Set<Transaction>().Add(transaction);
            _context.SaveChanges();

            string expectedOldStatus = TransactionStatus.Confirmed.ToString();
            string expectedNewStatus = TransactionStatus.Canceled.ToString();

            // Act
            _employeeService.CancelTransaction(transaction.Id);

            // Assert
            // Verify money was successfully refunded (Sender gets money back, Receiver loses it)
            Assert.Equal(1000m + transactionAmount, sender.MoneyBalance); // 1500m
            Assert.Equal(2000m - transactionAmount, receiver.MoneyBalance); // 1500m

            // Verify transaction status was updated to Canceled
            Assert.Equal(TransactionStatus.Canceled, transaction.Status);

            // Verify changes were persisted in the database
            Transaction transactionFromDb = _context.Set<Transaction>().Find(transaction.Id);
            Assert.NotNull(transactionFromDb);
            Assert.Equal(TransactionStatus.Canceled, transactionFromDb.Status);

            // Verify that the logger recorded the cancellation event after SaveChanges
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.TRANSACTION_CANCELED,
                nameof(Transaction),
                transaction.Id,
                expectedOldStatus,
                expectedNewStatus
                ),
                Times.Once);
        }

        [Fact]
        public void CancelTransaction_TransactionIsAlreadyCanceled_ShouldThrowTransactionNotFoundException()
        {
            // Arrange
            BankAccount sender = CreateTestBankAccount(id: 10L);
            BankAccount receiver = CreateTestBankAccount(id: 20L);

            // Create a transaction that is ALREADY Canceled (FirstOrDefault query should filter this out)
            Transaction canceledTransaction = CreateTestTransaction(id: 100L, sender: sender, receiver: receiver, status: TransactionStatus.Canceled);

            _context.BankAccounts.AddRange(sender, receiver);
            _context.Set<Transaction>().Add(canceledTransaction);
            _context.SaveChanges();

            // Act & Assert
            // The service should fail because it only looks for Confirmed transactions
            Assert.Throws<TransactionNotFoundException>(() =>
                _employeeService.CancelTransaction(canceledTransaction.Id)
            );

            // Ensure balances remain untouched
            Assert.Equal(1000m, sender.MoneyBalance);
            Assert.Equal(1000m, receiver.MoneyBalance);

            // Ensure logger was never called
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
        public void CancelTransaction_TransactionDoesNotExist_ShouldThrowTransactionNotFoundException()
        {
            // Act & Assert (Database is empty, searching for non-existent ID 999 must throw exception)
            Assert.Throws<TransactionNotFoundException>(() =>
                _employeeService.CancelTransaction(transactionId: 999L)
            );

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
