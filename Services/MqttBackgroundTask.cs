using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models;
using System.Text.Json;

namespace PredikceVytěžováníFVE.Services {
    public class MqttBackgroundTask : IHostedService {

        private readonly IHubContext<MqttHub> _hubContext;
        private readonly MQTTService mqttService = new("147.230.76.38");
        private readonly ILogger<MqttBackgroundTask> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MqttBackgroundTask(IHubContext<MqttHub> hubContext, ILogger<MqttBackgroundTask> logger, IServiceProvider serviceProvider) {
            _hubContext = hubContext;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }


        public async Task StartAsync(CancellationToken cancellationToken) {
            await mqttService.Connect();
            await mqttService.Subscribe("FVE/Ibehej_TX", e => {
                string message = e.ApplicationMessage.ConvertPayloadToString();
                MqttData? mqttMessage = JsonSerializer.Deserialize<MqttData>(message);
                if (mqttMessage == null) {
                    _logger.LogError("Message not found");
                    return Task.CompletedTask;
                }
                using IServiceScope scope = _serviceProvider.CreateScope();
                FVEDbContext _db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();

                bool entryExists = _db.mqttData.Any(x => x.Date.Equals(mqttMessage.Date) && x.Time.Equals(mqttMessage.Time));

                if (!entryExists) {
                    _logger.LogInformation($"Saving mqttMessage: {message}");
                    _db.Add(mqttMessage);
                    _db.SaveChangesAsync();
                    _hubContext.Clients.All.SendAsync("ReceiveMqtt", ConverHelper.ToFVEData(mqttMessage));
                }
                return Task.CompletedTask;
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await mqttService.DisConnect();
        }
    }
}
