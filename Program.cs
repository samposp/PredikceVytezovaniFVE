using EasyCronJob.Core;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Hubs;
using PredikceVytìžováníFVE.Services;
using System.Globalization;

// Early init of NLog to allow startup and exception logging, before host is built
var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
var logger = LogManager.Setup()
                       .LoadConfigurationFromSection(config)
                       .GetCurrentClassLogger();
logger.Info("init main");
CultureInfo.CurrentCulture = new CultureInfo("cs-CZ");
CultureInfo.CurrentUICulture = new CultureInfo("cs-CZ");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddSingleton<ConfigurationService>();

    builder.Services.AddSingleton<MqttBackgroundTask>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<MqttBackgroundTask>());
    builder.Services.AddSingleton<SpotSoapService>();
    builder.Services.AddTransient<OpenMeteoService>();
    builder.Services.AddTransient<BatteryMilpOptimizationService>();
    builder.Services.AddSingleton<PredictitonService>();
    builder.Services.AddTransient<ConcumptionPredictionService>();
    builder.Services.AddTransient<PVForecastService>();
    builder.Services.AddTransient<ForecastService>();
    builder.Services.AddTransient<MqttDataService>();
    builder.Services.AddTransient<PredictionDataService>();
    builder.Services.AddTransient<EcbCurrencyConversionService>();
    builder.Services.AddTransient<SendPredictionService>();
    builder.Services.ApplyResulation<PredictionSechduleJob>(options =>
    {
        options.CronExpression = "0 23 * * *";
        options.TimeZoneInfo = TimeZoneInfo.Local;
        options.CronFormat = Cronos.CronFormat.Standard;
    });
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    builder.Services.AddSignalR();
    builder.Services.AddDbContext<FVEDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();

    app.UseStaticFiles();
    app.MapRazorPages();
    app.MapHub<MqttHub>("/mqtthub");

    using (var serviceScope = app.Services.CreateScope())
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<FVEDbContext>();
        context.Database.Migrate();
    }

    app.MapGet("/api/mqtt/status", (MqttBackgroundTask mqttTask) =>
    {
        return Results.Ok(new
        {
            connected = mqttTask.IsConnected,
            healthy = mqttTask.IsHealthy,
            lastMessageReceivedUtc = mqttTask.LastMessageReceivedUtc
        });
    });

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}