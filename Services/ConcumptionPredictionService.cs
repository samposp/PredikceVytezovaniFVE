using PredikceVytěžováníFVE.Data;
using PredikceVytěžováníFVE.Helpers;
using PredikceVytěžováníFVE.Models;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PredikceVytěžováníFVE.Services;

public class ConcumptionPredictionService(IServiceProvider serviceProvider, ILogger<PredictitonService> logger, OpenMeteoService meteoService, ConfigurationService configuration, IHostApplicationLifetime hostLifetime)
{
    public async Task<List<TimeValuePair>?> GetPrediction(DateTime date, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        bool retrain = configuration.Settings.Model.Retrain;
        var db = scope.ServiceProvider.GetRequiredService<FVEDbContext>();
        if (!retrain)
        {
            var dbData = db.ConsumptionForecastData.Where(db => db.TimeStamp.Date == date.Date).ToList();
            if (dbData != null && dbData.Count == 24)
            {
                logger.LogInformation("Consumption prediction data retrieved from DB");
                return [.. dbData.Select(ConverterHelper.ToTimeValuePair)];
            }
        }
        await CallPythonScript("process_hourly_data.py", 5, cancellationToken);  // get historic data
        await meteoService.GetTomorrowTemperature(date); // get forecast data
        await CallPythonScript("prediction.py", retrain ? 20 : 5, cancellationToken); // predict consumption

        var newDbData = db.ConsumptionForecastData.Where(db => db.TimeStamp.Date == date.Date).ToList();
        return [.. newDbData.Select(ConverterHelper.ToTimeValuePair)];
    }
    private async Task CallPythonScript(string filename, int timeout = 5, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"-u {filename}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                logger.LogInformation("[PY] {Line}", e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                logger.LogError("[PY-ERR] {Line}", e.Data);
        };

        logger.LogInformation("Calling script: {Filename}", filename);

        if (!process.Start())
        {
            logger.LogError("Failed to start Python process");
            return;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(timeout));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            hostLifetime.ApplicationStopping,
            timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Python script '{filename}' exited with code {process.ExitCode}");
            }
            logger.LogInformation("Python script finished with exit code {ExitCode}", process.ExitCode);
        }
        catch (OperationCanceledException)
        {
            logger.LogError("Python script timed out: {Filename}", filename);

            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                logger.LogError("Python script timed out: {Filename}", filename);
                KillProcessTree(process, filename);
                throw new TimeoutException($"Python script timed out: {filename}");
            }
            catch (OperationCanceledException) when (hostLifetime.ApplicationStopping.IsCancellationRequested || cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Python script cancelled due to shutdown/request: {Filename}", filename);
                KillProcessTree(process, filename);
                throw;
            }
            catch
            {
                if (!process.HasExited)
                    KillProcessTree(process, filename);
                throw;
            }
            finally
            {
                try
                {
                    if (!process.HasExited)
                        KillProcessTree(process, filename);
                }
                catch
                {
                    // ignore cleanup errors here
                }
            }
        }
    }

    private void KillProcessTree(Process process, string filename)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                logger.LogWarning("Killed Python process tree for {Filename}", filename);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to kill Python process tree for {Filename}", filename);
        }
    }
}
