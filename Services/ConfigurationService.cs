using PredikceVytěžováníFVE.Models.Settings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PredikceVytěžováníFVE.Services;

public class ConfigurationService
{
    private readonly string _filePath = "settings.config.json";
    private readonly SemaphoreSlim _lock = new(1, 1);

    private ApplicationSettings _settings;

    public ApplicationSettings Settings => _settings;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ConfigurationService()
    {
        _settings = LoadOrCreateAsync().GetAwaiter().GetResult();
    }

    private async Task<ApplicationSettings> LoadOrCreateAsync()
    {
        if (!File.Exists(_filePath))
        {
            var defaultSettings = new ApplicationSettings();
            await SaveToFileAsync(defaultSettings);
            return defaultSettings;
        }

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<ApplicationSettings>(json, _jsonOptions)
               ?? new ApplicationSettings();
    }

    public async Task SaveAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await SaveToFileAsync(_settings);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(Action<ApplicationSettings> updateAction)
    {
        await _lock.WaitAsync();
        try
        {
            updateAction(_settings);
            await SaveToFileAsync(_settings);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ReloadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _settings = await LoadOrCreateAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveToFileAsync(ApplicationSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
