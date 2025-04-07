using PredikceVytìžováníFVE.Data;
using PredikceVytìžováníFVE.Hubs;
using Microsoft.EntityFrameworkCore;
using PredikceVytìžováníFVE.Services;

var builder = WebApplication.CreateBuilder(args);

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
