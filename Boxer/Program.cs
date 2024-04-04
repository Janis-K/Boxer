using Boxer.Infrastructure;
using Boxer.Interfaces;
using Boxer.Models.Configuration;
using Boxer.Repository;
using Boxer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BoxAppSettings>(builder.Configuration.GetSection("BoxAppSettings"));

builder.Services.AddTransient<IFileProcessingService, FileProcessingService>();
builder.Services.AddTransient<IBoxRepository, BoxRepository>();

builder.Services.AddHostedService<MonitoringService>();

builder.Services.AddDbContext<BoxDbContext>(options => 
{
    options.UseInMemoryDatabase("boxes");
});

builder.Logging.AddConsole();

var app = builder.Build();

app.Run();