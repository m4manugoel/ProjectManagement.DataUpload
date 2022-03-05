using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectManagement.DataUpload;
using Serilog;

var host = AppStartup();

var service = ActivatorUtilities.CreateInstance<UploadDataService>(host.Services);

await service.UploadData();

static IHost AppStartup()
{
    var configurationRoot = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configurationRoot)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

    Log.Logger.Information("Application Starting");

    return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<ITokenService, TokenService>();
                    services.AddScoped<IExcelService, ExcelService>();
                    services.AddTransient<IUploadDataService, UploadDataService>();
                    services.AddTransient<HttpClient>();
                })
                .UseSerilog()
                .Build();
}