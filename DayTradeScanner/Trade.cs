using System;
using System.Collections.Generic;
using ExchangeSharp;

namespace DayTradeScanner
{
	public class Trade
	{
		private const decimal RebuyPercentage = 0.9825m; //1.75%
		private const decimal SecondBuyFactor = 2m; // 2x
		private const decimal ThirdBuyFactor = 4m;  // 4x
		private const decimal FeesPercentage = 0.2m; // 0.2% fees

		public string Symbol { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? CloseDate { get; set; }
		public decimal OpenPrice { get; set; }
		public decimal? ClosePrice { get; set; }
		public TradeType TradeType { get; set; }
		public decimal? ProfitPercentage { get; set; }
		public decimal ProfitDollars { get; set; }
		public bool IsClosed => _state == TradeState.Closed;
		public int Rebuys { get; set; }

		private decimal RebuyPrice1;
		private DateTime RebuyDate2;

		private decimal RebuyPrice2;
		private DateTime RebuyDate1;


		private TradeState _state = TradeState.Opened;
		private decimal _bundleSize;
		private decimal _amountOfCoins;
		private decimal _totalInvestment = 0;
		private decimal _capital = 0;
		private decimal _feesPayed = 0;


		public Trade(string symbol, DateTime date, TradeType tradeType, decimal openPrice, decimal capital)
		{
			Symbol = symbol;
			StartDate = date;
			OpenPrice = openPrice;
			TradeType = tradeType;
			Rebuys = 0;

			_capital = capital;
			_bundleSize = _capital / (1m + SecondBuyFactor + ThirdBuyFactor);
			_amountOfCoins = _bundleSize / OpenPrice;
			_totalInvestment = _bundleSize;

			_feesPayed = (FeesPercentage / 100m) * _bundleSize;
		}

		public void Process(MarketCandle candle, BollingerBands bbands, Stochastics stoch)
		{
			if (_state == TradeState.Closed) return;

			// close trade if price gets above the upper bollinger band for long positions
            // close trade if price gets below the lower bollinger band for short positions
			var lastPrice = (TradeType == TradeType.Long) ? candle.HighPrice : candle.LowPrice;
			if ((TradeType == TradeType.Long  && lastPrice > bbands.Upper) ||
				(TradeType == TradeType.Short && lastPrice < bbands.Lower))
			{
				CloseDate = candle.Timestamp;
				ClosePrice = lastPrice;

				_feesPayed += (FeesPercentage / 100m) * _totalInvestment;
				ProfitDollars = (_amountOfCoins * ClosePrice.Value) - _totalInvestment;

                if (TradeType == TradeType.Short)
                {
					ProfitDollars = -ProfitDollars;
                }

				ProfitDollars -= _feesPayed;

				ProfitPercentage = (ProfitDollars / _totalInvestment) * 100m;

				_state = TradeState.Closed;
				return;
			}

			var closePrice = candle.ClosePrice;
			var date = candle.Timestamp;
			switch (_state)
			{
				case TradeState.Opened:
					if (TradeType == TradeType.Long && closePrice < bbands.Lower && stoch.K < 20 && stoch.D < 20)
					{
						if (closePrice < RebuyPercentage * OpenPrice)
						{
							RebuyPrice1 = closePrice;
							RebuyDate1 = date;
							Rebuys = 1;

							var investment = SecondBuyFactor * _bundleSize;
							var coins = ((investment) / closePrice);
							_amountOfCoins += coins;

							_totalInvestment += investment;
							_feesPayed += (FeesPercentage / 100m) * (investment);
							_state = TradeState.Rebuy1;
						}
					}
					else if (TradeType == TradeType.Short && closePrice > bbands.Upper && stoch.K > 80 && stoch.D > 80)
					{
						if (closePrice > RebuyPercentage * OpenPrice)
						{
							RebuyPrice1 = closePrice;
							RebuyDate1 = date;
							Rebuys = 1;

							var investment = SecondBuyFactor * _bundleSize;
							var coins = ((investment) / closePrice);
							_amountOfCoins += coins;

							_totalInvestment += investment;
							_feesPayed += (FeesPercentage / 100m) * (investment);
							_state = TradeState.Rebuy1;
						}
					}
					break;

				case TradeState.Rebuy1:

					if (TradeType == TradeType.Long && closePrice < bbands.Lower && stoch.K < 20 && stoch.D < 20)
					{
						if (closePrice < RebuyPercentage * RebuyPrice1)
						{
							RebuyPrice2 = closePrice;
							RebuyDate2 = date;
							Rebuys = 2;
							var investment = ThirdBuyFactor * _bundleSize;
							var coins = ((investment) / closePrice);
							_amountOfCoins += coins;

							_totalInvestment += investment;
							_feesPayed += (FeesPercentage / 100m) * (investment);
							_state = TradeState.Rebuy2;
						}
					}
					else if (TradeType == TradeType.Short && closePrice > bbands.Upper && stoch.K > 80 && stoch.D > 80)
					{
						if (closePrice > RebuyPercentage * RebuyPrice1)
						{
							RebuyPrice2 = closePrice;
							RebuyDate2 = date;
							Rebuys = 2;
							var investment = ThirdBuyFactor * _bundleSize;
							var coins = ((investment) / closePrice);
							_amountOfCoins += coins;

							_totalInvestment += investment;
							_feesPayed += (FeesPercentage / 100m) * (investment);
							_state = TradeState.Rebuy2;
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
			Console.WriteLine($"{StartDate:dd-MM-yyyy HH:mm} - {CloseDate:dd-MM-yyyy HH:mm} capital:${_capital:0.00} bundlesize:${_bundleSize:0.00} open:{ClosePrice:0.000000} close: {OpenPrice:0.000000}  profit:{ProfitPercentage:0.00} % , ${ProfitDollars:0.00}");
			switch (Rebuys)
			{
				case 0:
					break;

				case 1:
					Console.WriteLine($"  rebuy #1: {RebuyDate1:dd-MM-yyyy HH:mm} {RebuyPrice1:0.000000}");
					break;

				case 2:
					Console.WriteLine($"  rebuy #1: {RebuyDate1:dd-MM-yyyy HH:mm} {RebuyPrice1:0.000000}");
					Console.WriteLine($"  rebuy #2: {RebuyDate2:dd-MM-yyyy HH:mm} {RebuyPrice2:0.000000}");
					break;
			}
		}
	}
}
