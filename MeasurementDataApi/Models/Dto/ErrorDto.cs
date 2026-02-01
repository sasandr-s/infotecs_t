namespace MeasurementDataApi.Models.Dto;

/// <summary>
/// DTO для ошибки сервера.
/// </summary>
public record ErrorDto
{
    /// <summary>
    /// Описание ошибки.
    /// </summary>
    public required string Error { get; init; }
}
