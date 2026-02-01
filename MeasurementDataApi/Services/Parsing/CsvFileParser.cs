using System.Globalization;
using MeasurementDataApi.Models;

namespace MeasurementDataApi.Services.Parsing;

/// <summary>
/// Реализация парсера для CSV-файлов.
/// </summary>
public class CsvFileParser : IFileParser
{
    private readonly ILogger<CsvFileParser> _logger;

    public CsvFileParser(ILogger<CsvFileParser> logger)
    {
        _logger = logger;
    }

    public IEnumerable<string> SupportedExtensions => new[] { ".csv" };

    /// <inheritdoc/>
    public async Task<(List<ValueRecord> Values, List<string> ParsingErrors)> ParseFileAsync(IFormFile file, string fileName)
    {
        var values = new List<ValueRecord>();
        var parsingErrors = new List<string>();

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        // Пропускаем заголовок
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            parsingErrors.Add("Файл пустой (отсутствует заголовок).");
            return (values, parsingErrors);
        }

        int lineNumber = 1;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parseResult = ParseLine(line, lineNumber, fileName);

            if (parseResult.Error != null)
            {
                parsingErrors.Add(parseResult.Error);
                continue;
            }

            values.Add(parseResult.Record!);
        }

        return (values, parsingErrors);
    }

    /// <summary>
    /// Парсит одну строку CSV и возвращает запись или ошибку.
    /// Оптимизировано для минимальных аллокаций (без Split).
    /// </summary>
    private static (ValueRecord? Record, string? Error) ParseLine(string line, int lineNumber, string fileName)
    {
        var span = line.AsSpan();
        
        // Поиск индексов разделителей ';'
        int firstSemi = span.IndexOf(';');
        if (firstSemi == -1) return (null, $"Строка {lineNumber}: Неверный формат (отсутствует разделитель).");
        
        int secondSemi = span.Slice(firstSemi + 1).IndexOf(';');
        if (secondSemi == -1) return (null, $"Строка {lineNumber}: Неверный формат (отсутствует второй разделитель).");
        secondSemi += firstSemi + 1;

        // Извлечение частей без создания промежуточных строк (где возможно)
        var datePart = span.Slice(0, firstSemi).Trim();
        var execPart = span.Slice(firstSemi + 1, secondSemi - firstSemi - 1).Trim();
        var valuePart = span.Slice(secondSemi + 1).Trim();

        // Парсинг даты
        if (!TryParseDate(datePart.ToString(), out DateTime date))
        {
            return (null, $"Строка {lineNumber}: Неверный формат даты '{datePart.ToString()}'.");
        }

        // Парсинг чисел
        if (!double.TryParse(execPart, NumberStyles.Any, CultureInfo.InvariantCulture, out double execTime))
        {
            return (null, $"Строка {lineNumber}: Неверный формат времени выполнения '{execPart.ToString()}'.");
        }

        if (!double.TryParse(valuePart, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
        {
            return (null, $"Строка {lineNumber}: Неверный формат значения '{valuePart.ToString()}'.");
        }

        return (new ValueRecord
        {
            FileName = fileName,
            LineNumber = lineNumber,
            Date = date.ToUniversalTime(),
            ExecutionTime = execTime,
            Value = value
        }, null);
    }

    private static readonly string[] _dateFormats = 
    { 
        "yyyy-MM-ddTHH-mm-ss.fffffffZ", 
        "yyyy-MM-ddTHH-mm-ss.ffffffZ", 
        "yyyy-MM-ddTHH-mm-ss.fffffZ",
        "yyyy-MM-ddTHH-mm-ss.ffffZ",
        "yyyy-MM-ddTHH-mm-ss.fffZ",
        "yyyy-MM-ddTHH-mm-ss.ffZ",
        "yyyy-MM-ddTHH-mm-ss.fZ",
        "yyyy-MM-ddTHH-mm-ssZ",
        "yyyy-MM-ddTHH:mm:ss.fffffffZ", 
        "yyyy-MM-ddTHH:mm:ss.ffffffZ", 
        "yyyy-MM-ddTHH:mm:ss.fffffZ",
        "yyyy-MM-ddTHH:mm:ss.ffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ss.ffZ",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:ssZ"
    };

    /// <summary>
    /// Пытается распознать дату в нескольких форматах с дефисами в времени (согласно ТЗ).
    /// </summary>
    private static bool TryParseDate(string input, out DateTime date)
    {
        return DateTime.TryParseExact(input, _dateFormats, 
            CultureInfo.InvariantCulture, 
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, 
            out date);
    }
}
