using System;
using System.Collections.Generic;
using System.Text;

namespace BankSystemAPI.Tests.BankAccountServiceTests
{
    public class GenerateUniqueBankAccountNumberTests : BankAccountServiceTests
    {
        [Theory]
        [InlineData(28)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(100)]
        public void GenerateUniqueBankAccountNumber_ValidData_ShouldGenerateAndReturn(int length)
        {
            // Act
            string result = _service.GenerateUniqueBankAccountNumber(length);

            // Assert
            Assert.Equal(length, result.Length);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(-25)]
        [InlineData(-100)]
        [InlineData(-13)]
        public void GenerateUniqueBankAccountNumber_LengthLessOrEqualThanZero_ShouldGenerateAndReturn(int length)
        {
            // Act
            string result = _service.GenerateUniqueBankAccountNumber(length);

            // Assert
            Assert.Empty(result);
        }
    }
}
