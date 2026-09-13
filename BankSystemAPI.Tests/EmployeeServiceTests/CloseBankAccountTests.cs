using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.EmployeeServiceTests
{
    public class CloseBankAccountTests : EmployeeServiceTests
    {
        [Fact]
        public void CloseBankAccount_ValidData_ShouldCallBankAccountServiceOnce()
        {
            // Arrange
            long targetBankAccountId = 42L;

            // Setup the mock bank account service to accept the call without errors
            _bankAccountServiceMock
                .Setup(s => s.SystemCloseBankAccount(targetBankAccountId))
                .Verifiable();

            // Act
            _employeeService.CloseBankAccount(targetBankAccountId);

            // Assert
            // Verify that EmployeeService correctly delegated the work to BankAccountService with matching ID
            _bankAccountServiceMock.Verify(s => s.SystemCloseBankAccount(targetBankAccountId), Times.Once);
        }

        [Fact]
        public void CloseBankAccount_ServiceThrowsException_ShouldPropagateExceptionWithoutInterruption()
        {
            // Arrange
            long targetBankAccountId = 999L;

            // Instruct the mock bank account service to simulate a failure (e.g., account has positive balance or is not found)
            _bankAccountServiceMock
                .Setup(s => s.SystemCloseBankAccount(targetBankAccountId))
                .Throws(new InvalidOperationException("Невозможно закрыть счет со средствами на балансе"));

            // Act & Assert
            // Verify that the exception bubbles up seamlessly out of the employee service method execution
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _employeeService.CloseBankAccount(targetBankAccountId)
            );

            Assert.Equal("Невозможно закрыть счет со средствами на балансе", exception.Message);

            // Ensure that the call to the underlying service was attempted exactly once
            _bankAccountServiceMock.Verify(s => s.SystemCloseBankAccount(targetBankAccountId), Times.Once);
        }
    }
}
