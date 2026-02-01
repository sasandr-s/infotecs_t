using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MeasurementDataApi.Services.Parsing;

namespace MeasurementDataApi.Tests;

public class CsvFileParserTests
{
    private readonly Mock<ILogger<CsvFileParser>> _loggerMock;
    private readonly CsvFileParser _parser;
    private readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");

    public CsvFileParserTests()
    {
        _loggerMock = new Mock<ILogger<CsvFileParser>>();
        _parser = new CsvFileParser(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseFileAsync_ValidCsvFile_ReturnsValues()
    {
        // Arrange
        var filePath = Path.Combine(TestDataPath, "valid_data.csv");
        var content = await File.ReadAllTextAsync(filePath);
        var fileMock = CreateFormFileMock("valid_data.csv", content);

        // Act
        var result = await _parser.ParseFileAsync(fileMock.Object, "valid_data.csv");

        // Assert
        Assert.Empty(result.ParsingErrors);
        Assert.Equal(3, result.Values.Count); // 3 строки в valid_data.csv
        Assert.Equal(0.1, result.Values[0].ExecutionTime);
        Assert.Equal(100.0, result.Values[0].Value);
        Assert.Equal(new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc), result.Values[0].Date);
    }

    [Fact]
    public async Task ParseFileAsync_InvalidCsvFile_ReturnsErrors()
    {
        // Arrange
        var filePath = Path.Combine(TestDataPath, "invalid_data.csv");
        var content = await File.ReadAllTextAsync(filePath);
        var fileMock = CreateFormFileMock("invalid_data.csv", content);

        // Act
        var result = await _parser.ParseFileAsync(fileMock.Object, "invalid_data.csv");

        // Assert
        // Ожидаем 1 ошибку парсинга (строка с "not_a_number")
        // Строка с "-1.0" является валидным CSV (число), хоть и невалидна логически (это проверит валидатор позже)
        Assert.Single(result.ParsingErrors); 
        Assert.Contains("Неверный формат времени выполнения", result.ParsingErrors[0]);

        // Ожидаем 1 успешно распаршенную запись (строка с "-1.0")
        Assert.Single(result.Values);
        Assert.Equal(-1.0, result.Values[0].ExecutionTime);
    }

    [Fact]
    public async Task ParseFileAsync_BadHeader_ParsesSuccessfully()
    {
        // Arrange
        // Текущая реализация парсера игнорирует содержимое заголовка, проверяя только структуру данных
        var filePath = Path.Combine(TestDataPath, "bad_header.csv");
        var content = await File.ReadAllTextAsync(filePath);
        var fileMock = CreateFormFileMock("bad_header.csv", content);

        // Act
        var result = await _parser.ParseFileAsync(fileMock.Object, "bad_header.csv");

        // Assert
        // Ошибок быть не должно, так как сами данные валидны
        Assert.Empty(result.ParsingErrors);
        Assert.Single(result.Values);
    }
    
    [Theory]
    [InlineData("Date;ExecutionTime;Value\n2000-01-01T00:00:00Z;10;100", 1, 0)]
    [InlineData("Date;ExecutionTime;Value\nbad_date;10;100", 0, 1)]
    public async Task ParseFileAsync_InlineData_Works(string content, int expectedValues, int expectedErrors)
    {
        // Arrange
        var fileMock = CreateFormFileMock("inline.csv", content);

        // Act
        var result = await _parser.ParseFileAsync(fileMock.Object, "inline.csv");

        // Assert
        Assert.Equal(expectedValues, result.Values.Count);
        Assert.Equal(expectedErrors, result.ParsingErrors.Count);
    }

    private Mock<IFormFile> CreateFormFileMock(string fileName, string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        return fileMock;
    }
}
