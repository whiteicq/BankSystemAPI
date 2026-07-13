using CoreLayer.Enums.Common;
using CoreLayer.Enums.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Transactions;
using TransactionStatus = CoreLayer.Enums.Transaction.TransactionStatus;

namespace CoreLayer
{
    public class Transaction
    {
        public long Id { get; set; }
        public CurrencyType Currency { get; set; }
        public decimal TransactionAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public BankAccount? Sender { get; set; }
        public long SenderId { get; set; }
        [Required]
        public BankAccount? Reciever { get; set; }
        public long RecieverId { get; set; }
        public TransactionStatus Status { get; set; }
        public TransactionType TransactionType { get; set; }
        
    }
}
