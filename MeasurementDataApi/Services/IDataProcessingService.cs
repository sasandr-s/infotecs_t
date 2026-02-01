namespace MeasurementDataApi.Services;

/// <summary>
/// Интерфейс сервиса обработки файлов с данными измерений.
/// </summary>
public interface IDataProcessingService
{
    /// <summary>
    /// Обрабатывает файл и возвращает количество обработанных записей.
    /// </summary>
    /// <param name="file">Загруженный файл.</param>
    /// <returns>Количество успешно обработанных записей.</returns>
    Task<int> ProcessFileAsync(IFormFile file);
}
