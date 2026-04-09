using Microsoft.AspNetCore.SignalR;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models.DB;
using System.Globalization;
using System.Text.Json;

namespace PredikceVytěžováníFVE.Services;

public class SendPredictionService
{
    const int DischargeMode = 0;
    const int ChargeMode = 1;
    const int SellMode = 2;
    const int HoldMode = 3;

    private readonly MQTTService mqttService;
    private readonly ILogger<SendPredictionService> _logger;
    private readonly string mqttTopic;
    private readonly ConfigurationService _config;

    public SendPredictionService(ILogger<SendPredictionService> logger, IServiceProvider serviceProvider, ConfigurationService configuration)
    {
        _logger = logger;
        mqttService = new(configuration.Settings.Api.MqttBroker ?? throw new Exception("missing mqtt broker"));
        mqttTopic = configuration.Settings.Api.MqttPredictTopic ?? throw new Exception("missing mqtt data topic");
        _config = configuration;
    }

    public async Task SendData(ControlPredictionBo? control)
    {
        if (control == null)
        {
            _logger.LogWarning("Control prediction data is null, skipping MQTT send.");
            return;
        }
        string message = CreateMessage(control);
        _logger.LogWarning("Sending prediction control to MQTT topic: {message}", message);
        try
        {
            await mqttService.Connect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MQTT broker");
            return;
        }

        try
        {
            await mqttService.SendMessage(mqttTopic, message);
            _logger.LogInformation("Prediction control sent to MQTT topic: {message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to MQTT topic");
            return;
        }
    }

    private string CreateMessage(ControlPredictionBo model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        int dischargeValue = _config.Settings.Battery.MinLevel ?? throw new Exception("Missing configuration for battery minimum level.");

        if (model.Capacity == null || model.Capacity.Count != 96)
            throw new InvalidOperationException("Capacity must be filled.");

        var dateKey = model.TimeStamp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var times = new SortedDictionary<string, int[]>();
        var baseActions = new List<(DateTime Time, int Mode, int Value)>();
        var holdActions = new List<(DateTime Time, int Mode, int Value)>();

        if (model.ChargeTimes != null)
        {
            if (model.ChargeToCapacities == null)
                throw new InvalidOperationException("ChargeToCapacities must be filled when ChargeTimes are used.");

            if (model.ChargeTimes.Count != model.ChargeToCapacities.Count)
                throw new InvalidOperationException("ChargeTimes and ChargeToCapacities must have the same number of items.");

            for (int i = 0; i < model.ChargeTimes.Count; i++)
            {
                var startTime = model.ChargeTimes[i];
                var target = model.ChargeToCapacities[i];

                baseActions.Add((startTime, ChargeMode, target));

                var holdTime = FindHoldTime(
                    scheduleDay: model.TimeStamp.Date,
                    startTime: startTime,
                    capacities: model.Capacity,
                    targetValue: target,
                    isCharge: true);

                if (holdTime.HasValue)
                {
                    holdActions.Add((holdTime.Value, HoldMode, target));
                }
            }
        }

        if (model.DischargeTimes != null)
        {
            foreach (var startTime in model.DischargeTimes)
            {
                baseActions.Add((startTime, DischargeMode, dischargeValue));

                var holdTime = FindHoldTime(
                    scheduleDay: model.TimeStamp.Date,
                    startTime: startTime,
                    capacities: model.Capacity,
                    targetValue: dischargeValue,
                    isCharge: false);

                if (holdTime.HasValue)
                {
                    holdActions.Add((holdTime.Value, HoldMode, dischargeValue));
                }
            }
        }

        // First add real actions
        foreach (var action in baseActions)
        {
            times[ToTimeKey(action.Time)] = new[] { action.Mode, action.Value };
        }

        // Then add hold actions, but do not overwrite real actions
        foreach (var hold in holdActions)
        {
            var key = ToTimeKey(hold.Time);

            if (!times.ContainsKey(key))
            {
                times[key] = new[] { hold.Mode, hold.Value };
            }
        }

        var daySchedule = new Dictionary<string, SortedDictionary<string, int[]>>
        {
            [dateKey] = times
        };

        return JsonSerializer.Serialize(new[] { daySchedule });
    }

    private static DateTime? FindHoldTime(
        DateTime scheduleDay,
        DateTime startTime,
        List<float> capacities,
        int targetValue,
        bool isCharge)
    {
        int startIndex = GetQuarterHourIndex(startTime);

        if (startIndex < 0 || startIndex >= capacities.Count)
            return null;

        int sameCount = 0;
        float lastCapacity = -1;
        for (int i = startIndex; i < capacities.Count; i++)
        {
            float capacity = capacities[i];

            bool reachedTarget = isCharge
                ? capacity >= targetValue
                : capacity <= targetValue;

            if (reachedTarget)
            {
                return scheduleDay.Date.AddMinutes(i * 15);
            }

            if (capacity == lastCapacity)
            {
                sameCount++;
                if (sameCount == 3)
                {
                    return scheduleDay.Date.AddMinutes((i-2) * 15);
                }
            }
            else
            {
                sameCount = 0;
            }
            lastCapacity = capacity;
        }
        return scheduleDay.Date.AddMinutes((capacities.Count-1) * 15);
    }

    private static int GetQuarterHourIndex(DateTime time)
    {
        return (time.Hour * 60 + time.Minute) / 15;
    }

    private static string ToTimeKey(DateTime time)
    {
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}


