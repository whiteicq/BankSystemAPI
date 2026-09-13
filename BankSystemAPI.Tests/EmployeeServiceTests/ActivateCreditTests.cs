using BusinessLogicLayer.Exceptions.Credit;
using DataAccessLayer.Entities;
using DataAccessLayer.Enums.Logs;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class ActivateCreditTests : EmployeeServiceTests
    {
        [Fact]
        public void ActivateCredit_ValidData_ShouldCallCreditServiceAndLog()
        {
            // Arrange
            long clientId = 1L;
            long creditId = 10L;
            long receiverAccountId = 55L;

            // We do not need to populate DB because we mock the underlying service behavior
            _creditServiceMock
                .Setup(s => s.TransferMoneyForLoan(clientId, creditId, receiverAccountId))
                .Verifiable(); // Marks this setup as expected to be called

            // Act
            _employeeService.ActivateCredit(clientId, creditId, receiverAccountId);

            // Assert
            // Verify that EmployeeService successfully delegated the work to CreditService
            _creditServiceMock.Verify(s => s.TransferMoneyForLoan(clientId, creditId, receiverAccountId), Times.Once);

            // Verify that the approval log was generated after successful execution
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.CREDIT_APPROVED,
                nameof(Credit),
                creditId,
                null, // Assuming default null values for unpassed optional logging parameters
                null
                ),
                Times.Once);
        }

        [Fact]
        public void ActivateCredit_CreditServiceThrowsException_ShouldPropagateExceptionAndNotLog()
        {
            // Arrange
            long clientId = 1L;
            long creditId = 10L;
            long receiverAccountId = 55L;

            // Instruct the mock credit service to fail with an exception (e.g., credit not found or inactive client)
            _creditServiceMock
                .Setup(s => s.TransferMoneyForLoan(clientId, creditId, receiverAccountId))
                .Throws(new InvalidCreditStatusException("Cannot transfer money on loan twice"));

            // Act & Assert
            // Verify that the exception bubbles up perfectly out of the method
            Assert.Throws<InvalidCreditStatusException>(() =>
                _employeeService.ActivateCredit(clientId, creditId, receiverAccountId)
            );

            // Verify that because of the exception, the approval log statement was NEVER reached
            _loggerMock.Verify(m => m.MakeLog(
                OperationType.CREDIT_APPROVED,
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                ),
                Times.Never);
        }
    }
}
