using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;
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

                string dateTime = mqttMessage.Date + " " + mqttMessage.Time;

                using IServiceScope scope = _serviceProvider.CreateScope();
                FVEDbContext _db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();

                bool entryExists = _db.mqttBos.Any(x => x.DateTime.Equals(dateTime));

                if (!entryExists) {
                    _logger.LogInformation($"Saving mqttMessage: {message}");
                    _db.Add(ToMqttBo(mqttMessage));
                    _db.SaveChangesAsync();
                    _hubContext.Clients.All.SendAsync("ReceiveMqtt", ToFVEData(mqttMessage));
                }
                return Task.CompletedTask;
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await mqttService.DisConnect();
        }
 

        private FVEData ToFVEData(MqttData message) {
            return new() {
                BatteryPercentage = message.SoC,
                BatteryOutput = message.P_Batt,
                PVOutput = message.P_Inv,
                PVEnergyCumulative = message.PVenergy,
                OutputCumulative = message.Output,
                InputCumulative = message.Consumption,
                Timestamp = DateTime.Parse(message.Date + " " + message.Time)
            };
        }
        private MqttBo ToMqttBo(MqttData data) {
            return new() {
                SoC = data.SoC,
                P_PV = data.P_PV,
                P_Inv = data.P_Inv,
                P_OnGr = data.P_OnGr,
                P_OffGr = data.P_OffGr,
                P_Grid = data.P_Grid,
                P_Batt = data.P_Batt,
                PVenergy = data.PVenergy,
                Charge = data.Charge,
                Discharge = data.Discharge,
                Feed = data.Feed,
                Consumption = data.Consumption,
                Output = data.Output,
                Input = data.Input,
                Load = data.Load,
                DateTime = data.Date + " " + data.Time,
            };
        }
    }
}
