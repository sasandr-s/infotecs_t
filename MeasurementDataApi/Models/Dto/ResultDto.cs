using MeasurementDataApi.Models;

namespace MeasurementDataApi.Models.Dto;

/// <summary>
/// DTO для результата расчётов (Метод 2).
/// </summary>
public record ResultDto
{
    /// <summary>
    /// Имя файла.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Разница между максимальной и минимальной датой (в секундах).
    /// </summary>
    public double TimeDeltaSeconds { get; init; }

    /// <summary>
    /// Минимальная дата среди всех записей.
    /// </summary>
    public DateTime MinDate { get; init; }

    /// <summary>
    /// Среднее время выполнения.
    /// </summary>
    public double AvgExecutionTime { get; init; }

    /// <summary>
    /// Среднее значение показателя.
    /// </summary>
    public double AvgValue { get; init; }

    /// <summary>
    /// Медианное значение показателя.
    /// </summary>
    public double MedianValue { get; init; }

    /// <summary>
    /// Максимальное значение показателя.
    /// </summary>
    public double MaxValue { get; init; }

    /// <summary>
    /// Минимальное значение показателя.
    /// </summary>
    public double MinValue { get; init; }

    /// <summary>
    /// Создаёт DTO из модели ResultRecord.
    /// </summary>
    public static ResultDto FromEntity(ResultRecord entity) => new()
    {
        FileName = entity.FileName,
        TimeDeltaSeconds = entity.TimeDelta,
        MinDate = entity.MinDate,
        AvgExecutionTime = entity.AvgExecutionTime,
        AvgValue = entity.AvgValue,
        MedianValue = entity.MedianValue,
        MaxValue = entity.MaxValue,
        MinValue = entity.MinValue
    };
}
