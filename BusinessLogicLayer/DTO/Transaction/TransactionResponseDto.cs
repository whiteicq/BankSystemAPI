using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO.Transaction
{
    public class TransactionResponseDto
    {
        public decimal TransactionAmount { get; set; }
        public string SenderBankAccountNumber { get; set; } = null!;
        public string ReceiverBankAccountNumber { get; set; } = null!;
        public TransactionType Type { get; set; }
        public CurrencyType Currency { get; set; }
    }
}
