using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExchangeSharp;
using Microsoft.Extensions.Configuration;

namespace DayTradeScanner
{
	class Program
	{
		public static IConfigurationRoot Configuration;
		static void Main(string[] args)
		{         
			//var tester = new BackTester();
			//tester.Test(new ExchangeBitfinexAPI(), "EOSUSD");

			var builder = new ConfigurationBuilder()
				.SetBasePath(Path.Combine(AppContext.BaseDirectory))
				.AddJsonFile("appsettings.json", optional: true);

			Configuration = builder.Build();

			var exchange = Configuration["Exchange"];
			var currency = Configuration["Currency"].ToString();
			var volume = long.Parse(Configuration["Min24HrVolume"]);
			var minBBWPercentage = decimal.Parse(Configuration["MinBollingBandWidthPercentage"]);

			ExchangeAPI api = null;
			switch (exchange.ToLowerInvariant())
			{
				case "bitfinex":
					api = new ExchangeBitfinexAPI();
					break;
				case "bittrex":
					api = new ExchangeBittrexAPI();
					break;
				case "binance":
					api = new ExchangeBinanceAPI();
					break;
				case "kraken":
					api = new ExchangeKrakenAPI();
					break;
				case "gdax":
					api = new ExchangeGdaxAPI();
					break;
                case "hitbtc":
                    api = new ExchangeHitbtcAPI();
                    break;
				default:
					Console.WriteLine($"Unknown exchange:{Configuration["Exchange"]}");
					return;
			}


			var bgColor = Console.BackgroundColor;
			var fgColor = Console.ForegroundColor;
			Console.WriteLine("Daytrader scanner 1.0");
			Console.WriteLine($"Construct list of {currency} symbols with 24hr volume > {volume} on {exchange}");
			Task.Run(async () =>
			{
				Console.WriteLine("Downloading all symbols...");
				var allSymbols = await api.GetSymbolsAsync();
				Console.WriteLine("Downloading tickers...");
				var allTickers = await api.GetTickersAsync();

				Console.WriteLine($"Filtering symbols with {currency} and 24hr volume > {volume}");
			    var symbols = new List<string>();
				foreach (var symbol in allSymbols)
				{
					if (!symbol.ToLowerInvariant().Contains(currency.ToLowerInvariant()))
					{
						// only scan USD pairs
						continue;
					}

					var ticker = allTickers.FirstOrDefault(e => e.Key == symbol).Value;
					if (ticker.Volume.ConvertedVolume < volume)
					{
                        Console.WriteLine($"{symbol} 24hr volume:{Math.Floor(ticker.Volume.ConvertedVolume)} skipped");
						continue;
					}
                    Console.WriteLine($"{symbol} 24hr volume:{Math.Floor(ticker.Volume.ConvertedVolume)} added!");
					symbols.Add(symbol);
				}

				Console.WriteLine($"List constructed.. Now starting scanning of 5 min charts for {symbols.Count} symbols...");
				var date = DateTime.Now;
				while (true)
				{
					Console.WriteLine($"scanning...");
					date = DateTime.Now;
					foreach (var symbol in symbols)
					{
						try
						{
							var candles = (await api.GetCandlesAsync(symbol, 60 * 5, DateTime.Now.AddMinutes(-5 * 100))).Reverse().ToList();

							var bbands = new BollingerBands(candles,0);
							var stoch = new Stochastics(candles,0);

							if (bbands.Bandwidth >= minBBWPercentage)
							{
								if (stoch.K < 20 && stoch.D < 20)
								{
									if (candles[0].ClosePrice < bbands.Lower)
									{
										Console.Beep();
										Console.BackgroundColor = ConsoleColor.Red;
										Console.ForegroundColor = ConsoleColor.White;
										Console.WriteLine($"{symbol} long signal found");
										Console.BackgroundColor = bgColor;
										Console.ForegroundColor = fgColor;
									}
								}
								else if (stoch.K > 80 && stoch.D > 80)
								{
									if (candles[0].ClosePrice > bbands.Upper)
									{
										Console.Beep();
										Console.BackgroundColor = ConsoleColor.Red;
										Console.ForegroundColor = ConsoleColor.White;
										Console.WriteLine($"{symbol} short signal found");
										Console.BackgroundColor = bgColor;
										Console.ForegroundColor = fgColor;
									}
								}
							}
						}
						catch(Exception){
							
						}
					}

					Console.WriteLine("waiting...");
					while (true)
					{
						var ts = DateTime.Now - date;
						if (ts.TotalMinutes >= 1) break;
						await Task.Delay(5000);
					}
				}
			}).Wait();
		}
	}
}
