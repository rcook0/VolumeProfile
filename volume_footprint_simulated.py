import pandas as pd
import numpy as np

# Use your 15-min candle data
df = pd.read_csv("data.csv", parse_dates=["datetime"])

footprint_data = []

for idx, row in df.iterrows():
    open_, high, low, close, vol = row['open'], row['high'], row['low'], row['close'], row['volume']
    # Divide candle into 10 price levels
    levels = np.linspace(low, high, 10)
    # Simulate volume distribution using Dirichlet
    volume_distribution = np.random.dirichlet(np.ones(len(levels))) * vol
    # Simulate bid/ask split (delta)
    bid_fraction = np.random.uniform(0.4, 0.6, len(levels))
    bid_vol = volume_distribution * bid_fraction
    ask_vol = volume_distribution - bid_vol
    delta = ask_vol - bid_vol
    footprint_data.append({
        "datetime": row['datetime'],
        "levels": levels,
        "bid_vol": bid_vol,
        "ask_vol": ask_vol,
        "delta": delta
    })

# Example: footprint_data[0] has first candle's simulated footprint
