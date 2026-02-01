using MeasurementDataApi.Models;

namespace MeasurementDataApi.Models.Dto;

/// <summary>
/// DTO для значения из таблицы Values (Метод 3).
/// </summary>
public record ValueDto
{
    /// <summary>
    /// Дата записи.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// Время выполнения (в секундах).
    /// </summary>
    public double ExecutionTime { get; init; }

    /// <summary>
    /// Значение показателя.
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// Создаёт DTO из модели ValueRecord.
    /// </summary>
    public static ValueDto FromEntity(ValueRecord entity) => new()
    {
        Date = entity.Date,
        ExecutionTime = entity.ExecutionTime,
        Value = entity.Value
    };
}
