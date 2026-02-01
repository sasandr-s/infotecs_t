using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MeasurementDataApi.Data;
using MeasurementDataApi.Models;
using MeasurementDataApi.Exceptions;
using MeasurementDataApi.Models.Dto;
using MeasurementDataApi.Services;
using MeasurementDataApi.Repositories;

namespace MeasurementDataApi.Controllers;

/// <summary>
/// Контроллер для работы с данными измерений и статистикой.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MeasurementDataController : ControllerBase
{
    private readonly IDataProcessingService _dataService;
    private readonly IDataRepository _repository;

    public MeasurementDataController(IDataProcessingService dataService, IDataRepository repository)
    {
        _dataService = dataService;
        _repository = repository;
    }

    /// <summary>
    /// Сохранение и обработка файла с данными (Метод 1).
    /// </summary>
    /// <remarks>
    /// Принимает CSV файл, валидирует данные, рассчитывает статистику и сохраняет результаты в БД.
    /// Если файл с таким именем уже существует, старые данные перезаписываются.
    /// </remarks>
    /// <param name="file">CSV файл с колонками: Date, ExecutionTime, Value.</param>
    /// <returns>Информация о количестве обработанных строк.</returns>
    /// <response code="200">Файл успешно обработан.</response>
    /// <response code="400">Ошибка валидации (неверный формат данных, превышение лимитов строк и т.д.).</response>
    /// <response code="500">Внутренняя ошибка сервера.</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var recordsCount = await _dataService.ProcessFileAsync(file);
        return Ok(new UploadResultDto
        {
            Message = "Файл успешно обработан.",
            RecordsProcessed = recordsCount,
            FileName = file.FileName
        });
    }

    /// <summary>
    /// Получение списка результатов обработки (Метод 2).
    /// </summary>
    /// <remarks>
    /// Позволяет фильтровать результаты по имени файла и диапазонам различных показателей.
    /// </remarks>
    /// <param name="filter">Набор фильтров для поиска.</param>
    /// <returns>Список записей статистики из таблицы Results.</returns>
    /// <response code="200">Список результатов, подходящих под фильтры.</response>
    [HttpGet("results")]
    [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ResultDto>>> GetResults([FromQuery] ResultFilter filter)
    {
        var results = await _repository.GetResultsAsync(filter);
        return Ok(results.Select(ResultDto.FromEntity).ToList());
    }

    /// <summary>
    /// Получение последних 10 значений для заданного файла (Метод 3).
    /// </summary>
    /// <remarks>
    /// Возвращает записи из таблицы Values, отсортированные по дате в обратном порядке.
    /// </remarks>
    /// <param name="fileName">Имя файла, для которого нужно получить данные.</param>
    /// <returns>Список из 10 последних измерений.</returns>
    /// <response code="200">Список последних 10 значений.</response>
    /// <response code="400">Параметр fileName не указан.</response>
    [HttpGet("values/last10")]
    [ProducesResponseType(typeof(List<ValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ValueDto>>> GetLast10Values([FromQuery] string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return BadRequest(new ErrorDto { Error = "Параметр fileName обязателен." });
        }

        var values = await _repository.GetLast10ValuesAsync(fileName);

        return Ok(values.Select(ValueDto.FromEntity).ToList());
    }
}
