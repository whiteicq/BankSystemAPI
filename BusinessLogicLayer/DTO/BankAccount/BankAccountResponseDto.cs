using DataAccessLayer.Enums.BankAccount;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO.BankAccount
{
    public class BankAccountResponseDto
    {
        public decimal MoneyBalance { get; set; }
        public string BankAccountNumber { get; set; } = null!;
        public BankAccountType Type { get; set; }
        public BankAccountStatus Status { get; set; }
        public long ClientId { get; set; }
    }
}
