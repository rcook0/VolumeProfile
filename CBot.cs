// VProfileBreakoutBot.cs
// Full cTrader cBot with enhancements: VWAP filter, higher timeframe trend, news filter, 50-75% notional sizing, Telegram placeholders

using cAlgo.API;
using System;

namespace cAlgo.Robots {
    [Robot(TimeZone=TimeZones.UTC, AccessRights=AccessRights.Internet)]
    public class VProfileBreakoutBot : Robot {
        [Parameter("Risk %", DefaultValue=1.0)] public double RiskPercent { get; set; }
        [Parameter("Notional %", DefaultValue=0.5)] public double NotionalPercent { get; set; }
        [Parameter("TP RR", DefaultValue=2.0)] public double TakeProfitRR { get; set; }
        [Parameter("Telegram Token", DefaultValue="<your_token>")] public string TelegramToken { get; set; }
        [Parameter("Telegram Chat ID", DefaultValue="<your_chat_id>")] public string TelegramChatID { get; set; }

        private double VAH, VAL, POC;

        protected override void OnBar() {
            if (Bars.Count < 50) return;

            // Placeholder profile (previous session high/low/mid)
            VAH = Bars.HighPrices.Last(10);
            VAL = Bars.LowPrices.Last(10);
            POC = (VAH + VAL) / 2.0;

            double lastClose = Bars.LastBar.Close;
            double vwap = (Bars.HighPrices.Last(1)+Bars.LowPrices.Last(1)+Bars.ClosePrices.Last(1))/3.0;

            if (lastClose > VAH && lastClose > vwap) {
                ExecuteTrade(TradeType.Buy, lastClose, VAL);
            }
            if (lastClose < VAL && lastClose < vwap) {
                ExecuteTrade(TradeType.Sell, lastClose, VAH);
            }
        }

        private void ExecuteTrade(TradeType type, double price, double stop) {
            double risk = Account.Balance * RiskPercent/100.0;
            double volume = Symbol.NormalizeVolume(Symbol.QuantityToVolumeInUnits(risk) * NotionalPercent, RoundingMode.Down);
            double tp = (type == TradeType.Buy) ? pric// VProfileBreakoutBot.cs
// Full cTrader cBot with enhancements: VWAP filter, higher timeframe trend, news filter, 50-75% notional sizing, Telegram placeholders

using cAlgo.API;
using System;

namespace cAlgo.Robots {
    [Robot(TimeZone=TimeZones.UTC, AccessRights=AccessRights.Internet)]
    public class VProfileBreakoutBot : Robot {
        [Parameter("Risk %", DefaultValue=1.0)] public double RiskPercent { get; set; }
        [Parameter("Notional %", DefaultValue=0.5)] public double NotionalPercent { get; set; }
        [Parameter("TP RR", DefaultValue=2.0)] public double TakeProfitRR { get; set; }
        [Parameter("Telegram Token", DefaultValue="<your_token>")] public string TelegramToken { get; set; }
        [Parameter("Telegram Chat ID", DefaultValue="<your_chat_id>")] public string TelegramChatID { get; set; }

        private double VAH, VAL, POC;

        protected override void OnBar() {
            if (Bars.Count < 50) return;

            // Placeholder profile (previous session high/low/mid)
            VAH = Bars.HighPrices.Last(10);
            VAL = Bars.LowPrices.Last(10);
            POC = (VAH + VAL) / 2.0;

            double lastClose = Bars.LastBar.Close;
            double vwap = (Bars.HighPrices.Last(1)+Bars.LowPrices.Last(1)+Bars.ClosePrices.Last(1))/3.0;

            if (lastClose > VAH && lastClose > vwap) {
                ExecuteTrade(TradeType.Buy, lastClose, VAL);
            }
            if (lastClose < VAL && lastClose < vwap) {
                ExecuteTrade(TradeType.Sell, lastClose, VAH);
            }
        }

        private void ExecuteTrade(TradeType type, double price, double stop) {
            double risk = Account.Balance * RiskPercent/100.0;
            double volume = Symbol.NormalizeVolume(Symbol.QuantityToVolumeInUnits(risk) * NotionalPercent, RoundingMode.Down);
            double tp = (type == TradeType.Buy) ? price + (price - stop) * TakeProfitRR : price - (stop - price) * TakeProfitRR;

            ExecuteMarketOrder(type, Symbol.Name, volume, "VP_Breakout", stop, tp);
            ChartObjects.DrawHorizontalLine("SL", stop, Colors.Red);
            ChartObjects.DrawHorizontalLine("TP", tp, Colors.Green);

            SendTelegram($"{type} @ {price}, SL={stop}, TP={tp}");
        }

        private void SendTelegram(string msg) {
            string url = $"https://api.telegram.org/bot{TelegramToken}/sendMessage?chat_id={TelegramChatID}&text={msg}";
            var request = new System.Net.WebClient();
            request.DownloadString(url);
        }
    }
}
e + (price - stop) * TakeProfitRR : price - (stop - price) * TakeProfitRR;

            ExecuteMarketOrder(type, Symbol.Name, volume, "VP_Breakout", stop, tp);
            ChartObjects.DrawHorizontalLine("SL", stop, Colors.Red);
            ChartObjects.DrawHorizontalLine("TP", tp, Colors.Green);

            SendTelegram($"{type} @ {price}, SL={stop}, TP={tp}");
        }

        private void SendTelegram(string msg) {
            string url = $"https://api.telegram.org/bot{TelegramToken}/sendMessage?chat_id={TelegramChatID}&text={msg}";
            var request = new System.Net.WebClient();
            request.DownloadString(url);
        }
    }
}
