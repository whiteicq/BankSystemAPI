using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Log
{
    public long Id { get; set; }

    [MaxLength(20)]
    public string TypeOperation { get; set; } = null!;

    [MaxLength(30)]
    public string TargetTable { get; set; } = null!;

    public int TargetRowId { get; set; }
    [MaxLength(45)]
    public string? OldValue { get; set; }
    [MaxLength(45)]
    public string? NewValue { get; set; }

    public DateOnly CreatedAt { get; set; }

    public long? EmployeeId { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
