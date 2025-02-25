using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Client;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Services;
using MQTTnet;
using Microsoft.AspNetCore.SignalR;
using PredikceVytěžováníFVE.Hubs;

namespace PredikceVytěžováníFVE.Pages
{
    public class FVEModel : PageModel
    {
        private readonly IHubContext<MqttHub> _hubContext;

        public FVEModel(IHubContext<MqttHub> hubContext) {
            _hubContext = hubContext;

        }

        public FVEData FVEData { get; set; } = new();

        readonly private MQTTService mqttService = new("147.230.76.38");

        public string text { get; set; } = "test";

        public async Task OnGet()
        {
            FVEData = new() {
                BatteryPercentage = 60,
                BatteryOutput = 23,
                PVOutput = 45,
                PVEnergyCumulative = 23,
                OutputCumulative = 54,
                InputCumulative = 45,
                Timestamp = DateTime.UtcNow
            };

            await mqttService.Connect();
            await mqttService.Subscribe("FVE/Ibehej_TX", e => {
                text = e.ApplicationMessage.ConvertPayloadToString();
                Console.WriteLine($"Received message: {text}");
                _hubContext.Clients.All.SendAsync("ReceiveMqtt", text);
                return Task.CompletedTask;
            });
        }
    }
}
