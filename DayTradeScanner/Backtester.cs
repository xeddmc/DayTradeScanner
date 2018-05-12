using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeSharp;

namespace DayTradeScanner
{
	public class BackTester
	{
		public void Test()
		{
			var api = new ExchangeBitfinexAPI();

			// get candle stick data
			Console.WriteLine("Loading candlesticks");
			var allCandles = new List<MarketCandle>();
			var startDate = new DateTime(2018, 1, 1);
			while (true)
			{
				try
				{
					var candles = api.GetCandles("EOSUSD", 5 * 60, startDate, DateTime.Now, 1000).ToList();
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

			// do back testing
			Console.WriteLine("");
			Console.WriteLine("Start backtesting...");
			var trades = new List<Trade>();
			Trade trade = null;
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
								trade = new Trade("EOSUSD", candle.Timestamp, TradeType.Long, candle.ClosePrice);
							}
						}
					}
				}
			}


			// Show statistics
			double winners = 0;
			double losers = 0;
			decimal totalProfit = 0;
			double totalMinutes = 0;
			foreach (var tr in trades)
			{
				totalProfit += tr.ProfitPercentage.Value;
				if (tr.ProfitPercentage < 0) losers++;
				else winners++;
				totalMinutes += (tr.CloseDate.Value - tr.StartDate).TotalMinutes;
			}

			double winPercentage = winners / (winners + losers);
			winPercentage *= 100.0;
			totalMinutes /= trades.Count;

			var averageTime = TimeSpan.FromMinutes(totalMinutes);
			var averageProfit = totalProfit / trades.Count;

			Console.WriteLine("");
			Console.WriteLine("Backtesting results");
			Console.WriteLine($"Symbol              : EOS/USD on bitfinex");
			Console.WriteLine($"Period              : 1-1-2018 / 12-5-2018");
			Console.WriteLine($"Trades              : {trades.Count}");
			Console.WriteLine($"Winners             : {winners}");
			Console.WriteLine($"Losers              : {losers}");
			Console.WriteLine($"Win %               : {winPercentage}");
			Console.WriteLine($"Average time/trade  : {averageTime}");
			Console.WriteLine($"Average profit/trade: {averageProfit} %");

			Console.ReadLine();
		}
	}
}
