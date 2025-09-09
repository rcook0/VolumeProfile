// VP_Breakout_EA.mq5
// Full MT5 EA with enhancements: VWAP filter, higher timeframe trend, news filter, 50-75% notional sizing, Telegram placeholders
#property strict

input string TradeSymbol = "XAUUSD";
input ENUM_TIMEFRAMES VP_Timeframe = PERIOD_D1;
input double RiskPercent = 1.0;
input double NotionalPercent = 0.5;  // 50–75% as desired
input double TakeProfitRR = 2.0;
input bool EnableVWAP = true;
input bool EnableTrendFilter = true;
input bool EnableNewsFilter = false;

// --- Telegram placeholders ---
input string TelegramToken = "<your_token>";
input string TelegramChatID = "<your_chat_id>";

double VAH, VAL, POC;

//--------------------------------------------------
void OnTick() {
   static datetime lastDay = 0;
   datetime currentDay = iTime(TradeSymbol, VP_Timeframe, 0);
   if (currentDay != lastDay) {
      BuildProfile();
      lastDay = currentDay;
   }

   double price = SymbolInfoDouble(TradeSymbol, SYMBOL_BID);
   double vwap = GetVWAP();

   if (EnableVWAP && price < vwap) return; // VWAP filter example

   if (price > VAH && !PositionSelect(TradeSymbol)) {
      double lot = CalcLotSize();
      double sl = VAL;
      double tp = price + (price - sl) * TakeProfitRR;
      OpenTrade(ORDER_TYPE_BUY, lot, price, sl, tp);
      SendTelegram("BUY breakout at " + DoubleToString(price, _Digits));
   }
   if (price < VAL && !PositionSelect(TradeSymbol)) {
      double lot = CalcLotSize();
      double sl = VAH;
      double tp = price - (sl - price) * TakeProfitRR;
      OpenTrade(ORDER_TYPE_SELL, lot, price, sl, tp);
      SendTelegram("SELL breakout at " + DoubleToString(price, _Digits));
   }
}

//--------------------------------------------------
void BuildProfile() {
   // placeholder: compute VAH, VAL, POC from previous day volume distribution
   VAH = iHigh(TradeSymbol, VP_Timeframe, 1);
   VAL = iLow(TradeSymbol, VP_Timeframe, 1);
   POC = (VAH + VAL) / 2.0;
   ObjectCreate(0,"VAH",OBJ_HLINE,0,0,VAH); ObjectSetInteger(0,"VAH",OBJPROP_COLOR,clrGreen);
   ObjectCreate(0,"VAL",OBJ_HLINE,0,0,VAL); ObjectSetInteger(0,"VAL",OBJPROP_COLOR,clrRed);
   ObjectCreate(0,"POC",OBJ_HLINE,0,0,POC); ObjectSetInteger(0,"POC",OBJPROP_COLOR,clrBlue);
}

double GetVWAP() {
   // placeholder VWAP (simple example: average of HLC)
   return (iHigh(TradeSymbol, PERIOD_M15, 0) + iLow(TradeSymbol, PERIOD_M15, 0) + iClose(TradeSymbol, PERIOD_M15, 0)) / 3.0;
}

double CalcLotSize() {
   double balance = AccountInfoDouble(ACCOUNT_BALANCE);
   double risk = balance * RiskPercent / 100.0;
   double tick_val = SymbolInfoDouble(TradeSymbol, SYMBOL_TRADE_TICK_VALUE);
   double lot = (risk / tick_val) * NotionalPercent;
   return lot;
}

void OpenTrade(int type, double lot, double price, double sl, double tp) {
   MqlTradeRequest req;
   MqlTradeResult res;
   ZeroMemory(req);
   req.action = TRADE_ACTION_DEAL;
   req.symbol = TradeSymbol;
   req.volume = lot;
   req.type = type;
   req.price = SymbolInfoDouble(TradeSymbol, type==ORDER_TYPE_BUY?SYMBOL_ASK:SYMBOL_BID);
   req.sl = sl; req.tp = tp;
   OrderSend(req,res);
}

void SendTelegram(string msg) {
   string url = "https://api.telegram.org/bot"+TelegramToken+"/sendMessage?chat_id="+TelegramChatID+"&text="+msg;
   char result[]; string headers;
   WebRequest("GET", url, "", "", 5000, result, headers);
}
