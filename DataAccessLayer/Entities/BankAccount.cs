using DataAccessLayer.Enums.BankAccount;
using DataAccessLayer.Enums.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class BankAccount
{
    public long Id { get; set; }

    public decimal MoneyBalance { get; set; } = 0.0m;

    [MaxLength(28)]
    public string BankAccountNumber { get; set; } = null!;

    public DateOnly OpenedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? ClosedAt { get; set; }

    public long BankId { get; set; }

    public long ClientId { get; set; }

    [MaxLength(20)]
    public BankAccountType Type { get; set; }

    [MaxLength(20)]
    public BankAccountStatus Status { get; set; } = BankAccountStatus.Unactivated;
    [MaxLength(3)]
    public CurrencyType Currency { get; set; } = CurrencyType.BYN;

    public virtual Bank Bank { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Transaction> TransactionReceivers { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionSenders { get; set; } = new List<Transaction>();
}
