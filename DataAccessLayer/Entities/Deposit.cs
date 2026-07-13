using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.FinancialProduct.Deposit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Deposit
{
    public long Id { get; set; }

    public decimal DepositAmount { get; set; }

    public float DepositInterest { get; set; }

    public int DepositTerm { get; set; }

    public DateOnly OpenedAt { get; set; }

    public DateOnly? ClosedAt { get; set; }

    [MaxLength(20)]
    public DepositStatus Status { get; set; }

    [MaxLength(3)]
    public CurrencyType Currency { get; set; } = CurrencyType.BYN;

    public long ClientId { get; set; }

    public long BankId { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;
}
