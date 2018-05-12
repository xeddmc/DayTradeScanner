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

		private TradeState _state = TradeState.Opened;
		private decimal _rebuyPrice1;
		private decimal _rebuyPrice2;


		public Trade(string symbol, DateTime date, TradeType tradeType, decimal openPrice)
		{
			Symbol = symbol;
			StartDate = date;
			OpenPrice = openPrice;
			TradeType = tradeType;
		}

		public void Process(MarketCandle candle, BollingerBands bbands, Stochastics stoch)
		{
			if (_state == TradeState.Closed) return;

			// close trade if price gets above the upper bollinger band
			var lastPrice = candle.HighPrice;
			if (lastPrice > bbands.Upper)
			{
				CloseDate = candle.Timestamp;
				ClosePrice = lastPrice;

				// calculate average price
				var averagePrice = OpenPrice;
				if (_state == TradeState.Rebuy2)
				{
					averagePrice = OpenPrice + _rebuyPrice1 + _rebuyPrice2;
					averagePrice = averagePrice / 7m;
				}
				else if (_state == TradeState.Rebuy1)
				{
					averagePrice = OpenPrice + _rebuyPrice1;
					averagePrice = averagePrice / 3m;
				}

                // calculate profit
				if (ClosePrice >= averagePrice)
				{
					var profit = ClosePrice / averagePrice;
					ProfitPercentage = 100m * profit;
				}
				else
				{
					var profit = averagePrice / ClosePrice;
					ProfitPercentage = -(100m * profit);
				}
				_state = TradeState.Closed;
				return;
			}

			var closePrice = candle.ClosePrice;
			switch (_state)
			{
				case TradeState.Opened:
					if (closePrice < bbands.Lower)
					{
						if (stoch.K < 20 && stoch.D < 20)
						{
							if (closePrice < 0.9825m * OpenPrice)
							{
								_rebuyPrice1 = closePrice;
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
							if (closePrice < 0.9825m * _rebuyPrice1)
							{
								_rebuyPrice2 = closePrice;
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
	}
}
