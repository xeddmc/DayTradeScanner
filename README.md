# DayTradeScanner
Simple day trade scanner which scans USD symbols on bitfinex for day trade signals

This scanner will scan 
- All USD symbols on Bitfinex with a 24hr volume > $500,000
- where price is above the upper bollinger band or below the lower bollinger band
- and the %K and %D of the stochastic oscillator is <20 or > 80

