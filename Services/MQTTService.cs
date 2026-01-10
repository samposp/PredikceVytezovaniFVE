using MQTTnet;
using MQTTnet.Protocol;
using System.ServiceModel.Security;
using System.Text;

namespace PredikceVytěžováníFVE.Services
{
    public class MQTTService
    {
        private string broker, clientId, username, password;
        private int port;

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

        public async Task Disconnect() {
            await mqttClient.DisconnectAsync();
        }
        public async Task Subscribe(string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> callback)
        {
            await mqttClient.SubscribeAsync(topic);

            // Callback function when a message is received
            mqttClient.ApplicationMessageReceivedAsync += callback;
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
