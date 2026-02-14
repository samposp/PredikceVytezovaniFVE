using EasyCronJob.Abstractions;

namespace PredikceVytěžováníFVE.Services;

public class PredictionSechduleJob : CronJobService
{
    private readonly ILogger<PredictionSechduleJob> _logger;
    private readonly PredictitonService _predictionService;

    public PredictionSechduleJob(ICronConfiguration<PredictionSechduleJob> cronConfiguration, ILogger<PredictionSechduleJob> logger, PredictitonService predictionService) 
        : base(cronConfiguration.CronExpression,cronConfiguration.TimeZoneInfo,cronConfiguration.CronFormat)
    {
        _logger = logger;
        _predictionService = predictionService; 
    }
    public async override Task<Task> DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting prediction Service");
        await _predictionService.Predict();
        _logger.LogInformation("Prediction has ended");
        return base.DoWork(cancellationToken);
    }
}
