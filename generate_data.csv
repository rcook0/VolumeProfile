import pandas as pd
import numpy as np

# Generate 1 month of synthetic 15-minute candles
dates = pd.date_range("2025-08-01 00:00:00", "2025-08-31 23:45:00", freq="15T")
n = len(dates)

# Create synthetic price series around 1.0850 with some random walk
np.random.seed(42)
price = 1.0850 + np.cumsum(np.random.normal(0, 0.0005, n))

# Construct OHLCV
openp = price
highp = price + np.random.uniform(0.0002, 0.0008, n)
lowp = price - np.random.uniform(0.0002, 0.0008, n)
closep = price + np.random.uniform(-0.0003, 0.0003, n)
vol = np.random.randint(100, 300, n)

df = pd.DataFrame({
    "datetime": dates.strftime("%Y-%m-%d %H:%M:%S"),
    "open": openp.round(5),
    "high": highp.round(5),
    "low": lowp.round(5),
    "close": closep.round(5),
    "volume": vol
})

# Save to CSV
df.to_csv("data.csv", index=False)
print("data.csv generated with", len(df), "rows")
