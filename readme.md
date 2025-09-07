# Volume Profile Breakout Package - Enhanced Version

Contains:
- MT5 EA (`VP_Breakout_EA.mq5`)
- cTrader cBot (`VProfileBreakoutBot.cs`)
- Python Backtrader strategy (`vp_backtrader.py`)

## Features
- VAH/VAL/POC breakout
- VWAP filter
- Higher timeframe trend filter (placeholder)
- Optional news filter
- SL/TP plotting
- 50–75% notional trade sizing
- Telegram alerts (placeholders)

## Setup
- Replace `<your_token>` and `<your_chat_id>` with real Telegram bot credentials.
- Compile/run in MT5, cTrader Automate, or Python with:
  ```bash
  pip install backtrader pandas matplotlib requests
  python vp_backtrader.py
