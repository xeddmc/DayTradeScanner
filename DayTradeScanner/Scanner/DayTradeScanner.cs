using DayTradeScanner.Bot.Implementation;
using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DayTradeScanner
{
    public class Scanner
    {
        private Settings _settings;
        private ExchangeAPI _api;
        private List<string> _symbols = new List<string>();

        public Scanner(Settings settings)
        {
            _settings = settings;

            switch (_settings.Exchange.ToLowerInvariant())
            {
                case "bitfinex":
                    _api = new ExchangeBitfinexAPI();
                    break;

                case "bittrex":
                    _api = new ExchangeBittrexAPI();
                    break;

                case "binance":
                    _api = new ExchangeBinanceAPI();
                    break;

                case "kraken":
                    _api = new ExchangeKrakenAPI();
                    break;

                case "gdax":
                    _api = new ExchangeGdaxAPI();
                    break;

                case "hitbtc":
                    _api = new ExchangeHitbtcAPI();
                    break;

                default:
                    Console.WriteLine($"Unknown exchange:{_settings.Exchange}");
                    return;
            }
            FindCoinsWithEnoughVolume();
        }

        /// <summary>
        /// Downloads all symbols from the exchanges and filters out the coins with enough 24hr Volume
        /// </summary>
        /// <returns></returns>
        private void FindCoinsWithEnoughVolume()
        {
            _symbols = new List<string>();
            var allSymbols = _api.GetSymbols();

            var allTickers = _api.GetTickers();

            // for each symbol
            foreach (var symbol in allSymbols)
            {
                if (!IsValidCurrency(symbol))
                {
                    // ignore, symbol has wrong currency
                    continue;
                }

                // check volume
                var ticker = allTickers.FirstOrDefault(e => e.Key == symbol).Value;
                if (ticker.Volume.ConvertedVolume < _settings.Min24HrVolume)
                {
                    // ignore since volume is to low
                    continue;
                }

                // add to list
                _symbols.Add(symbol);
            }
            _symbols = _symbols.OrderBy(e => e).ToList();
        }

        /// <summary>
        /// returns whether currency is allowed
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private bool IsValidCurrency(string symbol)
        {
            if (_settings.ETH && symbol.ToStringLowerInvariant().Contains("eth")) return true;
            if (_settings.EUR && symbol.ToStringLowerInvariant().Contains("eur")) return true;
            if (_settings.USD && symbol.ToStringLowerInvariant().Contains("usd")) return true;
            if (_settings.BTC && symbol.ToStringLowerInvariant().Contains("btc")) return true;
            if (_settings.BNB && symbol.ToStringLowerInvariant().Contains("bnb")) return true;
            return false;
        }

        /// <summary>
        /// List of symbols
        /// </summary>
        /// <value>The symbols.</value>
        public List<string> Symbols
        {
            get
            {
                return _symbols;
            }
        }

        /// <summary>
        /// Performs a scan for all filtered symbols
        /// </summary>
        /// <returns></returns>
        public async Task<Signal> ScanAsync(string symbol)
        {
            try
            {
                // download the new candles
                var candles = (await _api.GetCandlesAsync(symbol, 60 * 5, DateTime.Now.AddMinutes(-5 * 50))).Reverse().ToList();

                // scan candles for buy/sell signal
                TradeType tradeType = TradeType.Long;
                var strategy = new DayTradingStrategy(symbol);
                if (strategy.IsValidEntry(candles, 0, out tradeType))
                {
                    // got buy/sell signal.. write to console
                    return new Signal()
                    {
                        Symbol = symbol,
                        Trade = tradeType.ToString(),
                        Date = candles[0].Timestamp.AddHours(2)
                    };
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}