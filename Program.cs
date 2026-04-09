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

    builder.Services.AddControllers();
    builder.Services.AddOpenApiDocument();
    builder.Services.AddRazorPages();
    builder.Services.AddSignalR();
    builder.Services.AddDbContext<FVEDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    });
    builder.Services.AddHostedService<MqttBackgroundTask>();
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
    builder.Services.AddOpenApiDocument();
    builder.Services.AddRazorPages();
    builder.Services.AddSignalR();
    builder.Services.AddDbContext<FVEDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    var app = builder.Build();


    app.UseHttpsRedirection();
    app.UseRouting();
    //app.UseAuthorization();
    app.MapControllers();

    if (app.Environment.IsDevelopment())
    {
        // Add OpenAPI 3.0 document serving middleware
        // Available at: http://localhost:<port>/swagger/v1/swagger.json
        app.UseOpenApi();

        // Add web UIs to interact with the document
        // Available at: http://localhost:<port>/swagger
        app.UseSwaggerUi(); // UseSwaggerUI Protected by if (env.IsDevelopment())
    }

    app.UseStaticFiles();
    app.MapRazorPages();
    app.MapHub<MqttHub>("/mqtthub");

    using (var serviceScope = app.Services.CreateScope())
    {
        var context = serviceScope.ServiceProvider.GetRequiredService<FVEDbContext>();
        context.Database.Migrate();

        var needsBackfill = await context.Database.SqlQueryRaw<int>("""
                SELECT 1
                FROM MqttData
                WHERE DateTime IS NULL
                   OR trim(DateTime) = ''
                   OR DateTime = '0001-01-01 00:00:00'
                LIMIT 1
            """).AnyAsync();

        if (needsBackfill)
        {
            await context.Database.ExecuteSqlRawAsync("""
            UPDATE MqttData
            SET DateTime = datetime(
                substr(Date, 7, 4) || '-' ||
                substr(Date, 4, 2) || '-' ||
                substr(Date, 1, 2) || ' ' ||
                Time
            )
            WHERE (DateTime IS NULL
                OR DateTime = '0001-01-01 00:00:00'
                OR trim(DateTime) = '')
              AND Date IS NOT NULL
              AND Time IS NOT NULL
              AND length(Date) = 10
              AND length(Time) = 8;
        """);
        }
    }

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