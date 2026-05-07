# Predikce vytěžování FVE

Webová aplikace pro sledování dat z fotovoltaické elektrárny, predikci výroby a spotřeby a návrh optimálního řízení baterie podle spotových cen elektřiny. Aplikace průběžně ukládá data z MQTT do SQLite databáze, zobrazuje aktuální a historická data v grafech a denně generuje predikci pro další den.

## Hlavní funkce

- zobrazení aktuálního stavu FVE, baterie, spotřeby domu a toku energie ze/do sítě,
- příjem dat z MQTT brokeru a okamžitá aktualizace UI přes SignalR,
- ukládání historických dat do SQLite databáze,
- zobrazení historických denních grafů FVE,
- získávání spotových cen elektřiny přes OTE SOAP službu,
- získávání předpovědi výroby FVE přes Forecast.Solar / PVForecast,
- získávání teplotní předpovědi přes Open-Meteo,
- predikce spotřeby pomocí Python modelu SARIMAX + XGBoost,
- optimalizace nabíjení a vybíjení baterie pomocí Google OR-Tools,
- odesílání vypočteného řídicího plánu zpět přes MQTT,
- webová stránka pro úpravu základních parametrů aplikace.

## Použité technologie

- **.NET 8 / ASP.NET Core Razor Pages**
- **Entity Framework Core + SQLite**
- **SignalR** pro živou aktualizaci dat v prohlížeči
- **MQTTnet** pro příjem a odesílání MQTT zpráv
- **Google OR-Tools** pro optimalizaci baterie
- **NLog** pro logování
- **NSwag / Swagger** ve vývojovém režimu
- **Python** pro predikci spotřeby
- **Chart.js** pro grafy v UI

## Struktura projektu

```text
Aplikace/
├── BackTest/                  # Backtesting optimalizace baterie
├── Connected Services/        # SOAP reference pro OTE službu
├── Data/                      # EF Core DbContext
├── Helpers/                   # Pomocné konverze, filtry a tvorba URL
├── Hubs/                      # SignalR hub pro MQTT data
├── Migrations/                # EF Core migrace SQLite databáze
├── Models/                    # Datové modely a konfigurační modely
├── Pages/                     # Razor Pages UI
├── Services/                  # Aplikační služby, MQTT, predikce, API klienti
├── wwwroot/                   # CSS, JS, knihovny a statické soubory
├── Program.cs                 # Konfigurace aplikace a DI kontejneru
├── appsettings.json           # ASP.NET Core konfigurace
├── settings.config.json       # Konfigurace FVE, baterie, modelu a API
├── prediction.py              # Predikce spotřeby pomocí SARIMAX + XGBoost
└── process_hourly_data.py     # Předzpracování MQTT dat na hodinová data
```

## Požadavky

- .NET SDK 8.0 nebo novější
- Python 3.10 nebo novější
- SQLite databáze je používaná automaticky přes EF Core
- dostupné internetové připojení pro externí API služby
- MQTT broker s daty ve formátu očekávaném modelem `MqttData`

## Konfigurace

### `appsettings.json`

Soubor obsahuje hlavně připojení k databázi a nastavení logování:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data source=./FVEDB.db"
  }
}
```

SQLite databáze `FVEDB.db` se vytváří v pracovním adresáři aplikace. Migrace se aplikují automaticky při startu aplikace.

### `settings.config.json`

Soubor obsahuje provozní nastavení aplikace:

```json
{
  "Fve": {
    "Latitude": "50.000",
    "Longitude": "15.000",
    "PeakPower": 9.9,
    "Declination": 35,
    "Azimuth": -15
  },
  "Api": {
    "MqttBroker": "mqtt.example.com",
    "MqttDataTopic": "FVE/data",
    "MqttPredictTopic": "FVE/prediction",
    "PvForecastApiKey": "YOUR_API_KEY"
  },
  "Model": {
    "HistoryDays": 30,
    "Retrain": false,
    "SARIMAX": {
      "order": [2, 0, 0],
      "seasonal_order": [1, 1, 0, 24]
    },
    "XGBoostParams": {
      "subsample": 1,
      "n_estimators": 100,
      "max_depth": 7,
      "learning_rate": 0.01,
      "colsample_bytree": 0.7
    }
  },
  "Battery": {
    "MinLevel": 20,
    "MaxLevel": 95,
    "ChargeSpeed": 7.5,
    "Capacity": 14.4
  },
  "Control": {
    "MinSellPrice": 1.5,
    "MaxChargingBlocks": 2
  }
}
```

> Skutečné API klíče, hesla a adresy brokerů neukládejte do veřejného repozitáře.

### Heslo pro uložení nastavení

Stránka **Nastavení** vyžaduje konfigurační hodnotu `SettingsSavePassword`. Pro lokální vývoj ji lze nastavit přes user-secrets:

```bash
dotnet user-secrets set "SettingsSavePassword" "vase-heslo"
```

Při nasazení nastavte stejnou hodnotu jako proměnnou prostředí nebo jiným bezpečným konfiguračním mechanismem.

## Spuštění aplikace

1. Rozbalte projekt a přejděte do složky aplikace:

   ```bash
   cd Aplikace
   ```

2. Obnovte NuGet balíčky:

   ```bash
   dotnet restore
   ```

3. Nainstalujte Python závislosti:

   ```bash
   pip install pandas numpy statsmodels xgboost scikit-learn requests
   ```

4. Nastavte heslo pro ukládání konfigurace:

   ```bash
   dotnet user-secrets set "SettingsSavePassword" "vase-heslo"
   ```

5. Spusťte aplikaci:

   ```bash
   dotnet run
   ```

6. Otevřete aplikaci v prohlížeči:

   ```text
   http://localhost:5167
   ```
   ```text
   https://localhost:7287
   ```

## Databáze

Aplikace používá SQLite databázi `FVEDB.db`. Databáze se vytváří automaticky při startu a EF Core migrace se spustí v `Program.cs` pomocí:

```csharp
context.Database.Migrate();
```

Hlavní tabulky:

- `MqttData` – příchozí data z MQTT,
- `SpotData` – spotové ceny,
- `ForecastSolarData` – predikce výroby FVE,
- `WeatherForecastData` – předpověď teploty,
- `HourlyData` – hodinově agregovaná historická spotřeba,
- `ConsumptionForecastData` – predikce spotřeby,
- `PredictedControlData` – vypočtené řízení baterie.

Ruční aplikace migrací:

```bash
dotnet ef database update
```

Pokud příkaz `dotnet ef` není dostupný:

```bash
dotnet tool install --global dotnet-ef
```

## MQTT komunikace

Aplikace se při startu připojí k MQTT brokeru nastavenému v `settings.config.json`:

- `Api:MqttBroker` – adresa brokeru,
- `Api:MqttDataTopic` – topic pro příjem dat z FVE,
- `Api:MqttPredictTopic` – topic pro odesílání predikovaného řízení baterie.

Stav MQTT připojení je vidět v horní navigaci aplikace. Aplikace navíc poskytuje jednoduchý endpoint:

```text
GET /api/mqtt/status
```
Vrací stav připojení, zdraví příjmu dat a čas poslední přijaté zprávy.

## Predikce a plánování

Predikce se skládá z několika kroků:

1. načtení spotových cen přes OTE SOAP službu,
2. načtení nebo stažení predikce výroby FVE,
3. předzpracování historických MQTT dat do hodinového formátu,
4. načtení předpovědi teploty,
5. spuštění Python skriptu `prediction.py`,
6. optimalizace baterie přes OR-Tools,
7. uložení výsledku do databáze,
8. volitelné odeslání řídicího plánu přes MQTT.

Plánovaná úloha `PredictionSechduleJob` běží podle cron výrazu:

```text
0 23 * * *
```

To znamená každý den ve 23:00 lokálního času. Úloha vytvoří predikci na následující den a odešle ji na MQTT topic pro řízení.

## Webové stránky aplikace

- `/` – aktuální data FVE, baterie, sítě a spotřeby, včetně živých aktualizací přes SignalR,
- `/Predict` – predikce výroby, spotřeby, spotových cen a návrhu řízení baterie,
- `/FVE` – historická data a denní grafy,
- `/Settings` – úprava parametrů FVE, baterie, modelu a API.

## Vývojářské poznámky

- Python skripty jsou spouštěny přes příkaz `python`. V prostředí, kde je dostupný pouze `python3`, je potřeba upravit prostředí nebo kód v `ConcumptionPredictionService`.
- Soubor `settings.config.json` je kopírován do výstupního adresáře při buildu.
- Predikce spotřeby používá historická data z `HourlyData`; pokud databáze neobsahuje dostatek dat, predikce nemusí být spolehlivá.
- V režimu `Model:Retrain = true` se mohou přepočítávat parametry modelu a běh predikce může trvat déle.
- Logy se ukládají do složky `log/` v adresáři aplikace.
- Po rozbalení ZIPu z některých prostředí může být název `.csproj` souboru uložen s escapovanými znaky. Pokud `dotnet restore` nebo Visual Studio nenajde projekt, přejmenujte soubor podle názvu uvedeného v `.sln`: `PredikceVytěžováníFVE.csproj`.
