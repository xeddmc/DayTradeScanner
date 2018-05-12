using System;
using System.Collections.Generic;
using ExchangeSharp;

namespace DayTradeScanner
{
	public class Trade
	{
		public string Symbol { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public decimal OpenPrice { get; set; }
		public decimal? ClosePrice { get; set; }
		public TradeType TradeType { get; set; }
		public decimal? ProfitPercentage { get; set; }
		public bool IsClosed => _state == TradeState.Closed;
		public int Rebuys { get; set; }

		private decimal RebuyPrice1;
		private DateTime RebuyDate2;
		private DateTime RebuyDate1;
		private decimal RebuyPrice2;

		private TradeState _state = TradeState.Opened;


		public Trade(string symbol, DateTime date, TradeType tradeType, decimal openPrice)
		{
			Symbol = symbol;
			StartDate = date.AddHours(2);
			OpenPrice = openPrice;
			TradeType = tradeType;
			Rebuys = 0;
		}

		public void Process(MarketCandle candle, BollingerBands bbands, Stochastics stoch)
		{
			if (_state == TradeState.Closed) return;

			// close trade if price gets above the upper bollinger band
			var lastPrice = candle.HighPrice;
			if (lastPrice > bbands.Upper)
			{
				CloseDate = candle.Timestamp.AddHours(2);;
				ClosePrice = lastPrice;

				// calculate average price
				var averagePrice = OpenPrice;
				if (_state == TradeState.Rebuy1)
				{
					averagePrice = OpenPrice + 2 * RebuyPrice1;
					averagePrice = averagePrice / 3m;
				}
				else if (_state == TradeState.Rebuy2)
				{
					averagePrice = OpenPrice + 2 * RebuyPrice1 + 4 * RebuyPrice2;
					averagePrice = averagePrice / 7m;
				}

				// calculate profit
				var profit = (ClosePrice / averagePrice) * 100m;
				if (ClosePrice >= averagePrice)
				{
					ProfitPercentage = profit - 100m;
				}
				else
				{
					ProfitPercentage = -(100m - profit);
				}
				_state = TradeState.Closed;
				return;
			}

			var closePrice = candle.ClosePrice;
			var date = candle.Timestamp.AddHours(2);
			switch (_state)
			{
				case TradeState.Opened:
					if (closePrice < bbands.Lower)
					{
						if (stoch.K < 20 && stoch.D < 20)
						{
							if (closePrice < 0.9825m * OpenPrice)
							{
								RebuyPrice1 = closePrice;
								RebuyDate1 = date;
								Rebuys = 1;
								_state = TradeState.Rebuy1;
							}
						}
					}
					break;

				case TradeState.Rebuy1:

					if (closePrice < bbands.Lower)
					{
						if (stoch.K < 20 && stoch.D < 20)
						{
							if (closePrice < 0.9825m * RebuyPrice1)
							{
								RebuyPrice2 = closePrice;
								RebuyDate2 = date;
								Rebuys = 2;
								_state = TradeState.Rebuy2;
							}
						}
					}
					break;

				case TradeState.Rebuy2:
					// wait....
					break;

				case TradeState.Closed:
					// done...
					break;
			}
		}

		public void Dump()
		{
			Console.WriteLine($"{StartDate:dd-MM-yyyy HH:mm} - {CloseDate:dd-MM-yyyy HH:mm} open:{ClosePrice:0.000000} close: {OpenPrice:0.000000}  profit:{ProfitPercentage:0.00} %");
			switch (Rebuys)
			{
				case 0:
					break;
				case 1:
					Console.WriteLine($"  rebuy1: {RebuyDate1:dd-MM-yyyy HH:mm} {RebuyPrice1:0.000000}");
					break;
				case 2:
					Console.WriteLine($"  rebuy1: {RebuyDate1:dd-MM-yyyy HH:mm} {RebuyPrice1:0.000000}");
					Console.WriteLine($"  rebuy2: {RebuyDate2:dd-MM-yyyy HH:mm} {RebuyPrice2:0.000000}");
					break;
			}
		}
	}
}
