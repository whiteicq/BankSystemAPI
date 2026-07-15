using DataAccessLayer.Enums.Client;
using DataAccessLayer.Enums.Common;
using DataAccessLayer.Enums.FinancialProduct.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Credit
{
    public long Id { get; set; }

    public decimal LoanAmount { get; set; }

    public decimal LoanBalance { get; set; } = 0.0m;

    public int LoanTerm { get; set; }

    public decimal LoanInterest { get; set; }

    public DateOnly OpenedAt { get; set; }

    public DateOnly? ClosedAt { get; set; }

    [MaxLength(20)]
    public CreditStatus Status { get; set; } = CreditStatus.Unactivated;

    [MaxLength(3)]
    public CurrencyType Currency { get; set; } = CurrencyType.BYN;

    public long ClientId { get; set; }

    public long BankId { get; set; }

    public long? BankAccountId { get; set; }
    public virtual BankAccount? BankAccount { get; set; } = null!;

    public virtual Bank Bank { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;
}
