using System.Globalization;
using System.Xml.Linq;

namespace PredikceVytěžováníFVE.Services;

public sealed class EcbCurrencyConversionService
{
    private const string EcbDailyUrl = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
    private static readonly XNamespace Ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

    public async Task<decimal> GetCurrentEurToCzkRateAsync(CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(EcbDailyUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);
        var document = XDocument.Parse(xml);

        var czkNode = document
            .Descendants(Ns + "Cube")
            .FirstOrDefault(x => (string?)x.Attribute("currency") == "CZK");

        if (czkNode is null)
        {
            throw new InvalidOperationException("CZK rate was not found in the ECB response.");
        }

        var rateText = (string?)czkNode.Attribute("rate");
        if (string.IsNullOrWhiteSpace(rateText))
        {
            throw new InvalidOperationException("CZK rate is missing in the ECB response.");
        }

        if (!decimal.TryParse(rateText, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
        {
            throw new InvalidOperationException($"Unable to parse CZK rate '{rateText}'.");
        }

        return rate;
    }

    public async Task<decimal> ConvertEurToCzkAsync(decimal amountEur, CancellationToken cancellationToken = default)
    {
        var rate = await GetCurrentEurToCzkRateAsync(cancellationToken);
        return amountEur * rate;
    }
}