using Microsoft.EntityFrameworkCore;
using Polly;
using PdfProcessor.API.Infrastructure;
using PdfProcessor.API.Services;
using PdfProcessor.API.Settings;
using PdfProcessor.Data;
using PdfProcessor.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("StorageSettings"));
builder.Services.AddDbContext<PdfProcessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<IPdfDocumentRepository, PdfDocumentRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<OutboxWorker>();

var app = builder.Build();

var storageSettings = builder.Configuration.GetSection("StorageSettings").Get<StorageSettings>();
if (string.IsNullOrWhiteSpace(storageSettings?.UploadPdfPath))
    throw new InvalidOperationException("UploadPdfPath configuration is invalid.");
Directory.CreateDirectory(storageSettings.UploadPdfPath);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PdfProcessorDbContext>();
    await Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
        .ExecuteAsync(() => db.Database.MigrateAsync());
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();