using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using MeasurementDataApi.Exceptions;
using MeasurementDataApi.Models;
using MeasurementDataApi.Services.Validation;

namespace MeasurementDataApi.Tests;

public class ValueValidatorTests
{
    private readonly Mock<IOptions<FileValidationSettings>> _optionsMock;
    private readonly FileValidationSettings _settings;
    private readonly ValueValidator _validator;

    public ValueValidatorTests()
    {
        _settings = new FileValidationSettings
        {
            MinAllowedYear = 2000,
            MinRowCount = 1,
            MaxRowCount = 10000,
            AllowNegativeExecutionTime = false,
            AllowNegativeValue = false
        };
        _optionsMock = new Mock<IOptions<FileValidationSettings>>();
        _optionsMock.Setup(o => o.Value).Returns(_settings);

        _validator = new ValueValidator(_optionsMock.Object);
    }

    [Fact]
    public void ValidateFile_NullOrEmpty_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<ValidationException>(() => _validator.ValidateFile(null!));

        var emptyFile = new Mock<IFormFile>();
        emptyFile.Setup(f => f.Length).Returns(0);
        Assert.Throws<ValidationException>(() => _validator.ValidateFile(emptyFile.Object));
    }

    [Fact]
    public void ValidateRowCount_TooFewOrTooMany_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<ValidationException>(() => _validator.ValidateRowCount(0));     // Низкая граница
        Assert.Throws<ValidationException>(() => _validator.ValidateRowCount(10001)); // Высокая граница
        
        // Валидные значения (не должны вызывать ошибку)
        _validator.ValidateRowCount(1);
        _validator.ValidateRowCount(10000);
    }

    [Fact]
    public void ValidateValues_InvalidDate_ReturnsError()
    {
        // Arrange 
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = new DateTime(1999, 12, 31, 0, 0, 0, DateTimeKind.Utc), LineNumber = 1 }
        };

        // Act
        var errors = _validator.ValidateValues(values);

        // Assert
        Assert.Single(errors);
        Assert.Contains("вне допустимого диапазона", errors[0]);
    }

    [Fact]
    public void ValidateValues_NegativeExecutionTime_ReturnsError()
    {
        // Arrange
        // Настройки по умолчанию: AllowNegativeExecutionTime = false
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = DateTime.UtcNow, ExecutionTime = -1, LineNumber = 1 }
        };

        // Act
        var errors = _validator.ValidateValues(values);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Время выполнения не может быть отрицательным", errors[0]);
    }

    [Fact]
    public void ValidateValues_NegativeValue_ReturnsError()
    {
        // Arrange
        // Настройки по умолчанию: AllowNegativeValue = false
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = DateTime.UtcNow, Value = -1, LineNumber = 1 }
        };

        // Act
        var errors = _validator.ValidateValues(values);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Значение показателя не может быть отрицательным", errors[0]);
    }

    [Fact]
    public void ValidateValues_ValidData_ReturnsEmptyErrors()
    {
        // Arrange
        var values = new List<ValueRecord>
        {
            new ValueRecord { Date = DateTime.UtcNow, Value = 10, ExecutionTime = 10, LineNumber = 1 }
        };

        // Act
        var errors = _validator.ValidateValues(values);

        // Assert
        Assert.Empty(errors);
    }
}
