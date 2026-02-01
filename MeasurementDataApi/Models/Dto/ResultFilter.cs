namespace MeasurementDataApi.Models.Dto;

/// <summary>
/// Фильтр для получения результатов (Метод 2).
/// </summary>
public record ResultFilter
{
    /// <summary>
    /// Имя файла для фильтрации.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// Минимальная дата запуска (от).
    /// </summary>
    public DateTime? MinDateFrom { get; init; }

    /// <summary>
    /// Минимальная дата запуска (до).
    /// </summary>
    public DateTime? MinDateTo { get; init; }

    /// <summary>
    /// Среднее значение показателя (от).
    /// </summary>
    public double? AvgValueFrom { get; init; }

    /// <summary>
    /// Среднее значение показателя (до).
    /// </summary>
    public double? AvgValueTo { get; init; }

    /// <summary>
    /// Среднее время выполнения (от).
    /// </summary>
    public double? AvgExecTimeFrom { get; init; }

    /// <summary>
    /// Среднее время выполнения (до).
    /// </summary>
    public double? AvgExecTimeTo { get; init; }
}
