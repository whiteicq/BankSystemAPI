using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BusinessLogicLayer.DTO.Credit
{
    public class CreditResponseDto
    {
        public decimal LoanAmount { get; set; }

        public decimal LoanBalance { get; set; }

        public int LoanTerm { get; set; }

        public decimal LoanInterest { get; set; }

        public DateOnly OpenedAt { get; set; }

        public CreditStatus Status { get; set; } = CreditStatus.Unactivated;

        public CurrencyType Currency { get; set; } = CurrencyType.BYN;

        public long ClientId { get; set; }
    }
}
