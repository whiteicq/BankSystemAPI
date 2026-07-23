using DataAccessLayer.Enums.Logs;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Entities;

public partial class Log
{
    public long Id { get; set; }

    [MaxLength(20)]
    public OperationType TypeOperation { get; set; }

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
