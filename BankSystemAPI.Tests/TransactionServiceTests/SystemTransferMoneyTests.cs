using BusinessLogicLayer.Exceptions.BankAccount;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Logs;
using DataAccessLayer.Enums.Transaction;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.TransactionServiceTests
{
    public class SystemTransferMoneyTests : TransactionServiceTests
    {
        [Fact]
        public void SystemTransferMoney_ValidSystemTransfer_ShouldUpdateBalancesAndExecuteSuccessfully()
        {
            // Arrange
            // System transfers specifically filter for sender.Type == BankAccountType.Current
            BankAccount sender = CreateTestBankAccount(id: 10L, clientId: 1L, balance: 5000m, type: BankAccountType.Current);
            BankAccount receiver = CreateTestBankAccount(id: 20L, clientId: 2L, balance: 100m, type: BankAccountType.Credit);

            _context.BankAccounts.AddRange(sender, receiver);
            _context.SaveChanges();

            decimal systemAmount = 2000m;

            // Act
            Transaction result = _service.SystemTransferMoney(systemAmount, sender.Id, receiver.Id, TransactionType.Credit, CurrencyType.BYN);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000m, sender.MoneyBalance);   // 5000 - 2000
            Assert.Equal(2100m, receiver.MoneyBalance); // 100 + 2000

            Assert.Equal(systemAmount, result.TransactionAmount);
            Assert.Equal(TransactionType.Credit, result.Type);

            _loggerMock.Verify(m => m.MakeLog(
                OperationType.TRANSACTION_COMPLETED,
                nameof(Transaction),
                result.Id,
                null,
                systemAmount.ToString()
                ),
                Times.Once);
        }

        [Fact]
        public void SystemTransferMoney_SenderAccountIsNotCurrentType_ShouldThrowBankAccountNotFoundException()
        {
            // Arrange
            // Sender account is marked as BankAccountType.Saving, which fails the internal type validation check
            BankAccount invalidSenderType = CreateTestBankAccount(id: 10L, clientId: 1L, balance: 5000m, type: BankAccountType.Credit);
            BankAccount receiver = CreateTestBankAccount(id: 20L, clientId: 2L, balance: 100m);

            _context.BankAccounts.AddRange(invalidSenderType, receiver);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<BankAccountNotFoundException>(() =>
                _service.SystemTransferMoney(amount: 1000m, invalidSenderType.Id, receiver.Id)
            );
        }
    }
}
