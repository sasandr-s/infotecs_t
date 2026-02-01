using MeasurementDataApi.Models;

namespace MeasurementDataApi.Services.Validation;

/// <summary>
/// Интерфейс для валидации импортируемых данных согласно бизнес-правилам.
/// </summary>
public interface IValueValidator
{
    /// <summary>
    /// Проверяет наличие файла и его длину.
    /// </summary>
    void ValidateFile(IFormFile file);

    /// <summary>
    /// Проверяет допустимое количество строк.
    /// </summary>
    void ValidateRowCount(int count);

    /// <summary>
    /// Валидирует список записей и возвращает список всех найденных ошибок.
    /// </summary>
    List<string> ValidateValues(List<ValueRecord> values);
}
