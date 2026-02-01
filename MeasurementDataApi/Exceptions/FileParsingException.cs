namespace MeasurementDataApi.Exceptions;

public class FileParsingException : Exception
{
    public List<string> Errors { get; }

    public FileParsingException(List<string> errors) 
        : base("Ошибка при парсинге файла.")
    {
        Errors = errors;
    }
}
