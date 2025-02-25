using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Hubs {
    public class MqttHub : Hub {

        readonly private MQTTService mqttService = new("147.230.76.38");

        public async Task Subscribe() {
            await mqttService.Connect();
            await mqttService.Subscribe("FVE/Ibehej_TX", e => {
                Console.WriteLine(e.ApplicationMessage.ConvertPayloadToString());
                return Clients.All.SendAsync("ReceiveMqtt", e.ApplicationMessage.ConvertPayloadToString());
            } );
        }
    }
}
