namespace MeasurementDataApi.Models;

/// <summary>
/// Настройки валидации файлов с данными измерений.
/// </summary>
public class FileValidationSettings
{
    /// <summary>
    /// Секция конфигурации.
    /// </summary>
    public const string SectionName = "FileValidation";

    /// <summary>
    /// Минимально допустимый год в дате записи.
    /// </summary>
    public int MinAllowedYear { get; set; } = 2000;

    /// <summary>
    /// Минимальное количество строк данных в файле.
    /// </summary>
    public int MinRowCount { get; set; } = 1;

    /// <summary>
    /// Максимальное количество строк данных в файле.
    /// </summary>
    public int MaxRowCount { get; set; } = 10000;

    /// <summary>
    /// Разрешить отрицательные значения ExecutionTime.
    /// </summary>
    public bool AllowNegativeExecutionTime { get; set; } = false;

    /// <summary>
    /// Разрешить отрицательные значения Value.
    /// </summary>
    public bool AllowNegativeValue { get; set; } = false;
}
