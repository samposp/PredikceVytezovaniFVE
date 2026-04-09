using EasyCronJob.Abstractions;

namespace PredikceVytěžováníFVE.Services;

public class PredictionSechduleJob : CronJobService
{
    private readonly ILogger<PredictionSechduleJob> _logger;
    private readonly PredictitonService _predictionService;
    private readonly SendPredictionService _sendPredictionService;

    public PredictionSechduleJob(ICronConfiguration<PredictionSechduleJob> cronConfiguration, ILogger<PredictionSechduleJob> logger, PredictitonService predictionService, SendPredictionService sendPredictionService) 
        : base(cronConfiguration.CronExpression,cronConfiguration.TimeZoneInfo,cronConfiguration.CronFormat)
    {
        _logger = logger;
        _predictionService = predictionService;
        _sendPredictionService = sendPredictionService;
    }
    public async override Task<Task> DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting prediction Service");
        var predicted = await _predictionService.Predict(DateTime.Now.AddDays(1));
        await _sendPredictionService.SendData(predicted);
        _logger.LogInformation("Prediction has ended");
        return base.DoWork(cancellationToken);
    }
}
