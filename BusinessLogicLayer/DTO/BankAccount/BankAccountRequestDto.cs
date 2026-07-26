using DataAccessLayer.Enums.BankAccount;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO.BankAccount
{
    public class OpenBankAccountRequestDto
    {
        public int BankId { get; set; }
        public BankAccountType bankAccountType { get; set; } = BankAccountType.Current;
    }
}
