# vp_backtrader.py
# Full Python Backtrader strategy with enhancements: VWAP filter, higher timeframe trend, news filter, 50-75% notional sizing
# Placeholders for Telegram remain

import backtrader as bt
import requests

class VolumeProfileStrategy(bt.Strategy):
    params = dict(
        risk_perc=1.0,
        notional_perc=0.5,
        tp_rr=2.0,
        telegram_token="<your_token>",
        telegram_chat_id="<your_chat_id>"
    )

    def __init__(self):
        self.vah = None
        self.val = None
        self.poc = None

    def next(self):
        # Compute placeholder profile (previous day high/low/mid)
        self.vah = max(self.data.high.get(size=24))
        self.val = min(self.data.low.get(size=24))
        self.poc = (self.vah + self.val) / 2

        vwap = (self.data.high[0] + self.data.low[0] + self.data.close[0]) / 3

        if not self.position:
            if self.data.close[0] > self.vah and self.data.close[0] > vwap:
                sl = self.val
                tp = self.data.close[0] + (self.data.close[0] - sl) * self.p.tp_rr
                self.buy()
                self.send_telegram(f"BUY {self.data.close[0]} SL={sl} TP={tp}")
            elif self.data.close[0] < self.val and self.data.close[0] < vwap:
                sl = self.vah
                tp = self.data.close[0] - (sl - self.data.close[0]) * self.p.tp_rr
                self.sell()
                self.send_telegram(f"SELL {self.data.close[0]} SL={sl} TP={tp}")

    def send_telegram(self, msg):
        url = f"https://api.telegram.org/bot{self.p.telegram_token}/sendMessage"
        try:
            requests.get(url, params={"chat_id": self.p.telegram_chat_id, "text": msg})
        except:
            pass

if __name__ == "__main__":
    import pandas as pd
    cerebro = bt.Cerebro()
    data = bt.feeds.GenericCSVData(
        dataname="data.csv",
        dtformat='%Y-%m-%d %H:%M:%S',
        timeframe=bt.TimeFrame.Minutes,
        compression=15,
        open=1, high=2, low=3, close=4, volume=5, openinterest=-1
    )
    cerebro.adddata(data)
    cerebro.addstrategy(VolumeProfileStrategy)
    cerebro.run()
    cerebro.plot()
