using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.Transaction;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Transaction
{
    public long Id { get; set; }

    public decimal TransactionAmount { get; set; } = 0.0m;

    [MaxLength(3)]
    public CurrencyType Currency { get; set; } = CurrencyType.BYN;
    [MaxLength(15)]
    public TransactionStatus Status { get; set; } = TransactionStatus.Confirmed;

    [MaxLength(20)]
    public TransactionType Type { get; set; }

    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public long SenderId { get; set; }

    public long ReceiverId { get; set; }

    public virtual BankAccount Receiver { get; set; } = null!;

    public virtual BankAccount Sender { get; set; } = null!;
}
