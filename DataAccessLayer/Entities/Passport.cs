using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Passport
{
    public long Id { get; set; }

    [MaxLength(14)]
    public string IdentificationNumber { get; set; } = null!;

    [MaxLength(2)]
    public string Series { get; set; } = null!;

    [MaxLength(7)]
    public string Number { get; set; } = null!;

    public virtual Client Client { get; set; } = null!; 

    public virtual Employee Employee { get; set; } = null!;
}
