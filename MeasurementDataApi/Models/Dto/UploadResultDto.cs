namespace MeasurementDataApi.Models.Dto;

/// <summary>
/// DTO для успешного ответа на загрузку файла.
/// </summary>
public record UploadResultDto
{
    /// <summary>
    /// Сообщение о результате операции.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Количество обработанных записей.
    /// </summary>
    public int RecordsProcessed { get; init; }

    /// <summary>
    /// Имя загруженного файла.
    /// </summary>
    public required string FileName { get; init; }
}
