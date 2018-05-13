using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DayTradeScanner.Backtest;
using DayTradeScanner.Bot.Implementation;
using ExchangeSharp;
using MessagePack;

namespace DayTradeScanner
{
	public class BackTester
	{
		private Cache _cache = new Cache();

		/// <summary>
		/// Downloads 5min. candle sticks data for the symbol from 1/1/2018 until now
		/// </summary>
		/// <returns>list of 5m candle sticks</returns>
		/// <param name="api">exchange api</param>
		/// <param name="symbol">symbol to get the candlesticks from</param>
		private List<MarketCandle> DownloadCandlesFromExchange(ExchangeAPI api, string symbol, DateTime startDate, DateTime endDate)
		{
			// get candle stick data
			Console.WriteLine($"Downloading 5m candlesticks for {symbol} from {api.Name}");
			var allCandles = new List<MarketCandle>();
			while (true)
			{
				try
				{
					var candles = api.GetCandles(symbol, 5 * 60, startDate, DateTime.Now, 1000).ToList();
					if (candles == null) break;
					if (candles.Count == 0) break;
					allCandles.AddRange(candles);
					startDate = candles.Last().Timestamp;
					Console.WriteLine($"date:{candles[0].Timestamp} - {startDate}  / total:{allCandles.Count}");
					if (candles.Count < 1000) break;
					if (startDate > endDate) break;
				}
				catch (Exception)
				{
					Console.WriteLine("-");
				}

				//if (allCandles.Count > 4000) break;
			}
			allCandles.Reverse();
			return allCandles;
		}

		/// <summary>
		/// Backtest strategy for the specified symbol.
		/// </summary>
		/// <param name="api">exchange api</param>
		/// <param name="symbol">Symbol.</param>
		public void Test(ExchangeAPI api, string symbol)
		{
			// Get all candle sticks
			var allCandles = _cache.Load(symbol);
			if (allCandles == null)
			{
				allCandles = DownloadCandlesFromExchange(api, symbol, new DateTime(2018, 1, 1, 0, 0, 0), DateTime.Now);

				_cache.Save(symbol, allCandles);
			}
			foreach (var candle in allCandles) candle.Timestamp = candle.Timestamp.AddHours(2);


            // backtest
			var virtualTradeManager = new VirtualTradeManager();
			var strategy = new DayTradingStrategy(symbol, virtualTradeManager);
			for (int i = allCandles.Count - 50; i > 0; i--)
			{
				virtualTradeManager.Candle = allCandles[i];
				strategy.OnCandle(allCandles, i);
			}


            // show results
			Console.WriteLine("");
            Console.WriteLine("Backtesting results");
            Console.WriteLine($"Symbol                : {symbol} on {api.Name}");
            Console.WriteLine($"Period                : {allCandles[allCandles.Count - 1].Timestamp:dd-MM-yyyy HH:mm} / {allCandles[0].Timestamp:dd-MM-yyyy HH:mm}");

            Console.WriteLine($"Starting capital      : $ 1000");
			Console.WriteLine($"Ending capital        : $ {virtualTradeManager.AccountBalance:0.00}");
            virtualTradeManager.DumpStatistics();
			Console.ReadLine();
		}
	}
}
