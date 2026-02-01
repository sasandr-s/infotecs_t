using MeasurementDataApi.Models;

namespace MeasurementDataApi.Services.Parsing;

/// <summary>
/// Интерфейс для парсинга файлов с данными измерений.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Парсит файл и возвращает сырые данные или ошибки формата.
    /// </summary>
    /// <param name="file">Загруженный файл.</param>
    /// <param name="fileName">Имя файла (для логирования).</param>
    /// <returns>Кортеж: Список разобранных записей и список ошибок парсинга.</returns>
    Task<(List<ValueRecord> Values, List<string> ParsingErrors)> ParseFileAsync(IFormFile file, string fileName);
    
    /// <summary>
    /// Поддерживаемые расширения файлов (например, ".csv").
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }
}
