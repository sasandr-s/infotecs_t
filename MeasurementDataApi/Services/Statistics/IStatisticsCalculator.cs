using MeasurementDataApi.Models;

namespace MeasurementDataApi.Services.Statistics;

/// <summary>
/// Интерфейс для расчета статистических показателей по выборке данных.
/// </summary>
public interface IStatisticsCalculator
{
    /// <summary>
    /// Рассчитывает интегральные показатели (статистику) для списка записей измерения.
    /// </summary>
    /// <param name="values">Список разобранных записей.</param>
    /// <param name="fileName">Имя файла.</param>
    /// <returns>Заполненный объект ResultRecord (медиана заполняется позже в БД).</returns>
    ResultRecord CalculateStatistics(List<ValueRecord> values, string fileName);
}
