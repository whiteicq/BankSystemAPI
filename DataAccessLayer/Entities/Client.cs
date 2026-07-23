using DataAccessLayer.Enums.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities;

public partial class Client
{
    public long Id { get; set; }

    [MaxLength(25)]
    public string Name { get; set; } = null!;

    [MaxLength(25)]
    public string? Patronymic { get; set; }
    [MaxLength(25)]
    public string Surname { get; set; } = null!;

    [MaxLength(13)]
    public string PhoneNumber { get; set; } = null!;

    public DateOnly BirthDate { get; set; }
    [MaxLength(20)]
    public ClientStatus Status { get; set; } = ClientStatus.Unactive;

    public long? PassportId { get; set; }

    public virtual Passport Passport { get; set; } = null!;

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
    public long UserId { get; set; }
}
