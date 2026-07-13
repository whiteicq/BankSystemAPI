using CoreLayer.Enums.FinancialProduct.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer.FinancialProducts
{
    public class Credit : FinancialProduct
    {
        public CreditStatus Status { get; set; }
        public decimal Balance { get; set; }
        [Required]
        public Bank? Bank { get; set; }
        public long BankId { get; set; }
    }
}
