import sqlite3
import pandas as pd
import requests
import json

def get_location(config_path="settings.config.json"):
    # Load JSON config
    try:
        with open(config_path, "r") as f:
            config = json.load(f)
    except FileNotFoundError:
        print(f"Error: Config file '{config_path}' not found.")
        sys.exit(1)
    except json.JSONDecodeError as e:
        print(f"Error: Failed to parse JSON. {e}")
        sys.exit(1)
    
    # Access FVE values
    try:
        latitude = float(config["Fve"]["Latitude"])
        longitude = float(config["Fve"]["Longitude"])
    except KeyError as e:
        print(f"Error: Missing key in config: {e}")
        sys.exit(1)
    except ValueError as e:
        print(f"Error: Invalid value type in config: {e}")
        sys.exit(1)
    return latitude, longitude

def get_temperature(start, end):
    latitude, longitude = get_location()
    url = (
        "https://archive-api.open-meteo.com/v1/archive"
        f"?latitude={latitude}&longitude={longitude}"
        f"&start_date={start}&end_date={end}"
        f"&hourly=temperature_2m"
        f"&timezone=auto"
    )

    response = requests.get(url)
    data = response.json()

    temp_df = pd.DataFrame({
        "timestamp": data["hourly"]["time"],
        "temperature": data["hourly"]["temperature_2m"]
    })

    temp_df["timestamp"] = pd.to_datetime(temp_df["timestamp"])
    temp_df = temp_df.set_index("timestamp")

    # Align to your df index range
    temp_df = temp_df.loc[start:end]

    return temp_df

def process_fve_data(
    db_path="FVEDB.db",
    raw_table="MqttData",
    processed_table="HourlyData",
    date_col="Date",
    time_col="Time",
    value_col="P_HOME",
    timestamp_col="TimeStamp",
    battery_col="P_BAT"
):
    conn = sqlite3.connect(db_path)

    # Ensure processed table exists
    conn.execute(f"""
    CREATE TABLE IF NOT EXISTS {processed_table} (
        {timestamp_col} TEXT PRIMARY KEY,
        {value_col} REAL,
        temperature REAL
    )
    """)

    # Get last processed timestamp
    query_last = f"""
    SELECT MAX({timestamp_col}) as last_ts
    FROM {processed_table}
    """
    last_ts_df = pd.read_sql(query_last, conn)
    last_ts = last_ts_df['last_ts'].iloc[0]

    if last_ts:
        last_ts = pd.to_datetime(last_ts)

    print(f"Last processed timestamp: {last_ts}")

    # Build query for raw data
    if last_ts is None:
        query_raw = f"""
            SELECT {date_col}, {time_col}, {value_col}, {battery_col}
            FROM {raw_table}
            ORDER BY {date_col}, {time_col}
        """
    else:
        last_date = last_ts.strftime("%d.%m.%Y")
        last_time = last_ts.strftime("%H:%M:%S")
        query_raw = f"""
            SELECT {date_col}, {time_col}, {value_col}, {battery_col}
            FROM {raw_table}
            WHERE ({date_col} || ' ' || {time_col}) > '{last_date} {last_time}'
            ORDER BY {date_col}, {time_col}
        """

    raw_df = pd.read_sql_query(query_raw, conn)

    if raw_df.empty:
        print("No new data to process.")
        conn.close()
        return 0

    # Data preparation
    df = raw_df.copy()
    df[timestamp_col] = pd.to_datetime(
        df[date_col] + " " + df[time_col],
        format="%d.%m.%Y %H:%M:%S"
    )
    df = df.set_index(timestamp_col)
    df = df.drop([date_col, time_col], axis=1)

    df[value_col] = (df[value_col] + df[battery_col]).abs()

    # Resampling + cleaning
    df = df.resample("h").mean()
    df = df.fillna(df.shift(24))
    df = df.interpolate("time").ffill().bfill()

    processed_df = df[[value_col]]
    
    if last_ts is not None:
        processed_df = processed_df[processed_df.index > last_ts]

    if processed_df.empty:
        print("No new data to add")
        return
        
    start = processed_df.index.min().strftime("%Y-%m-%d")
    end = processed_df.index.max().strftime("%Y-%m-%d")
    temp_df = get_temperature(start, end)
    processed_df = processed_df.join(temp_df)
    processed_df.index.name = timestamp_col
    processed_df = processed_df.reset_index()
    # Save to DB
    processed_df.to_sql(processed_table, conn, if_exists='append', index=False)

    print(f"Stored {len(processed_df)} processed rows")

    conn.close()
    return len(processed_df)

process_fve_data()