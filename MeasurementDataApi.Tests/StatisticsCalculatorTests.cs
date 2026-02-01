using MeasurementDataApi.Models;
using MeasurementDataApi.Services.Statistics;

namespace MeasurementDataApi.Tests;

public class StatisticsCalculatorTests
{
    private readonly StatisticsCalculator _calculator;

    public StatisticsCalculatorTests()
    {
        _calculator = new StatisticsCalculator();
    }

    [Fact]
    public void CalculateStatistics_EmptyList_ReturnsDefault()
    {
        // Arrange
        var values = new List<ValueRecord>();

        // Act
        var result = _calculator.CalculateStatistics(values, "test.csv");

        // Assert
        Assert.Equal("test.csv", result.FileName);
        Assert.Equal(0, result.AvgValue);
        Assert.Equal(0, result.MedianValue);
    }

    [Fact]
    public void CalculateStatistics_SingleValue_ReturnsCorrectStats()
    {
        // Arrange
        var date = new DateTime(2023, 10, 1, 12, 0, 0);
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = date, Value = 10, ExecutionTime = 5 }
        };

        // Act
        var result = _calculator.CalculateStatistics(values, "test.csv");

        // Assert
        Assert.Equal(10, result.MaxValue);
        Assert.Equal(10, result.MinValue);
        Assert.Equal(10, result.AvgValue);
        Assert.Equal(10, result.MedianValue);
        Assert.Equal(5, result.AvgExecutionTime);
        Assert.Equal(0, result.TimeDelta); // Для одной точки разница во времени 0
        Assert.Equal(date, result.MinDate);
    }

    [Fact]
    public void CalculateStatistics_OddCount_ReturnsCorrectMedian()
    {
        // Arrange
        // Значения: 10, 30, 20 -> Сортировка: 10, 20, 30 -> Медиана: 20
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = DateTime.MinValue, Value = 10 },
            new ValueRecord { Date = DateTime.MinValue, Value = 30 },
            new ValueRecord { Date = DateTime.MinValue, Value = 20 }
        };

        // Act
        var result = _calculator.CalculateStatistics(values, "test.csv");

        // Assert
        Assert.Equal(20, result.MedianValue);
        Assert.Equal(30, result.MaxValue);
        Assert.Equal(10, result.MinValue);
        Assert.Equal(20, result.AvgValue);
    }

    [Fact]
    public void CalculateStatistics_EvenCount_ReturnsCorrectMedian()
    {
        // Arrange
        // Значения: 10, 40, 20, 30 -> Сортировка: 10, 20, 30, 40 -> Медиана: (20+30)/2 = 25
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = DateTime.MinValue, Value = 10 },
            new ValueRecord { Date = DateTime.MinValue, Value = 40 },
            new ValueRecord { Date = DateTime.MinValue, Value = 20 },
            new ValueRecord { Date = DateTime.MinValue, Value = 30 }
        };

        // Act
        var result = _calculator.CalculateStatistics(values, "test.csv");

        // Assert
        Assert.Equal(25, result.MedianValue);
        Assert.Equal(25, result.AvgValue);
    }

    [Fact]
    public void CalculateStatistics_TimeDelta_ReturnsCorrectDifference()
    {
        // Arrange
        var start = new DateTime(2023, 1, 1, 10, 0, 0);
        var end = start.AddSeconds(100);
        
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = start, Value = 10 },
            new ValueRecord { Date = end, Value = 20 }
        };

        // Act
        var result = _calculator.CalculateStatistics(values, "test.csv");

        // Assert
        Assert.Equal(100, result.TimeDelta);
        Assert.Equal(start, result.MinDate);
    }
}
