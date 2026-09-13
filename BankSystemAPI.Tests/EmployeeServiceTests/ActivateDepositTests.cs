using BusinessLogicLayer.Exceptions.Transaction;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class ActivateDepositTests : EmployeeServiceTests
    {
        [Fact]
        public void ActivateDeposit_ValidData_ShouldCallDepositServiceAndLog()
        {
            // Arrange
            long clientId = 1L;
            long depositId = 25L;
            long senderAccountId = 10L;

            // Setup the mock deposit service to accept the call without throwing exceptions
            _depositServiceMock
                .Setup(s => s.TransferMoneyForDeposit(clientId, depositId, senderAccountId))
                .Verifiable();

            // Act
            _employeeService.ActivateDeposit(clientId, depositId, senderAccountId);

            // Assert
            // Verify that the EmployeeService correctly delegated the fund transfer process to DepositService
            _depositServiceMock.Verify(s => s.TransferMoneyForDeposit(clientId, depositId, senderAccountId), Times.Once);

            // Verify that the deposit approval log was generated after successful execution
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.DEPOSIT_APPROVED,
                nameof(Deposit),
                depositId,
                null,
                null
                ),
                Times.Once);
        }

        [Fact]
        public void ActivateDeposit_DepositServiceThrowsException_ShouldPropagateExceptionAndNotLog()
        {
            // Arrange
            long clientId = 1L;
            long depositId = 25L;
            long senderAccountId = 10L;

            // Instruct the mock deposit service to simulate a failure (e.g., insufficient funds or inactive sender account)
            _depositServiceMock
                .Setup(s => s.TransferMoneyForDeposit(clientId, depositId, senderAccountId))
                .Throws(new InsufficientFundsException("Insufficient funds in the bank account"));

            // Act & Assert
            // Verify that the exception bubbles up seamlessly out of the method execution flow
            Assert.Throws<InsufficientFundsException>(() =>
                _employeeService.ActivateDeposit(clientId, depositId, senderAccountId)
            );

            // Verify that because of the exception, the approval log statement was safely skipped
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.DEPOSIT_APPROVED,
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }
    }
}
