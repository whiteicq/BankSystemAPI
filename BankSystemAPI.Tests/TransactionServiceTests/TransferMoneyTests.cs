using BusinessLogicLayer.Exceptions.Transaction;
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
    public class TransferMoneyTests : TransactionServiceTests
    {
        [Fact]
        public void TransferMoney_ValidP2PTransfer_ShouldUpdateBalancesAndSaveTransaction()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L, userId: 10L);
            BankAccount sender = CreateTestBankAccount(id: 100L, clientId: client.Id, balance: 1500m, number: "SENDER-123");
            client.BankAccounts.Add(sender);

            // Receiver doesn't need to belong to this client, just needs to exist in the global system DB
            BankAccount receiver = CreateTestBankAccount(id: 200L, clientId: 2L, balance: 500m, number: "RECEIVER-777");

            _context.Clients.Add(client);
            _context.BankAccounts.AddRange(sender, receiver);
            _context.SaveChanges();

            decimal transferAmount = 500m;

            // Act
            Transaction result = _service.TransferMoney(client.UserId, transferAmount, sender.BankAccountNumber, receiver.BankAccountNumber);

            // Assert
            // 1. Verify that corporate cash calculations are perfectly balancing out
            Assert.NotNull(result);
            Assert.Equal(1000m, sender.MoneyBalance);   // 1500 - 500
            Assert.Equal(1000m, receiver.MoneyBalance); // 500 + 500

            // 2. Verify that transaction telemetry details were properly mapped
            Assert.Equal(transferAmount, result.TransactionAmount);
            Assert.Equal(sender.Id, result.SenderId);
            Assert.Equal(receiver.Id, result.ReceiverId);
            Assert.Equal(TransactionType.PeerToPeer, result.Type);
            Assert.Equal(CurrencyType.BYN, result.Currency);

            // 3. Verify physical row inclusion within the primary DB context tables
            Assert.NotNull(_context.Set<Transaction>().Find(result.Id));

            // 4. Verify that logging records match execution payload strings
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.TRANSACTION_COMPLETED,
                nameof(Transaction),
                result.Id,
                null,
                transferAmount.ToString()
                ),
                Times.Once);
        }

        [Fact]
        public void TransferMoney_NegativeAmount_ShouldThrowArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.TransferMoney(userId: 1L, amount: -100m, "1111", "2222")
            );
        }

        [Fact]
        public void TransferMoney_SenderHasInsufficientFunds_ShouldThrowInsufficientFundsException()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L, userId: 10L);
            BankAccount sender = CreateTestBankAccount(id: 100L, clientId: client.Id, balance: 100m, number: "SENDER-123");
            client.BankAccounts.Add(sender);
            BankAccount receiver = CreateTestBankAccount(id: 200L, clientId: 2L, balance: 500m, number: "RECEIVER-777");

            _context.Clients.Add(client);
            _context.BankAccounts.AddRange(sender, receiver);
            _context.SaveChanges();

            // Act & Assert - Attempting to transfer 500m when sender only possesses 100m
            Assert.Throws<InsufficientFundsException>(() =>
                _service.TransferMoney(client.UserId, amount: 500m, sender.BankAccountNumber, receiver.BankAccountNumber)
            );
        }

        [Fact]
        public void TransferMoney_ReceiverBankAccountIsInactive_ShouldThrowInvalidTransactionStatusException()
        {
            // Arrange
            Client client = CreateTestClient(id: 1L, userId: 10L);
            BankAccount sender = CreateTestBankAccount(id: 100L, clientId: client.Id, balance: 2000m, number: "SENDER-123");
            client.BankAccounts.Add(sender);

            // Receiver account is locked/inactive (Closed)
            BankAccount inactiveReceiver = CreateTestBankAccount(id: 200L, clientId: 2L, balance: 500m, number: "RECEIVER-777", status: BankAccountStatus.Closed);

            _context.Clients.Add(client);
            _context.BankAccounts.AddRange(sender, inactiveReceiver);
            _context.SaveChanges();

            // Act & Assert
            Assert.Throws<InvalidTransactionStatusException>(() =>
                _service.TransferMoney(client.UserId, amount: 500m, sender.BankAccountNumber, inactiveReceiver.BankAccountNumber)
            );
        }
    }
}
