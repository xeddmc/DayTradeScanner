using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DayTradeScanner.Backtest;
using DayTradeScanner.Bot.Implementation;
using ExchangeSharp;
using Microsoft.Extensions.Configuration;

namespace DayTradeScanner
{
	public class Scanner
	{
		private IConfigurationRoot _configuration;
		private string _exchange;
		private string _currency;
		private long _volume;
		private ExchangeAPI _api;
		private List<string> _symbols = new List<string>();

		public Scanner()
		{
			var builder = new ConfigurationBuilder()
			.SetBasePath(Path.Combine(AppContext.BaseDirectory))
			.AddJsonFile("appsettings.json", optional: true);

			_configuration = builder.Build();

			_exchange = _configuration["Exchange"];
			_currency = _configuration["Currency"].ToString();
			_volume = long.Parse(_configuration["Min24HrVolume"]);

			switch (_exchange.ToLowerInvariant())
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
					Console.WriteLine($"Unknown exchange:{_configuration["Exchange"]}");
					return;
			}
		}


        /// <summary>
        /// Downloads all symbols from the exchanges and filters out the coins with enough 24hr Volume
        /// </summary>
        /// <returns></returns>
		public async Task FindCoinsWithEnoughVolumeAsync()
		{
			_symbols = new List<string>();
			Console.WriteLine("Daytrader scanner 1.0");
			Console.WriteLine($"Construct list of {_currency} symbols with 24hr volume > {_volume} {_currency} on {_exchange}");

			Console.WriteLine("Downloading all symbols...");
			var allSymbols = await _api.GetSymbolsAsync();

			Console.WriteLine("Downloading tickers...");
			var allTickers = await _api.GetTickersAsync();

			Console.WriteLine($"Filtering symbols with {_currency} and 24hr volume > {_volume}");

            // for each symbol
			foreach (var symbol in allSymbols)
			{
				if (!symbol.ToLowerInvariant().Contains(_currency.ToLowerInvariant()))
				{
					// ignore, symbol has wrong currency
					continue;
				}

                // check volume
				var ticker = allTickers.FirstOrDefault(e => e.Key == symbol).Value;
				if (ticker.Volume.ConvertedVolume < _volume)
				{
					// ignore since volume is to low
					Console.WriteLine($"{symbol} 24hr volume:{Math.Floor(ticker.Volume.ConvertedVolume)} skipped");
					continue;
				}

                // add to list
				Console.WriteLine($"{symbol} 24hr volume:{Math.Floor(ticker.Volume.ConvertedVolume)} added!");
				_symbols.Add(symbol);
			}

			Console.WriteLine($"List constructed.. Now starting scanning of 5 min charts for {_symbols.Count} symbols...");
		}

        /// <summary>
        /// Performs a scan for all filtered symbols
        /// </summary>
        /// <returns></returns>
		public async Task ScanAsync()
		{
			// for each symbol
			foreach (var symbol in _symbols)
			{
				try
				{
                    // download the new candles
					Console.WriteLine($"Scanning {symbol}...");
					var candles = (await _api.GetCandlesAsync(symbol, 60 * 5, DateTime.Now.AddMinutes(-5 * 50))).Reverse().ToList();


                    // scan candles for buy/sell signal
					TradeType tradeType;
                    var strategy = new DayTradingStrategy(symbol);
					if (strategy.IsValidEntry(candles, 0, out tradeType))
					{
						// got buy/sell signal.. write to console
						Console.Beep();
						var bgColor = Console.BackgroundColor;
						var fgColor = Console.ForegroundColor;

						Console.BackgroundColor = ConsoleColor.Red;
						Console.ForegroundColor = ConsoleColor.White;
						Console.WriteLine($"{symbol} {tradeType} signal found");
						Console.BackgroundColor = bgColor;
						Console.ForegroundColor = fgColor;
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
