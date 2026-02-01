using System;
using System.ComponentModel.DataAnnotations;

namespace MeasurementDataApi.Models;

public class ValueRecord
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public double ExecutionTime { get; set; }

    public double Value { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int LineNumber { get; set; }
}
