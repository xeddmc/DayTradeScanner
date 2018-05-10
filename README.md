# DayTradeScanner
Simple day trade scanner which scans 5 minute charts for day trade signals

Supportee exchanges: Bitfinex, Binance, Bittrex, Kraken, GDax

This scanner will scan 
- The 5min charts
- All XXX/USD coins with a 24hr volume > $500,000
- where price is above the upper bollinger band or below the lower bollinger band
- and the %K and %D of the stochastic oscillator is <20 or > 80

