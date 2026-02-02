using Microsoft.EntityFrameworkCore;
using MeasurementDataApi.Data;
using MeasurementDataApi.Models;
using MeasurementDataApi.Repositories;
using MeasurementDataApi.Services;
using MeasurementDataApi.Services.Parsing;
using MeasurementDataApi.Services.Validation;
using MeasurementDataApi.Services.Statistics;
using MeasurementDataApi.Middleware;
using MeasurementDataApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов в контейнер.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Measurement Data API", Version = "v1", Description = "API для обработки файлов с данными измерений и расчёта статистики" });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Регистрация глобального обработчика ошибок
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Настройки валидации
builder.Services.Configure<FileValidationSettings>(
    builder.Configuration.GetSection(FileValidationSettings.SectionName));

// Настройка DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=PostgresDb;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions => 
    {
        npgsqlOptions.MaxBatchSize(1000);
    }));

// Register Layered Architecture Services
builder.Services.AddScoped<IDataRepository, DataRepository>();
builder.Services.AddScoped<IDataProcessingService, DataProcessingService>();
builder.Services.AddScoped<IFileParser, CsvFileParser>();
builder.Services.AddScoped<IValueValidator, ValueValidator>();
builder.Services.AddScoped<IStatisticsCalculator, StatisticsCalculator>();

var app = builder.Build();

// Глобальная обработка исключений
app.UseExceptionHandler();

// Прогрев EF Core, чтобы первый запрос не тормозил 2 секунды
await app.WarmupDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", context => {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
