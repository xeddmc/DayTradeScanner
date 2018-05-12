using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		/// <returns>list of 5m candle sticks since 1/1/2018</returns>
		/// <param name="api">exchange api</param>
		/// <param name="symbol">symbol to get the candlesticks from</param>
		private List<MarketCandle> DownloadCandlesFromExchange(ExchangeAPI api, string symbol)
		{
			// get candle stick data
			Console.WriteLine($"Downloading 5m candlesticks for {symbol} from {api.Name}");
			var allCandles = new List<MarketCandle>();
			var startDate = new DateTime(2018, 1, 1);
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
			var allCandles = _cache.LoadCandlesFromCache(symbol);
			if (allCandles == null)
			{
				allCandles = DownloadCandlesFromExchange(api, symbol);

				_cache.SaveCandlesToCache(symbol, allCandles);
			}

			// Do back testing
			Console.WriteLine("");
			Console.WriteLine("Start backtesting...");
			var trades = new List<Trade>();
			Trade trade = null;
			decimal capital = 1000;
			for (int i = allCandles.Count - 50; i > 0; i--)
			{
				var candle = allCandles[i];
				var bbands = new BollingerBands(allCandles, i);
				var stoch = new Stochastics(allCandles, i);

				if (trade != null)
				{
					trade.Process(candle, bbands, stoch);
					if (trade.IsClosed)
					{
						capital += trade.ProfitDollars;
						trades.Add(trade);
						trade = null;

					}
				}
				else
				{
					if (bbands.Bandwidth >= 2.0m)
					{
						if (stoch.K < 20 && stoch.D < 20)
						{
							if (candle.ClosePrice < bbands.Lower)
							{
								trade = new Trade(symbol, candle.Timestamp, TradeType.Long, candle.ClosePrice, capital);
							}
						}
					}
				}
			}


			// Show statistics


			double winners = 0;
			double losers = 0;
			double totalProfit = 0;
			double totalMinutes = 0;
			double reBuys0 = 0;
			double reBuys1 = 0;
			double reBuys2 = 0;
			double maxProfit = 0;
			double maxLoss = 0;
			var shortestTrade = TimeSpan.MaxValue;
			var longestTrade = TimeSpan.MinValue;
			foreach (var tr in trades)
			{
				tr.Dump();

				var profit = (double)tr.ProfitPercentage.Value;
				totalProfit += profit;
				if (profit < 0)
				{
					losers++;
					if (profit < maxLoss) maxLoss = profit;
				}
				else
				{
					winners++;
					if (profit > maxProfit) maxProfit = profit;
				}

				var duration = (tr.CloseDate.Value - tr.StartDate);
				totalMinutes += duration.TotalMinutes;
				if (duration < shortestTrade) shortestTrade = duration;
				if (duration > longestTrade) longestTrade = duration;

				if (tr.Rebuys == 0) reBuys0++;
				else if (tr.Rebuys == 1) reBuys1++;
				else if (tr.Rebuys == 2) reBuys2++;
			}

			reBuys0 = 100.0 * (reBuys0 / trades.Count);
			reBuys1 = 100.0 * (reBuys1 / trades.Count);
			reBuys2 = 100.0 * (reBuys2 / trades.Count);

			double winPercentage = winners / (winners + losers);
			winPercentage *= 100.0;
			totalMinutes /= trades.Count;

			var averageTime = TimeSpan.FromMinutes(totalMinutes);
			var averageProfit = totalProfit / trades.Count;

			Console.WriteLine("");
			Console.WriteLine("Backtesting results");
			Console.WriteLine($"Starting capital      : $ 1000" );
			Console.WriteLine($"Ending capital        : $ {capital:0.00}");
			Console.WriteLine($"Symbol                : {symbol} on {api.Name}");
			Console.WriteLine($"Period                : {allCandles[allCandles.Count - 1].Timestamp.AddHours(2):dd-MM-yyyy HH:mm} / {allCandles[0].Timestamp.AddHours(2):dd-MM-yyyy HH:mm}");
			Console.WriteLine($"Trades                : {trades.Count} trades");
			Console.WriteLine($"Winners               : {winners} trades");
			Console.WriteLine($"Losers                : {losers} trades");
			Console.WriteLine($"Win %                 : {winPercentage:0.00} %");

			Console.WriteLine($"Trades with no rebuys : {reBuys0:0.00} %");
			Console.WriteLine($"Trades with 1 rebuy   : {reBuys1:0.00} %");
			Console.WriteLine($"Trades with 2 rebuys  : {reBuys2:0.00} %");

			Console.WriteLine($"Average profit/trade  : {averageProfit:0.00} %");
			Console.WriteLine($"Max profit            : {maxProfit:0.00} %");
			Console.WriteLine($"Max loss              : {maxLoss:0.00} %");
			Console.WriteLine($"Average time/trade    : {averageTime}");
			Console.WriteLine($"Shortest trade        : {shortestTrade}");
			Console.WriteLine($"Longest trade         : {longestTrade}");

			Console.ReadLine();
		}
	}
}
