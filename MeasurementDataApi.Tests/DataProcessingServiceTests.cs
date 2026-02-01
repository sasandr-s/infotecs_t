using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeasurementDataApi.Services;
using MeasurementDataApi.Services.Parsing;
using MeasurementDataApi.Services.Validation;
using MeasurementDataApi.Services.Statistics;
using MeasurementDataApi.Repositories;
using MeasurementDataApi.Exceptions;
using MeasurementDataApi.Models;

namespace MeasurementDataApi.Tests;

public class DataProcessingServiceTests
{
    private readonly Mock<IDataRepository> _repositoryMock;
    private readonly Mock<IValueValidator> _validatorMock;
    private readonly Mock<IStatisticsCalculator> _calculatorMock;
    private readonly Mock<ILogger<DataProcessingService>> _loggerMock;
    private readonly Mock<IFileParser> _csvParserMock;
    private readonly DataProcessingService _service;

    public DataProcessingServiceTests()
    {
        // Настройка моков (имитаций) зависимостей
        _repositoryMock = new Mock<IDataRepository>();
        _validatorMock = new Mock<IValueValidator>();
        _calculatorMock = new Mock<IStatisticsCalculator>();
        _loggerMock = new Mock<ILogger<DataProcessingService>>();
        
        // Настройка парсера для поддержки .csv
        _csvParserMock = new Mock<IFileParser>();
        _csvParserMock.Setup(p => p.SupportedExtensions).Returns(new[] { ".csv" });

        var parsers = new List<IFileParser> { _csvParserMock.Object };

        _service = new DataProcessingService(
            _repositoryMock.Object,
            parsers,
            _validatorMock.Object,
            _calculatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ProcessFileAsync_ValidFile_SavesData()
    {
        // Arrange
        var fileMock = CreateFileMock("test.csv");
        var values = new List<ValueRecord> { new ValueRecord { Date = DateTime.UtcNow, Value = 10 } };
        var resultRecord = new ResultRecord { FileName = "test.csv" };

        // Настраиваем успешный парсинг без ошибок
        _csvParserMock.Setup(p => p.ParseFileAsync(It.IsAny<IFormFile>(), "test.csv"))
            .ReturnsAsync((values, new List<string>())); 

        // Настраиваем успешную валидацию и расчет статистики
        _validatorMock.Setup(v => v.ValidateValues(values)).Returns(new List<string>()); 
        _calculatorMock.Setup(c => c.CalculateStatistics(values, "test.csv")).Returns(resultRecord);

        // Act
        var result = await _service.ProcessFileAsync(fileMock.Object);

        // Assert
        Assert.Equal(1, result);
        
        // Проверяем, что метод сохранения в БД был вызван 1 раз с правильными параметрами
        _repositoryMock.Verify(r => r.SaveDataAsync("test.csv", values, resultRecord), Times.Once);
        
        // Проверяем вызовы валидатора
        _validatorMock.Verify(v => v.ValidateFile(fileMock.Object), Times.Once);
        _validatorMock.Verify(v => v.ValidateRowCount(1), Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_UnsupportedExtension_ThrowsValidationException()
    {
        // Arrange
        var fileMock = CreateFileMock("test.txt"); // Расширение .txt не поддерживается

        // Act & Assert
        // Ожидаем исключение ValidationException
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.ProcessFileAsync(fileMock.Object));
        Assert.Contains("не поддерживается", ex.Errors[0]);
    }

    [Fact]
    public async Task ProcessFileAsync_ParsingErrors_ThrowsValidationException()
    {
        // Arrange
        var fileMock = CreateFileMock("test.csv");
        var values = new List<ValueRecord>();
        var parsingErrors = new List<string> { "Ошибка парсинга строки 1" };

        // Парсер возвращает ошибки
        _csvParserMock.Setup(p => p.ParseFileAsync(It.IsAny<IFormFile>(), "test.csv"))
            .ReturnsAsync((values, parsingErrors));

        _validatorMock.Setup(v => v.ValidateValues(values)).Returns(new List<string>());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.ProcessFileAsync(fileMock.Object));
        Assert.Single(ex.Errors);
        Assert.Equal("Ошибка парсинга строки 1", ex.Errors[0]);
        
        // БД НЕ должна быть вызвана
        _repositoryMock.Verify(r => r.SaveDataAsync(It.IsAny<string>(), It.IsAny<List<ValueRecord>>(), It.IsAny<ResultRecord>()), Times.Never);
    }

    [Fact]
    public async Task ProcessFileAsync_ValidationErrors_ThrowsValidationException()
    {
        // Arrange
        var fileMock = CreateFileMock("test.csv");
        var values = new List<ValueRecord> { new ValueRecord() };
        
        _csvParserMock.Setup(p => p.ParseFileAsync(It.IsAny<IFormFile>(), "test.csv"))
            .ReturnsAsync((values, new List<string>()));

        // Валидатор возвращает ошибки бизнес-логики
        var validationErrors = new List<string> { "Значение слишком низкое" };
        _validatorMock.Setup(v => v.ValidateValues(values)).Returns(validationErrors);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.ProcessFileAsync(fileMock.Object));
        Assert.Single(ex.Errors);
        Assert.Equal("Значение слишком низкое", ex.Errors[0]);

        // БД НЕ должна быть вызвана
        _repositoryMock.Verify(r => r.SaveDataAsync(It.IsAny<string>(), It.IsAny<List<ValueRecord>>(), It.IsAny<ResultRecord>()), Times.Never);
    }
    
    [Fact]
    public async Task ProcessFileAsync_WaitStatistics_VerifyPipeline()
    {
        // Тест проверяет, что все шаги вызываются в правильном порядке
         // Arrange
        var fileMock = CreateFileMock("test.csv");
        var values = new List<ValueRecord> { new ValueRecord() };
        var stats = new ResultRecord();

        _csvParserMock.Setup(p => p.ParseFileAsync(It.IsAny<IFormFile>(), "test.csv"))
            .ReturnsAsync((values, new List<string>()));
        
        _validatorMock.Setup(v => v.ValidateValues(values)).Returns(new List<string>());
        _calculatorMock.Setup(c => c.CalculateStatistics(values, "test.csv")).Returns(stats);

        // Act
        await _service.ProcessFileAsync(fileMock.Object);

        // Assert
        _validatorMock.Verify(v => v.ValidateFile(fileMock.Object));        // 1. Проверка файла
        _csvParserMock.Verify(p => p.ParseFileAsync(fileMock.Object, "test.csv")); // 2. Парсинг
        _validatorMock.Verify(v => v.ValidateValues(values));               // 3. Валидация значений
        _validatorMock.Verify(v => v.ValidateRowCount(1));                  // 4. Валидация количества
        _calculatorMock.Verify(c => c.CalculateStatistics(values, "test.csv")); // 5. Расчет статистики
        _repositoryMock.Verify(r => r.SaveDataAsync("test.csv", values, stats)); // 6. Сохранение
    }


    private Mock<IFormFile> CreateFileMock(string fileName)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(100);
        return fileMock;
    }
}
