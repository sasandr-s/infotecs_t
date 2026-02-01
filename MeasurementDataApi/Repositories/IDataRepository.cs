using MeasurementDataApi.Models;
using MeasurementDataApi.Models.Dto;

namespace MeasurementDataApi.Repositories;

/// <summary>
/// Репозиторий для работы с данными измерений и результатами обработки.
/// </summary>
public interface IDataRepository
{
    /// <summary>
    /// Сохраняет данные файла и результаты обработки в БД.
    /// Метод выполняется транзакционно: удаляет старые данные, загружает новые, вычисляет медиану и сохраняет итог.
    /// </summary>
    /// <param name="fileName">Имя файла.</param>
    /// <param name="values">Список значений для сохранения.</param>
    /// <param name="result">Объект с результатами статистики.</param>
    Task SaveDataAsync(string fileName, List<ValueRecord> values, ResultRecord result);

    /// <summary>
    /// Возвращает список результатов, соответствующих фильтрам.
    /// </summary>
    Task<List<ResultRecord>> GetResultsAsync(ResultFilter filter);

    /// <summary>
    /// Возвращает последние 10 записей для указанного файла, отсортированные по дате.
    /// </summary>
    Task<List<ValueRecord>> GetLast10ValuesAsync(string fileName);
}
