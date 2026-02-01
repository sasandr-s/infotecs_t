namespace MeasurementDataApi.Models.Dto;

/// <summary>
/// DTO для ответа с ошибками валидации.
/// </summary>
public record ValidationErrorDto
{
    /// <summary>
    /// Список ошибок валидации.
    /// </summary>
    public required List<string> Errors { get; init; }

    /// <summary>
    /// Количество ошибок.
    /// </summary>
    public int ErrorCount => Errors.Count;
}
