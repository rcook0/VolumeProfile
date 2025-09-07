# candlestick_demo.ipynb (Jupyter Notebook cell)

import pandas as pd
import mplfinance as mpf

# Load the CSV
df = pd.read_csv("data.csv", parse_dates=["datetime"])
df.set_index("datetime", inplace=True)

# Plot candlestick chart (last 200 candles for readability)
mpf.plot(df.tail(200), type="candle", style="yahoo", volume=True, mav=(20,50))
