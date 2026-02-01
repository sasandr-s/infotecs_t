using Microsoft.EntityFrameworkCore;
using Npgsql;
using MeasurementDataApi.Data;
using MeasurementDataApi.Models;
using MeasurementDataApi.Models.Dto;

namespace MeasurementDataApi.Repositories;

/// <summary>
/// Репозиторий для изоляции работы с базой данных PostgreSQL.
/// </summary>
public class DataRepository : IDataRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataRepository> _logger;

    public DataRepository(AppDbContext context, ILogger<DataRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SaveDataAsync(string fileName, List<ValueRecord> values, ResultRecord result)
    {
        // Получаем низкоуровневое соединение Npgsql из DbContext
        var connection = (NpgsqlConnection)_context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();

        // Использую явную транзакцию для обеспечения атомарности (удаление + вставка)
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            // 1. Очистка старых данных для этого файла
            using (var deleteCmd = new NpgsqlCommand(@"
                DELETE FROM ""Results"" WHERE ""FileName"" = @fn;
                DELETE FROM ""Values"" WHERE ""FileName"" = @fn;", connection, transaction))
            {
                deleteCmd.Parameters.AddWithValue("fn", fileName);
                await deleteCmd.ExecuteNonQueryAsync();
            }

            // 2. Бинарная вставка данных
            // Использую протокол COPY через NpgsqlBinaryImporter
            await using (var writer = await connection.BeginBinaryImportAsync(
                @"COPY ""Values"" (""FileName"", ""Date"", ""ExecutionTime"", ""Value"") FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var v in values)
                {
                    await writer.StartRowAsync();
                    await writer.WriteAsync(v.FileName, NpgsqlTypes.NpgsqlDbType.Varchar);
                    await writer.WriteAsync(v.Date, NpgsqlTypes.NpgsqlDbType.TimestampTz);
                    await writer.WriteAsync(v.ExecutionTime, NpgsqlTypes.NpgsqlDbType.Double);
                    await writer.WriteAsync(v.Value, NpgsqlTypes.NpgsqlDbType.Double);
                }
                await writer.CompleteAsync();
            }

            // 3. Сохранение итогов в таблицу Results
            using (var resultCmd = new NpgsqlCommand(@"
                INSERT INTO ""Results"" (""FileName"", ""TimeDelta"", ""MinDate"", ""AvgExecutionTime"", ""AvgValue"", ""MedianValue"", ""MaxValue"", ""MinValue"")
                VALUES (@fn, @td, @md, @aet, @av, @med, @max, @min);", connection, transaction))
            {
                resultCmd.Parameters.AddWithValue("fn", fileName);
                resultCmd.Parameters.AddWithValue("td", result.TimeDelta);
                resultCmd.Parameters.AddWithValue("md", result.MinDate);
                resultCmd.Parameters.AddWithValue("aet", result.AvgExecutionTime);
                resultCmd.Parameters.AddWithValue("av", result.AvgValue);
                resultCmd.Parameters.AddWithValue("med", result.MedianValue);
                resultCmd.Parameters.AddWithValue("max", result.MaxValue);
                resultCmd.Parameters.AddWithValue("min", result.MinValue);
                await resultCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Ошибка в репозитории при бинарном сохранении данных файла {FileName}.", fileName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ResultRecord>> GetResultsAsync(ResultFilter filter)
    {
        var query = _context.Results.AsQueryable();

        if (!string.IsNullOrEmpty(filter.FileName))
        {
            query = query.Where(r => r.FileName == filter.FileName);
        }

        if (filter.MinDateFrom.HasValue) query = query.Where(r => r.MinDate >= filter.MinDateFrom.Value);
        if (filter.MinDateTo.HasValue) query = query.Where(r => r.MinDate <= filter.MinDateTo.Value);
        
        if (filter.AvgValueFrom.HasValue) query = query.Where(r => r.AvgValue >= filter.AvgValueFrom.Value);
        if (filter.AvgValueTo.HasValue) query = query.Where(r => r.AvgValue <= filter.AvgValueTo.Value);
        
        if (filter.AvgExecTimeFrom.HasValue) query = query.Where(r => r.AvgExecutionTime >= filter.AvgExecTimeFrom.Value);
        if (filter.AvgExecTimeTo.HasValue) query = query.Where(r => r.AvgExecutionTime <= filter.AvgExecTimeTo.Value);

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ValueRecord>> GetLast10ValuesAsync(string fileName)
    {
        return await _context.Values
            .Where(v => v.FileName == fileName)
            .OrderByDescending(v => v.Date)
            .Take(10)
            .ToListAsync();
    }
}
