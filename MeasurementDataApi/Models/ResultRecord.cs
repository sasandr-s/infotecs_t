using System;
using System.ComponentModel.DataAnnotations;

namespace MeasurementDataApi.Models;

public class ResultRecord
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Delta time in seconds (Max Date - Min Date)
    /// </summary>
    public double TimeDelta { get; set; }

    /// <summary>
    /// Minimum Date (Start time of first operation)
    /// </summary>
    public DateTime MinDate { get; set; }

    public double AvgExecutionTime { get; set; }

    public double AvgValue { get; set; }

    public double MedianValue { get; set; }

    public double MaxValue { get; set; }

    public double MinValue { get; set; }
}
