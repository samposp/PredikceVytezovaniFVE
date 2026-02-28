using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PredikceVytěžováníFVE.Models.Settings;
using PredikceVytěžováníFVE.Services;
using System.Text.Json;

namespace PredikceVytěžováníFVE.Pages;

public class SettingsModel(ConfigurationService config, ILogger<SettingsModel> logger) : PageModel
{
    [BindProperty]
    public FveInfo FveInfo { get; set; } = new();
    [BindProperty]
    public ApiInfo ApiInfo { get; set; } = new();

    public void OnGet()
    {
        FveInfo = config.Settings.Fve;
        ApiInfo = config.Settings.Api;
    }

    public async Task OnPost()
    {
        await config.UpdateAsync( settings =>
        {
            settings.Api = ApiInfo;
            settings.Fve = FveInfo;
        });
        logger.LogInformation("Changing settings To: {settings}", JsonSerializer.Serialize(
            new ApplicationSettings() { Fve = FveInfo, Api = ApiInfo }));
    }
}
