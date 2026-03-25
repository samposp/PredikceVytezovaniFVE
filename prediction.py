import pandas as pd
import numpy as np
from statsmodels.tsa.statespace.sarimax import SARIMAX
import xgboost as xgb
from sklearn.model_selection import RandomizedSearchCV
import sqlite3
import itertools
import warnings
import json

def hybrid_24h_forecast(df, value_col, temperature_col, wind_col=None, pressure_col=None, precip_col=None,
                        step_hours=1,
                        future_temp=None, future_wind=None, future_pressure=None, future_precip=None,
                        best_xgb_params=None,
                        best_order=(1,1,2),
                        best_seasonal_order=(1,1,1,24),
                        forecast_shift=0,
                        retrain=False):

    """
    Returns a 24-hour hybrid forecast using SARIMAX + XGBoost residual model.

    Parameters:
    - df: DataFrame with datetime index and columns: value_col, temperature_col
    - value_col: str, name of consumption column
    - temperature_col: str, name of temperature column
    - best_xgb_params: dict, optional XGBoost hyperparameters

    Returns:
    - forecast_df: DataFrame with 24-hour forecast
    """
    df = df.copy()
    agg_dict = {value_col: "sum", temperature_col: "mean"}
    if wind_col:
        agg_dict[wind_col] = "mean"
    if pressure_col:
        agg_dict[pressure_col] = "mean"
    if precip_col:
        agg_dict[precip_col] = "mean"

    df_resampled = df.resample(f"{step_hours}h").agg(agg_dict)
    
    # -----------------------------
    # Fill missing values
    # -----------------------------
    df_resampled[value_col] = df_resampled[value_col].interpolate(method='time').ffill().bfill()
    df_resampled[temperature_col] = df_resampled[temperature_col].interpolate(method='time').ffill().bfill()
    if wind_col:
        df_resampled[wind_col] = df_resampled[wind_col].interpolate(method='time').ffill().bfill()
    if pressure_col:
        df_resampled[pressure_col] = df_resampled[pressure_col].interpolate(method='time').ffill().bfill()
    if precip_col:
        df_resampled[precip_col] = df_resampled[precip_col].interpolate(method='time').ffill().bfill()
    
    # -----------------------------
    # Add features
    # -----------------------------
    df_resampled['hour'] = df_resampled.index.hour
    df_resampled['is_weekend'] = (df_resampled.index.dayofweek >= 5).astype(int)  # True for Saturday/Sunday

    short_lags=[]
    rolling_windows=[]

    # Short-term lags
    for lag in short_lags:
        df_resampled[f"lag_{lag}"] = df_resampled[value_col].shift(lag)

    # Rolling averages
    for w in rolling_windows:
        df_resampled[f"rolling_{w}"] = df_resampled[value_col].rolling(w).mean()
    
    # Long-term lags
    df_resampled['lag_24'] = df_resampled[value_col].shift(int(24 / step_hours))
    df_resampled['lag_48'] = df_resampled[value_col].shift(int(48 / step_hours))
    df_resampled['lag_168'] = df_resampled[value_col].shift(int(168 / step_hours))
    
    df_resampled = df_resampled.dropna()  # remove rows with insufficient lag history
    
    # -----------------------------
    # Prepare data
    # -----------------------------
    X_cols = ['hour', 'is_weekend', 'lag_24', 'lag_48', 'lag_168', temperature_col] \
            + [f"lag_{lag}" for lag in short_lags] \
            + [f"rolling_{w}" for w in rolling_windows]
    if wind_col:
        X_cols.append(wind_col)
    if pressure_col:
        X_cols.append(pressure_col)
    if precip_col:
        X_cols.append(precip_col)
        
    X = df_resampled[X_cols]
    y = df_resampled[value_col]

    # -----------------------------
    # SARIMAX for trend/seasonality
    # -----------------------------
    print("Start SARIMAX", flush=True)
    sarimax_model = SARIMAX(
        y,
        order=best_order,
        seasonal_order=best_seasonal_order,
        enforce_stationarity=False,
        enforce_invertibility=False
    ).fit(disp=False, method="lbfgs",maxiter=5)
    
    # Predict 24-hour SARIMAX forecast
    forecast_steps = int(24 / step_hours)
    sarimax_forecast = sarimax_model.get_forecast(forecast_steps).predicted_mean
    
    print("Stop SARIMAX", flush=True)

    # -----------------------------
    # XGBoost for residuals
    # -----------------------------
    residuals = y - sarimax_model.fittedvalues

    if retrain:
        param_grid = {
            'n_estimators': [100, 300, 500],
            'max_depth': [3, 5, 7],
            'learning_rate': [0.01, 0.05, 0.1],
            'subsample': [0.7, 1.0],
            'colsample_bytree': [0.7, 1.0]
        }
    
        xgb_models = xgb.XGBRegressor(objective='reg:squarederror', random_state=42)
        search = RandomizedSearchCV(
            estimator=xgb_models,
            param_distributions=param_grid,
            n_iter=10,  # number of parameter combinations to try
            scoring='neg_mean_squared_error',
            cv=3,
            verbose=0,
            n_jobs=-1
        )
        search.fit(X, residuals)
        xgb_model = search.best_estimator_

        config = load_config("settings.config.json")
        save_xgb_params_to_config(config, search.best_params_, "settings.config.json")
    else:
        xgb_params = best_xgb_params or {
            'objective': 'reg:squarederror',
            'n_estimators': 300,
            'max_depth': 5,
            'learning_rate': 0.05,
            'random_state': 42
        }
        xgb_model = xgb.XGBRegressor(**xgb_params)
        xgb_model.fit(X, residuals)
    print("Stop XGBoost", flush=True)


    # -----------------------------
    # Build 24-hour XGBoost features
    # -----------------------------
    last_data = df_resampled.copy()
    predictions = []

    forecast_steps = int(24 / step_hours)

    last_hist_time = df_resampled.index[-1]
    forecast_start = (last_hist_time + pd.Timedelta(days=1)).normalize()

    forecast_times = pd.date_range(
        start=forecast_start,
        periods=forecast_steps,
        freq=f"{step_hours}h"
    )

    for i, next_time in enumerate(forecast_times):
        row = {}
        row['hour'] = next_time.hour
        row['is_weekend'] = next_time.dayofweek >= 5

        # Use provided future temperature if available
        if future_temp is not None:
            if isinstance(future_temp, pd.Series):
                row[temperature_col] = future_temp.reindex([next_time], method="nearest").iloc[0]
            else:  # assume DataFrame with temperature_col
                row[temperature_col] = future_temp.loc[next_time, temperature_col]
        else:
            row[temperature_col] = last_data[temperature_col].iloc[-1]  # fallback

        # Wind
        if wind_col:
            if future_wind is not None:
                row[wind_col] = future_wind.reindex([next_time], method="nearest").iloc[0] \
                    if isinstance(future_wind, pd.Series) else future_wind.loc[next_time, wind_col]
            else:
                row[wind_col] = last_data[wind_col].iloc[-1]
        
        # Pressure
        if pressure_col:
            if future_pressure is not None:
                row[pressure_col] = future_pressure.reindex([next_time], method="nearest").iloc[0] \
                    if isinstance(future_pressure, pd.Series) else future_pressure.loc[next_time, pressure_col]
            else:
                row[pressure_col] = last_data[pressure_col].iloc[-1]
        
        # Precipitation
        if precip_col:
            if future_precip is not None:
                row[precip_col] = future_precip.reindex([next_time], method="nearest").iloc[0] \
                    if isinstance(future_precip, pd.Series) else future_precip.loc[next_time, precip_col]
            else:
                row[precip_col] = last_data[precip_col].iloc[-1]
        
        # Long-term lags
        row['lag_24'] = last_data[value_col].iloc[-24 // step_hours]
        row['lag_48'] = last_data[value_col].iloc[-48 // step_hours]
        row['lag_168'] = last_data[value_col].iloc[-168 // step_hours]

        # Short-term lags
        for lag in short_lags:
            row[f"lag_{lag}"] = last_data[value_col].iloc[-lag]

        # Rolling features
        for w in rolling_windows:
            row[f"rolling_{w}"] = last_data[value_col].iloc[-w:].mean()
        
        # Convert to DataFrame in correct column order
        X_new = pd.DataFrame([row], columns=X_cols)
        
        # Predict residual
        res_pred = xgb_model.predict(X_new)[0]

        # SARIMAX forecast for this hour
        sarimax_value = sarimax_forecast.iloc[i]
        
        # Hybrid forecast
        y_pred = sarimax_value + res_pred
        predictions.append(y_pred)
        
        # Append predicted value to last_data for recursive long-term lags
        row[value_col] = y_pred
        new_row_df = pd.DataFrame([row], index=[next_time])
        last_data = pd.concat([last_data, new_row_df])
    
    forecast_df = pd.DataFrame({'forecast': predictions}, index=forecast_times)
    
    return forecast_df

def find_best_sarimax_params(series, step_hours=1, maxiter=10):
    warnings.filterwarnings("ignore")

    p = d = q = range(0, 3)
    P = D = Q = range(0, 2)

    s = int(24 / step_hours)  # daily seasonality

    best_aic = np.inf
    best_order = None
    best_seasonal_order = None

    for order in itertools.product(p, d, q):
        for seasonal in itertools.product(P, D, Q):
            seasonal_order = (seasonal[0], seasonal[1], seasonal[2], s)

            # skip obviously heavy / often unstable models
            if sum(order) + sum(seasonal) > 5:
                continue

            try:
                model = SARIMAX(
                    series,
                    order=order,
                    seasonal_order=seasonal_order,
                    enforce_stationarity=False,
                    enforce_invertibility=False
                )

                results = model.fit(
                    disp=False,
                    maxiter=maxiter,
                    method="lbfgs"
                )

                # ignore failed convergence
                if not getattr(results, "mle_retvals", {}).get("converged", True):
                    continue

                if results.aic < best_aic:
                    best_aic = results.aic
                    best_order = order
                    best_seasonal_order = seasonal_order

            except Exception:
                continue

    print("Best SARIMAX order:", best_order)
    print("Best SARIMAX seasonal_order:", best_seasonal_order)
    print("Best AIC:", best_aic)

    return best_order, best_seasonal_order

def load_config(path):
    with open(path, "r") as f:
        return json.load(f)

def get_sarimax_params(config):
    sarimax_cfg = config.get("Model", {}).get("SARIMAX", {})
    
    order = tuple(sarimax_cfg.get("order", (1, 1, 2)))
    seasonal_order = tuple(sarimax_cfg.get("seasonal_order", (1, 1, 1, 24)))
    
    return order, seasonal_order

def get_xgb_params_from_config(config):
    return config.get("Model", {}).get("XGBoostParams", None)

def save_xgb_params_to_config(config, params, path):
    if "Model" not in config:
        config["Model"] = {}

    config["Model"]["XGBoostParams"] = params

    with open(path, "w") as f:
        json.dump(config, f, indent=2)

def save_sarimax_params(config, order, seasonal_order, path):
    if "Model" not in config:
        config["Model"] = {}

    config["Model"]["SARIMAX"] = {
        "order": list(order),
        "seasonal_order": list(seasonal_order)
    }

    with open(path, "w") as f:
        json.dump(config, f, indent=2)

def turn_off_retrain(config, retrain, path):
    if retrain:
        if "Model" not in config:
            config["Model"] = {}

        config["Model"]["Retrain"] = False

        with open(path, "w") as f:
            json.dump(config, f, indent=2)

def load_sqlite_data(db_path, table_name, datetime_col):
    conn = sqlite3.connect(db_path)
    query = f"SELECT * FROM {table_name}"
    df = pd.read_sql(query, conn)
    conn.close()

    df[datetime_col] = pd.to_datetime(df[datetime_col])
    df = df.set_index(datetime_col).sort_index()

    for col in df.columns:
        if df[col].dtype == object:
            df[col] = df[col].astype(str).str.replace(",", ".", regex=False)
        df[col] = pd.to_numeric(df[col], errors="coerce")

    return df

def save_forecast_to_sqlite(df, db_path, table_name):
    conn = sqlite3.connect(db_path)
    try:
        df_to_save = df.copy()
        df_to_save.index = pd.to_datetime(df_to_save.index)
        df_to_save.index.name = "index"

        cursor = conn.cursor()

        # delete existing rows with the same timestamps
        timestamps = [ts.strftime("%Y-%m-%d %H:%M:%S") for ts in df_to_save.index]
        placeholders = ",".join(["?"] * len(timestamps))

        cursor.execute(
            f'DELETE FROM {table_name} WHERE "index" IN ({placeholders})',
            timestamps
        )

        # insert new rows
        df_to_save.to_sql(table_name, conn, if_exists="append", index=True)

        conn.commit()
    finally:
        conn.close()

def run_forecast_pipeline():
    # -----------------------------
    # Paths
    # -----------------------------
    db = "FVEDB.db"
    settings = "settings.config.json"

    config = load_config(settings)

    # -----------------------------
    # Load historical data
    # -----------------------------
    df_hist = load_sqlite_data(
        db,
        table_name="HourlyData",
        datetime_col="TimeStamp"
    )
    DAYS = config.get("Model", {}).get("HistoryDays", 30)
    cutoff = df_hist.index.max() - pd.Timedelta(days=DAYS)
    df_hist = df_hist[df_hist.index >= cutoff]

    # -----------------------------
    # Load weather forecast
    # -----------------------------
    df_weather = load_sqlite_data(
        db,
        table_name="WeatherForecastData",
        datetime_col="TimeStamp"
    )

    future_temp = df_weather["Temperature"]


    # -----------------------------
    # Retrain models
    # -----------------------------
    retrain = config.get("Model", {}).get("Retrain", False)
    if retrain:
        print("Retraining models...", flush=True)
        best_order, best_seasonal_order = find_best_sarimax_params(df_hist["P_HOME"])
        if (best_order != None and best_seasonal_order != None):
            save_sarimax_params(config, best_order, best_seasonal_order, settings)

    # -----------------------------
    # Run forecast
    # -----------------------------
    best_order, best_seasonal_order = get_sarimax_params(config)
    best_xgb_params = get_xgb_params_from_config(config)

    forecast_df = hybrid_24h_forecast(
        df=df_hist,
        value_col="P_HOME",
        temperature_col="Temperature",
        future_temp=future_temp,
        best_xgb_params=best_xgb_params,
        best_order=best_order,
        best_seasonal_order=best_seasonal_order,
        retrain=retrain
    )

    # -----------------------------
    # Save results
    # -----------------------------
    save_forecast_to_sqlite(
        forecast_df,
        db,
        table_name="ConsumptionForecastData"
    )

    turn_off_retrain(config, retrain, settings)
        

    print("Forecast successfully saved.", flush=True)


run_forecast_pipeline()