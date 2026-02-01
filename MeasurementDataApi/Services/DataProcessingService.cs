using MeasurementDataApi.Exceptions;
using MeasurementDataApi.Models;
using MeasurementDataApi.Repositories;
using MeasurementDataApi.Services.Parsing;
using MeasurementDataApi.Services.Validation;
using MeasurementDataApi.Services.Statistics;

namespace MeasurementDataApi.Services;

/// <summary>
/// Сервис-оркестратор для обработки файлов с измерениями.
/// Координирует работу парсера, валидатора, калькулятора статистики и репозитория.
/// </summary>
public class DataProcessingService : IDataProcessingService
{
    private readonly IDataRepository _repository;
    private readonly IEnumerable<IFileParser> _parsers; // Все зарегистрированные парсеры
    private readonly IValueValidator _validator;
    private readonly IStatisticsCalculator _calculator;
    private readonly ILogger<DataProcessingService> _logger;

    public DataProcessingService(
        IDataRepository repository,
        IEnumerable<IFileParser> parsers,
        IValueValidator validator,
        IStatisticsCalculator calculator,
        ILogger<DataProcessingService> logger)
    {
        _repository = repository;
        _parsers = parsers;
        _validator = validator;
        _calculator = calculator;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> ProcessFileAsync(IFormFile file)
    {
        var swTotal = System.Diagnostics.Stopwatch.StartNew();
        var swStep = new System.Diagnostics.Stopwatch();
        
        // 1. Предварительная проверка
        _validator.ValidateFile(file);

        var fileName = file.FileName;
        
        // 2. Парсинг
        // Находим подходящий парсер по расширению файла (Strategy Pattern)
        var extension = Path.GetExtension(fileName).ToLower();
        var parser = _parsers.FirstOrDefault(p => p.SupportedExtensions.Contains(extension));

        if (parser == null)
        {
            throw new ValidationException(new List<string> { $"Формат файла '{extension}' не поддерживается." });
        }

        swStep.Start();
        var (values, parsingErrors) = await parser.ParseFileAsync(file, fileName);
        swStep.Stop();
        var parseMs = swStep.ElapsedMilliseconds;

        // 3. Бизнес-валидация (валидируем те строки, которые удалось распарсить)
        swStep.Restart();
        var validationErrors = _validator.ValidateValues(values);
        swStep.Stop();
        var validationMs = swStep.ElapsedMilliseconds;

        // 4. Проверка общего ограничения (согласно ТЗ: от 1 до 10 000)
        // Если парсинг прошел полностью, но строк 0 или >10000 - это ошибка.
        // Если были ошибки парсинга, итоговое количество строк тоже может быть неверным.
        var totalCount = values.Count;
        
        // Собираем и сортируем все ошибки по номеру строки (чтобы ошибки парсинга и валидации шли по порядку)
        var allErrors = parsingErrors.Concat(validationErrors)
            .OrderBy(e => {
                var match = System.Text.RegularExpressions.Regex.Match(e, @"Строка (\d+):");
                return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
            })
            .ToList();

        // Если есть ошибки парсинга или валидации
        if (allErrors.Count > 0)
        {
            _logger.LogWarning("Файл {FileName} содержит ошибки ({Count} шт). Обработка прервана. Время: Парсинг={P}мс, Валидация={V}мс.", 
                fileName, allErrors.Count, parseMs, validationMs);
            
            // Выбрасываем ValidationException, так как контроллер умеет его красиво отдавать
            throw new ValidationException(allErrors);
        }

        // 5. Проверка количества строк (только если нет ошибок в самих строках)
        _validator.ValidateRowCount(totalCount);

        // 6. Расчёт статистики
        swStep.Restart();
        var result = _calculator.CalculateStatistics(values, fileName);
        swStep.Stop();
        var statsMs = swStep.ElapsedMilliseconds;

        // 7. Сохранение в БД
        swStep.Restart();
        await _repository.SaveDataAsync(fileName, values, result);
        swStep.Stop();
        var dbMs = swStep.ElapsedMilliseconds;

        swTotal.Stop();
        _logger.LogInformation(
            "Файл {FileName} успешно обработан за {TotalMs} мс. Детали: Парсинг={P}мс, Валидация={V}мс, Статистика={S}мс, БД={D}мс. Записей: {Count}.",
            fileName, swTotal.ElapsedMilliseconds, parseMs, validationMs, statsMs, dbMs, totalCount);

        return values.Count;
    }
}
