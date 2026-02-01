using Microsoft.Extensions.Options;
using MeasurementDataApi.Exceptions;
using MeasurementDataApi.Models;

namespace MeasurementDataApi.Services.Validation;

/// <summary>
/// Реализация валидатора данных на основе конфигурации FileValidationSettings.
/// </summary>
public class ValueValidator : IValueValidator
{
    private readonly FileValidationSettings _settings;
    private readonly DateTime _minAllowedDate;

    public ValueValidator(IOptions<FileValidationSettings> settings)
    {
        _settings = settings.Value;
        _minAllowedDate = new DateTime(_settings.MinAllowedYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <inheritdoc/>
    public void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ValidationException("Файл пустой или не передан.");
        }
    }

    /// <inheritdoc/>
    public void ValidateRowCount(int count)
    {
        if (count < _settings.MinRowCount || count > _settings.MaxRowCount)
        {
            throw new ValidationException(
                $"Количество строк ({count}) должно быть от {_settings.MinRowCount} до {_settings.MaxRowCount}.");
        }
    }

    /// <inheritdoc/>
    public List<string> ValidateValues(List<ValueRecord> values)
    {
        var allErrors = new List<string>();
        for (int i = 0; i < values.Count; i++)
        {
            var record = values[i];
            ValidateRecord(allErrors, record.LineNumber, record.Date, record.ExecutionTime, record.Value);
        }

        return allErrors;
    }

    private void ValidateRecord(List<string> errors, int lineNumber, DateTime date, double execTime, double value)
    {
        if (date < _minAllowedDate || date > DateTime.UtcNow)
        {
            errors.Add($"Строка {lineNumber}: Дата {date:yyyy-MM-dd} вне допустимого диапазона ({_settings.MinAllowedYear}-01-01 — текущая дата).");
        }

        if (!_settings.AllowNegativeExecutionTime && execTime < 0)
        {
            errors.Add($"Строка {lineNumber}: Время выполнения не может быть отрицательным ({execTime}).");
        }

        if (!_settings.AllowNegativeValue && value < 0)
        {
            errors.Add($"Строка {lineNumber}: Значение показателя не может быть отрицательным ({value}).");
        }
    }
}
