using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR.Client;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Services;
using MQTTnet;

namespace PredikceVytěžováníFVE.Pages
{
    public class FVEModel : PageModel
    {

        [BindProperty]
        public FVEData FVEData { get; set; } = new();

        private HubConnection? hubConnection;
        readonly private MQTTService mqttService = new("147.230.76.38");

        [BindProperty]
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

            //await mqttService.Connect();
            //await mqttService.Subscribe("FVE/Ibehej_TX", e => {
            //    text = e.ApplicationMessage.ConvertPayloadToString();
            //    Console.WriteLine($"Received message: {text}");
            //    RedirectToPage("/FVE");
            //    return Task.CompletedTask;
                
            //});
            //hubConnection = new HubConnectionBuilder()
            //    .WithUrl(Request.Host+"/mqtthub")
            //    .Build();

            //hubConnection.On<string>("Subscribe", (message) => {
            //    text = $"{message}";
            //});

            //await hubConnection.StartAsync();
        }
    }
}
