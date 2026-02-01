using MeasurementDataApi.Models;

namespace MeasurementDataApi.Services.Statistics;

/// <summary>
/// Реализация калькулятора статистики на основе LINQ.
/// </summary>
public class StatisticsCalculator : IStatisticsCalculator
{
    /// <inheritdoc/>
    public ResultRecord CalculateStatistics(List<ValueRecord> values, string fileName)
    {
        if (values.Count == 0) return new ResultRecord { FileName = fileName };

        // 1. Расчет агрегатов за один проход
        DateTime minDate = values[0].Date;
        DateTime maxDate = values[0].Date;
        double sumExecutionTime = 0;
        double sumValue = 0;
        double maxValue = values[0].Value;
        double minValue = values[0].Value;

        foreach (var record in values)
        {
            if (record.Date < minDate) minDate = record.Date;
            if (record.Date > maxDate) maxDate = record.Date;
            
            sumExecutionTime += record.ExecutionTime;
            sumValue += record.Value;
            
            if (record.Value > maxValue) maxValue = record.Value;
            if (record.Value < minValue) minValue = record.Value;
        }

        // 2. Быстрый расчет медианы в памяти (многократно быстрее, чем запрос к БД с процентилями)
        var sortedValues = values.Select(v => v.Value).OrderBy(v => v).ToList();
        double median;
        int count = sortedValues.Count;
        if (count % 2 == 0)
        {
            median = (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }
        else
        {
            median = sortedValues[count / 2];
        }

        return new ResultRecord
        {
            FileName = fileName,
            TimeDelta = (maxDate - minDate).TotalSeconds,
            MinDate = minDate,
            AvgExecutionTime = sumExecutionTime / values.Count,
            AvgValue = sumValue / values.Count,
            MedianValue = median,
            MaxValue = maxValue,
            MinValue = minValue
        };
    }
}
