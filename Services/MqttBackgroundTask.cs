using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Hubs;
using PredikceVytěžováníFVE.Models;
using System.Text.Json;

namespace PredikceVytěžováníFVE.Services;

public class MqttBackgroundTask : BackgroundService
{
    private readonly IHubContext<MqttHub> _hubContext;
    private readonly MQTTService _mqttService;
    private readonly ILogger<MqttBackgroundTask> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _mqttTopic;

    private readonly TimeSpan _reconnectAfter = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _healthCheckInterval = TimeSpan.FromMinutes(1);

    private DateTime _lastConnectAttemptUtc = DateTime.MinValue;
    private DateTime _lastMessageReceivedUtc = DateTime.MinValue;
    private readonly DateTime _startedUtc = DateTime.UtcNow;

    public DateTime LastMessageReceivedUtc => _lastMessageReceivedUtc;

    private readonly SemaphoreSlim _reconnectLock = new(1, 1);
    private bool _subscribed = false;

    public bool IsConnected { get; private set; }
    public bool IsHealthy
    {
        get
        {
            if (!IsConnected)
                return false;

            var referenceTime = _lastMessageReceivedUtc == DateTime.MinValue
                ? _startedUtc
                : _lastMessageReceivedUtc;

            return DateTime.UtcNow - referenceTime < _reconnectAfter;
        }
    }

    public MqttBackgroundTask(
        IHubContext<MqttHub> hubContext,
        ILogger<MqttBackgroundTask> logger,
        IServiceProvider serviceProvider,
        ConfigurationService configuration)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceProvider = serviceProvider;

        _mqttService = new MQTTService(
            configuration.Settings.Api.MqttBroker ?? throw new Exception("missing mqtt broker"));

        _mqttTopic = configuration.Settings.Api.MqttDataTopic
            ?? throw new Exception("missing mqtt data topic");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureConnectedAndSubscribedAsync(stoppingToken);

        using var timer = new PeriodicTimer(_healthCheckInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);

                var now = DateTime.UtcNow;
                var referenceTime = _lastMessageReceivedUtc == DateTime.MinValue
                        ? _startedUtc
                        : _lastMessageReceivedUtc;
                var silenceDuration = now - referenceTime;

                if (silenceDuration >= _reconnectAfter)
                {
                    _logger.LogWarning(
                        "No MQTT message received for {Minutes} minutes. Reconnecting...",
                        _reconnectAfter.TotalMinutes);

                    await ReconnectAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MQTT health check loop.");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MQTT background task.");

        try
        {
            await _mqttService.Disconnect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while disconnecting MQTT.");
        }
        finally
        {
            IsConnected = false;
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task EnsureConnectedAndSubscribedAsync(CancellationToken cancellationToken)
    {
        await _reconnectLock.WaitAsync(cancellationToken);
        try
        {
            _lastConnectAttemptUtc = DateTime.UtcNow;

            _logger.LogInformation("Connecting to MQTT broker...");
            await _mqttService.Connect();
            IsConnected = true;

            if (!_subscribed)
            {
                _logger.LogInformation("Subscribing to MQTT topic: {Topic}", _mqttTopic);

                await _mqttService.Subscribe(_mqttTopic, async e =>
                {
                    try
                    {
                        string message = e.ApplicationMessage.ConvertPayloadToString();

                        _lastMessageReceivedUtc = DateTime.UtcNow;

                        _logger.LogInformation("Message received from MQTT: {Message}", message);

                        MqttData? mqttMessage = JsonSerializer.Deserialize<MqttData>(message);

                        if (mqttMessage == null)
                        {
                            _logger.LogError("MQTT message could not be deserialized.");
                            return;
                        }

                        mqttMessage.FullMessage = message;

                        try
                        {
                            mqttMessage.DateTime = ConverterHelper.ToDateTime(mqttMessage.Date, mqttMessage.Time);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse date and time from MQTT message.");
                            return;
                        }

                        using IServiceScope scope = _serviceProvider.CreateScope();
                        MqttDataService mqttDataService = scope.ServiceProvider.GetRequiredService<MqttDataService>();

                        if (await mqttDataService.SaveMqttData(mqttMessage))
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveMqtt", mqttMessage, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while processing MQTT message.");
                    }
                });

                _subscribed = true;
            }

            _logger.LogInformation("MQTT connected and subscribed successfully.");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            _logger.LogError(ex, "Failed to connect/subscribe to MQTT broker.");
        }
        finally
        {
            _reconnectLock.Release();
        }
    }

    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        if (!await _reconnectLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogInformation("Reconnect already in progress, skipping.");
            return;
        }

        try
        {
            _logger.LogWarning("Starting MQTT reconnect...");

            try
            {
                await _mqttService.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Disconnect before reconnect failed, continuing.");
            }
            IsConnected = false;
            _subscribed = false;

            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            await EnsureConnectedAndSubscribedAfterReconnectAsync(cancellationToken);

            _logger.LogInformation("MQTT reconnect finished.");
        }
        catch (Exception ex)
        {
            IsConnected = false;
            _subscribed = false;
            _logger.LogError(ex, "MQTT reconnect failed.");
        }
        finally
        {
            _reconnectLock.Release();
        }
    }

    private async Task EnsureConnectedAndSubscribedAfterReconnectAsync(CancellationToken cancellationToken)
    {
        _lastConnectAttemptUtc = DateTime.UtcNow;

        _logger.LogInformation("Reconnecting to MQTT broker...");
        await _mqttService.Connect();
        IsConnected = true;

        _logger.LogInformation("Re-subscribing to MQTT topic: {Topic}", _mqttTopic);

        await _mqttService.Subscribe(_mqttTopic, async e =>
        {
            try
            {
                string message = e.ApplicationMessage.ConvertPayloadToString();

                _lastMessageReceivedUtc = DateTime.UtcNow;

                _logger.LogInformation("Message received from MQTT: {Message}", message);

                MqttData? mqttMessage = JsonSerializer.Deserialize<MqttData>(message);

                if (mqttMessage == null)
                {
                    _logger.LogError("MQTT message could not be deserialized.");
                    return;
                }

                mqttMessage.FullMessage = message;

                try
                {
                    mqttMessage.DateTime = ConverterHelper.ToDateTime(mqttMessage.Date, mqttMessage.Time);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse date and time from MQTT message.");
                    return;
                }

                using IServiceScope scope = _serviceProvider.CreateScope();
                MqttDataService mqttDataService = scope.ServiceProvider.GetRequiredService<MqttDataService>();

                if (await mqttDataService.SaveMqttData(mqttMessage))
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMqtt", mqttMessage, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing MQTT message.");
            }
        });

        _subscribed = true;
    }

    public bool IsReceivingMessages()
    {
        if (_lastMessageReceivedUtc == DateTime.MinValue)
            return false;

        return DateTime.UtcNow - _lastMessageReceivedUtc < _reconnectAfter;
    }
}