using Microsoft.EntityFrameworkCore;
using MeasurementDataApi.Data;

namespace MeasurementDataApi.Extensions;

/// <summary>
/// Расширения для прогрева БД при старте приложения.
/// </summary>
public static class DbWarmupExtensions
{
    /// <summary>
    /// Выполняет прогрев EF Core и JIT для оптимизации первого запроса.
    /// </summary>
    public static async Task WarmupDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Автоматически применяем миграции при старте
        await context.Database.MigrateAsync();

        // 2. Прогреваем модель
        await context.Values.OrderBy(v => v.Id).Select(v => v.Id).FirstOrDefaultAsync();

        // 3. Прогреваем JIT для SQL с массивами и COPY
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM UNNEST({0}, {1}, {2}, {3}) LIMIT 0",
                new string[0], new DateTime[0], new double[0], new double[0]);
        }
        catch { /* Игнорируем ошибки прогрева */ }
    }
}
