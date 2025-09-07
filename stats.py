# -----------------------------
# Calculate PnL for each trade
# -----------------------------
trade_results = []

for d,p,s,sl,tp in signals:
    exit_price = None
    # Simple simulation: check which level (SL/TP) would have been hit first
    candle = df.loc[d]
    if s == "BUY":
        if candle['low'] <= sl:
            exit_price = sl
        else:
            exit_price = tp
    else:  # SELL
        if candle['high'] >= sl:
            exit_price = sl
        else:
            exit_price = tp
    pnl = (exit_price - p) if s=="BUY" else (p - exit_price)
    trade_results.append({
        "datetime": d,
        "type": s,
        "entry": p,
        "exit": exit_price,
        "pnl": pnl
    })

# Convert to DataFrame
df_trades = pd.DataFrame(trade_results)

# Summary stats
total_trades = len(df_trades)
winning_trades = len(df_trades[df_trades['pnl']>0])
losing_trades = len(df_trades[df_trades['pnl']<=0])
total_pnl = df_trades['pnl'].sum()

print(f"Total trades: {total_trades}")
print(f"Winning trades: {winning_trades}")
print(f"Losing trades: {losing_trades}")
print(f"Total PnL: {total_pnl:.5f}\n")

df_trades.head(10)
