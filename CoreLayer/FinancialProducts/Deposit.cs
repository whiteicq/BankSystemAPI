using CoreLayer.Enums.FinancialProduct.Deposit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer.FinancialProducts
{
    public class Deposit : FinancialProduct
    {
        public DepositStatus Status { get; set; }
        [Required]
        public Bank? Bank { get; set; }
        public long BankId { get; set; }
    }
}
