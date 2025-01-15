using MQTTnet;
using MQTTnet.Protocol;
using System.Text;

namespace PredikceVytěžováníFVE.Services
{
    public class MQTTService
    {
        string broker = "test.mosquitto.org";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string username = "";
        string password = "";

        IMqttClient mqttClient;

        public MQTTService()
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
                Console.WriteLine($"Received message: {e.ApplicationMessage.ConvertPayloadToString()}");
                return Task.CompletedTask;
            };

        }

        public async Task SendMessage(string topic, string message)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            await mqttClient.PublishAsync(mqttMessage);
        }

    }
}
