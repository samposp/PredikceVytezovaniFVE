using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Newtonsoft.Json.Linq;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PredikceVytěžováníFVE.Services
{
    public class MqttBackgroundTask : IHostedService
    {

        private readonly IHubContext<MqttHub> _hubContext;
        private readonly MQTTService mqttService;
        private readonly ILogger<MqttBackgroundTask> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string mqttTopic;

        public MqttBackgroundTask(IHubContext<MqttHub> hubContext, ILogger<MqttBackgroundTask> logger, IServiceProvider serviceProvider, ConfigurationService configuration)
        {
            _hubContext = hubContext;
            _logger = logger;
            _serviceProvider = serviceProvider;
            mqttService = new(configuration.Settings.Api.MqttBroker ?? throw new Exception("missing mqtt broker"));
            mqttTopic = configuration.Settings.Api.MqttTopic  ?? throw new Exception("missing mqtt topic");
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
                await mqttService.Subscribe(mqttTopic, async e =>
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
                    //mqttMessage.DateTime = DateTime.Parse(mqttMessage.Date + " " + mqttMessage.Time);

                    using IServiceScope scope = _serviceProvider.CreateScope();
                    MqttDataService mqttDataService = scope.ServiceProvider.GetRequiredService<MqttDataService>();
                    if (await mqttDataService.SaveMqttData(mqttMessage))
                        await _hubContext.Clients.All.SendAsync("ReceiveMqtt", ConverterHelper.ToFVEData(mqttMessage));
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
