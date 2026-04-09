namespace PredikceVytěžováníFVE.Models.Settings;

public class ApiInfo
{
    public string? MqttBroker { get; set; }
    public string? MqttDataTopic { get; set; }
    public string? MqttPredictTopic { get; set; }
    public string? PvForecastApiKey { get; set; }
}
