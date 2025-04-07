using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;


namespace PredikceVytěžováníFVE.Services {
    public class SchedulerBackgoundService : BackgroundService {
        private readonly ILogger<SchedulerBackgoundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SchedulerBackgoundService(ILogger<SchedulerBackgoundService> logger, IServiceProvider serviceProvider) {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (!stoppingToken.IsCancellationRequested) {
                // Schedule the job every day at 23:00
                await WaitForNextSchedule("0 23 * * *");

                using var scope = _serviceProvider.CreateScope();
                var scopedSchedulerService = scope.ServiceProvider.GetRequiredService<IScopedSchedulerService>();
                await scopedSchedulerService.ExecuteAsync(stoppingToken);
            }
        }

        private async Task WaitForNextSchedule(string cronExpression) {
            var parsedExp = CronExpression.Parse(cronExpression);
            var currentUtcTime = DateTimeOffset.UtcNow.UtcDateTime;
            var occurenceTime = parsedExp.GetNextOccurrence(currentUtcTime, TimeZoneInfo.Local);

            var delay = occurenceTime.GetValueOrDefault().Subtract(currentUtcTime);
            _logger.LogInformation("The run is delayed for {delay}. Current time: {time}", delay, DateTimeOffset.Now);

            await Task.Delay(delay);
        }
    }
}