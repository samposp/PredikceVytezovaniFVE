using MQTTnet;
using System.Text;

namespace PredikceVytěžováníFVE.Services
{
    public class MQTTService
    {
        string broker = "mqtt.eclipse.org";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string username = "";
        string password = "";

        IMqttClient mqttClient;


        MQTTService()
        {
            MqttClientFactory factory = new();
            mqttClient = factory.CreateMqttClient();
        }

        public async Task<MqttClientConnectResult?> Connect()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();
            return await mqttClient.ConnectAsync(options);
        }
        public async Task Subscribe(string topic)
        {
            await mqttClient.SubscribeAsync(topic);

            // Callback function when a message is received
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                return Task.CompletedTask;
            };

        }

    }
}
