using MQTTnet;
using MQTTnet.Protocol;
using System.ServiceModel.Security;
using System.Text;

namespace PredikceVytěžováníFVE.Services
{
    public class MQTTService
    {
        string broker = "147.230.76.38";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string username = "";
        string password = "";

        IMqttClient mqttClient;

        public MQTTService(string broker, int port = 1883,string? clientId = null, string username = "", string password = "")
        {
            this.broker = broker;
            this.port = port;
            this.clientId = clientId ?? Guid.NewGuid().ToString();
            this.username = username;
            this.password = password;

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
