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



		public async Task FindCoinsWithEnoughVolume()
		{
			_symbols = new List<string>();
			Console.WriteLine("Daytrader scanner 1.0");
			Console.WriteLine($"Construct list of {_currency} symbols with 24hr volume > {_volume} {_currency} on {_exchange}");

			Console.WriteLine("Downloading all symbols...");
			var allSymbols = await _api.GetSymbolsAsync();

			Console.WriteLine("Downloading tickers...");
			var allTickers = await _api.GetTickersAsync();

			Console.WriteLine($"Filtering symbols with {_currency} and 24hr volume > {_volume}");

			foreach (var symbol in allSymbols)
			{
				if (!symbol.ToLowerInvariant().Contains(_currency.ToLowerInvariant()))
				{
					// only scan valid pairs
					continue;
				}

				var ticker = allTickers.FirstOrDefault(e => e.Key == symbol).Value;
				if (ticker.Volume.ConvertedVolume < _volume)
				{
					Console.WriteLine($"{symbol} 24hr volume:{Math.Floor(ticker.Volume.ConvertedVolume)} skipped");
					continue;
				}
				Console.WriteLine($"{symbol} 24hr volume:{Math.Floor(ticker.Volume.ConvertedVolume)} added!");
				_symbols.Add(symbol);
			}

			Console.WriteLine($"List constructed.. Now starting scanning of 5 min charts for {_symbols.Count} symbols...");
		}

		public async Task Scan()
		{
			var bgColor = Console.BackgroundColor;
			var fgColor = Console.ForegroundColor;
    
			foreach (var symbol in _symbols)
			{
				try
				{
					Console.WriteLine($"Scanning {symbol}...");
					var strategy = new DayTradingStrategy(symbol, new VirtualTradeManager());
					var candles = (await _api.GetCandlesAsync(symbol, 60 * 5, DateTime.Now.AddMinutes(-5 * 50))).Reverse().ToList();

					TradeType tradeType;
					if (strategy.IsValidEntry(candles, 0, out tradeType))
					{
						Console.Beep();
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
