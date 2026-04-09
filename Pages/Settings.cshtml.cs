using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Models.Settings;
using PredikceVytěžováníFVE.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PredikceVytěžováníFVE.Pages;

public class SettingsModel(ConfigurationService config, IConfiguration configuration, ILogger<SettingsModel> logger) : PageModel
{
    [BindProperty]
    public FveInfo FveInfo { get; set; } = new();
    [BindProperty]
    public ApiInfo ApiInfo { get; set; } = new();
    [BindProperty]
    public BatteryInfo BatteryInfo { get; set; } = new();
    [BindProperty]
    public ModelInfo ModelInfo { get; set; } = new();
    [BindProperty]
    public ControlInfo ControlInfo { get; set; } = new();
    [BindProperty]
    [Required(ErrorMessage = "Zadejte heslo.")]
    public string SavePassword { get; set; } = string.Empty;

    public void OnGet()
    {
        LoadData();
    }

    public async Task<IActionResult> OnPost()
    {
        var configuredPassword = configuration["SettingsSavePassword"];

        if (string.IsNullOrWhiteSpace(configuredPassword))
        {
            ModelState.AddModelError(string.Empty, "Heslo pro ukládání není nakonfigurované.");
        }
        else if (SavePassword != configuredPassword)
        {
            ModelState.AddModelError(nameof(SavePassword), "Nesprávné heslo.");
        }
        if (!ModelState.IsValid)
        {
            LoadData();
            return Page();
        }

        var currentSettings = config.Settings;

        var fve = Merge(currentSettings.Fve, FveInfo);
        var api = Merge(currentSettings.Api, ApiInfo);
        var battery = Merge(currentSettings.Battery, BatteryInfo);
        var model = Merge(currentSettings.Model, ModelInfo);
        var control = Merge(currentSettings.Control, ControlInfo);

        await config.UpdateAsync(settings =>
        {
            settings.Fve = fve;
            settings.Api = api;
            settings.Battery = battery;
            settings.Model = model;
            settings.Control = control;
        });
        logger.LogInformation(
            "Changing settings to: {settings}",
            JsonSerializer.Serialize(new ApplicationSettings
            {
                Fve = fve,
                Api = api,
                Battery = battery,
                Model = model,
                Control = control
            }));
        return RedirectToPage();
    }

    private void LoadData()
    {
        FveInfo = config.Settings.Fve;
        ApiInfo = config.Settings.Api;
        BatteryInfo = config.Settings.Battery;
        ModelInfo = config.Settings.Model;
        ControlInfo = config.Settings.Control;
    }

    private static T Merge<T>(T current, T posted) where T : class, new()
    {
        var result = new T();

        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite))
        {
            var postedValue = prop.GetValue(posted);
            var currentValue = prop.GetValue(current);

            if (postedValue is string s)
            {
                prop.SetValue(result, string.IsNullOrWhiteSpace(s) ? currentValue : s);
            }
            else
            {
                prop.SetValue(result, postedValue ?? currentValue);
            }
        }

        return result;
    }
}
