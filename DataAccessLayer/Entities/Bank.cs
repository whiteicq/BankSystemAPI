using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Bank
{
    public long Id { get; set; }

    [MaxLength(30)]
    public string Title { get; set; } = null!;

    [MaxLength(8)]
    public string BIC { get; set; } = null!;

    [MaxLength(60)]
    public string Address { get; set; } = null!;

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
