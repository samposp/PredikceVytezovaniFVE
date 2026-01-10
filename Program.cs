using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Hubs;
using PredikceVytìžováníFVE.Services;
using EasyCronJob.Core;

// Early init of NLog to allow startup and exception logging, before host is built
var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables().Build();
var logger = LogManager.Setup()
                       .LoadConfigurationFromSection(config)
                       .GetCurrentClassLogger();
logger.Info("init main");
try
{

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

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
    builder.Services.AddSingleton<PredicitonService>();
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
builder.Services.AddDbContext<FVEDbContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHostedService<MqttBackgroundTask>();
builder.Services.AddHostedService<SchedulerBackgoundService>();
builder.Services.AddScoped<IScopedSchedulerService, ScopedSchedulerService>();


    var app = builder.Build();

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
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