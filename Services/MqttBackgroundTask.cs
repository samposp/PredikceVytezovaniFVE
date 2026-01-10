using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using Newtonsoft.Json.Linq;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models.DB;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PredikceVytěžováníFVE.Services
{
    public class MqttBackgroundTask : IHostedService
    {

        private readonly IHubContext<MqttHub> _hubContext;
        private readonly MQTTService mqttService = new("cassandra2.tul.cz");
        private readonly ILogger<MqttBackgroundTask> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MqttBackgroundTask(IHubContext<MqttHub> hubContext, ILogger<MqttBackgroundTask> logger, IServiceProvider serviceProvider)
        {
            _hubContext = hubContext;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await mqttService.Connect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker");
                return;
            }

            try
            {
                await mqttService.Subscribe("FVE/Ibehej_TX", async e =>
                {
                    string message = e.ApplicationMessage.ConvertPayloadToString();
                    _logger.LogInformation("Messege recieved from mqtt: {message}", message);
                    var json = JsonObject.Parse(message);
                    MqttData? mqttMessage = JsonSerializer.Deserialize<MqttData>(message);
                    if (mqttMessage == null)
                    {
                        _logger.LogError("Message not found");
                        return;
                    }
                    mqttMessage.FullMessage = message;
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    FVEDbContext _db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();

                    bool entryExists = _db.MqttData.Any(x => x.Date.Equals(mqttMessage.Date) && x.Time.Equals(mqttMessage.Time));

                    if (!entryExists)
                    {
                        _logger.LogInformation($"Saving mqttMessage to DB");
                        _db.Add(mqttMessage);
                        await _db.SaveChangesAsync();
                        await _hubContext.Clients.All.SendAsync("ReceiveMqtt", ConverterHelper.ToFVEData(mqttMessage));
                    }
                    else
                    {
                        _logger.LogInformation($"Entry for {mqttMessage.Date} {mqttMessage.Time} already exists. Skipping save.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to MQTT topic");
                return;
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await mqttService.Disconnect();
        }
    }
}
