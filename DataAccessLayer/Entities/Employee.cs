using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Employee
{
    public long Id { get; set; }

    [MaxLength(20)]
    public string Role { get; set; } = null!;

    [MaxLength(25)]
    public string Name { get; set; } = null!;

    [MaxLength(25)]
    public string? Patronymic { get; set; }

    [MaxLength(25)]
    public string Surname { get; set; } = null!;

    [MaxLength(13)]
    public string PhoneNumber { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public DateOnly HireDate { get; set; }

    [MaxLength(10)]
    public string PersonellNumber { get; set; } = null!;

    public long? PassportId { get; set; }

    public long BankId { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual Passport Passport { get; set; } = null!;

    public long UserId { get; set; }
}
