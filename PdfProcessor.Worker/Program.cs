using Microsoft.EntityFrameworkCore;
using PdfProcessor.Data;
using PdfProcessor.Worker;
using PdfProcessor.Worker.Services;
using PdfProcessor.Worker.Settings;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("StorageSettings"));

builder.Services.AddDbContext<PdfProcessorDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();