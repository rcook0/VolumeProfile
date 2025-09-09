// VProfileBreakoutBot.cs
using System;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class VProfileBreakoutBot : Robot
    {
        [Parameter("Symbol", DefaultValue = "")]
        public string SymbolName { get; set; }

        [Parameter("Buckets", DefaultValue = 50)]
        public int Buckets { get; set; }

        [Parameter("Risk %", DefaultValue = 1.0)]
        public double RiskPercent { get; set; }

        [Parameter("TP RR", DefaultValue = 2.0)]
        public double TakeProfitRR { get; set; }

        [Parameter("Value Area %", DefaultValue = 70)]
        public int ValueAreaPercent { get; set; }

        [Parameter("Only one trade", DefaultValue = true)]
        public bool OnlyOne { get; set; }

        private Symbol _symbol;
        private double VAH, VAL, POC;
        private DateTime lastProfileTime = DateTime.MinValue;

        protected override void OnStart()
        {
            _symbol = (SymbolName == "") ? Symbol : MarketData.GetSymbol(SymbolName);
            // build initial
            BuildProfile();
        }

        protected override void OnBar()
        {
            // rebuild daily profile if new day
            var daily = MarketData.GetBars(TimeFrame.Daily);
            if (daily.Count < 2) return;
            var anchor = daily.Last(1).OpenTime;
            if (anchor != lastProfileTime)
            {
                BuildProfile();
                lastProfileTime = anchor;
            }

            // check M15 closed bar
            var bars15 = MarketData.GetBars(TimeFrame.Minute15);
            if (bars15.Count < 2) return;
            var lastClose = bars15.Last(1).Close;
            // check positions
            int posCount = Positions.FindAll("VP_Breakout", _symbol.Name).Length;
            if (lastClose > VAH && (!OnlyOne || posCount==0))
                TryOpen(TradeType.Buy, lastClose);
            else if (lastClose < VAL && (!OnlyOne || posCount==0))
                TryOpen(TradeType.Sell, lastClose);
        }

        private void BuildProfile()
        {
            var dailyBars = MarketData.GetBars(TimeFrame.Daily);
            if (dailyBars.Count < 2) return;
            var sessionBar = dailyBars.Last(1);
            double sHigh = sessionBar.High;
            double sLow  = sessionBar.Low;
            if (sHigh <= sLow) return;

            var buckets = new double[Buckets];
            var prices = new double[Buckets];
            for (int i = 0; i < Buckets; i++)
            {
                prices[i] = sLow + (sHigh - sLow) * (i + 0.5) / Buckets;
                buckets[i] = 0;
            }

            // iterate minute bars within the session
            var minuteBars = MarketData.GetBars(TimeFrame.Minute);
            var startIndex = minuteBars.FindIndex(b => b.OpenTime == sessionBar.OpenTime);
            if (startIndex < 0) startIndex = minuteBars.Count - 1;
            for (int i = startIndex; i < minuteBars.Count; i++)
            {
                var b = minuteBars[i];
                if (b.OpenTime < sessionBar.OpenTime) continue;
                if (b.OpenTime >= sessionBar.OpenTime + TimeSpan.FromDays(1)) break;
                double price = b.Close;
                int idx = (int)Math.Floor((price - sLow) / (sHigh - sLow) * Buckets);
                if (idx < 0) idx = 0;
                if (idx >= Buckets) idx = Buckets - 1;
                buckets[idx] += b.TickVolume;
            }

            double total = buckets.Sum();
            if (total <= 0) return;
            int pocIdx = 0;
            for (int i = 1; i < Buckets; i++)
                if (buckets[i] > buckets[pocIdx]) pocIdx = i;
            POC = prices[pocIdx];

            double target = total * ValueAreaPercent / 100.0;
            double sum = buckets[pocIdx];
            int low = pocIdx, high = pocIdx;
            while (sum < target)
            {
                double left = (low - 1 >= 0) ? buckets[low - 1] : -1;
                double right = (high + 1 < Buckets) ? buckets[high + 1] : -1;
                if (left <= 0 && right <= 0) break;
                if (left > right) { low--; sum += buckets[low]; } else { high++; sum += buckets[high]; }
            }
            VAL = prices[low];
            VAH = prices[high];
            Print("VP built. POC=" + POC + " VAL=" + VAL + " VAH=" + VAH);
        }

        private void TryOpen(TradeType type, double price)
        {
            // Risk calc
            double equity = Account.Balance + Account.Equity - Account.Balance; // close enough; cTrader provides better APIs if needed
            equity = Account.Balance; // simple
            double risk = equity * RiskPercent / 100.0;
            double sl = (type == TradeType.Buy) ? VAL : VAH;
            double dist = Math.Abs(price - sl);
            if (dist <= Symbol.TickSize * 2) { Print("SL too close"); return; }
            // approximate pip value per lot:
            double pipValue = Symbol.PipValue;
            if (pipValue == 0) pipValue = 1.0;
            double riskPerLot = dist / Symbol.TickSize * pipValue;
            double volume = risk / Math.Max(riskPerLot, 1e-6);
            double volStep = Symbol.VolumeStep;
            volume = Math.Max(Symbol.VolumeMin, Math.Floor(volume / volStep) * volStep);
            volume = Math.Min(volume, Symbol.VolumeMax);
            if (volume < Symbol.VolumeMin) { Print("volume too small"); return; }

            double tp = (type==TradeType.Buy) ? price + TakeProfitRR * dist : price - TakeProfitRR * dist;
            var res = ExecuteMarketOrder(type, _symbol.Name, volume, "VP_Breakout", sl, tp);
            if (res.IsSuccessful) Print("Order placed: " + res.Position.Id);
            else Print("Order failed: " + res.Error);
        }
    }
}
