using CoreLayer.Actors;
using CoreLayer.Enums.Logs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CoreLayer
{
    public class Log
    {
        public long Id { get; set; }
        public OperationType LogType { get; set; }
        [Required]
        [MaxLength(30)]
        public string? TargetTable { get; set; }
        public long TargerRowId { get; set; }
        [MaxLength(45)]
        public string? OldValue { get; set; }
        [MaxLength(45)]
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public Employee? Employee { get; set; }
        public long EmployeeId { get; set; }
    }
}
