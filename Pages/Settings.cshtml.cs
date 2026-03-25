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
    [Required(ErrorMessage = "Zadejte heslo.")]
    public string SavePassword { get; set; } = string.Empty;

    public void OnGet()
    {
        GetData();
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
            return Page();
        }

        await config.UpdateAsync( settings =>
        {
            settings.Api = ApiInfo;
            settings.Fve = FveInfo;
            settings.Battery = BatteryInfo;
            settings.Model = ModelInfo;
        });
        logger.LogInformation(
            "Changing settings to: {settings}",
            JsonSerializer.Serialize(new ApplicationSettings
            {
                Fve = FveInfo,
                Api = ApiInfo,
                Battery = BatteryInfo,
                Model = ModelInfo
            }));
        return RedirectToPage();
    }

    private void GetData()
    {
        FveInfo = config.Settings.Fve;
        ApiInfo = config.Settings.Api;
        BatteryInfo = config.Settings.Battery;
        ModelInfo = config.Settings.Model;
    }
}
