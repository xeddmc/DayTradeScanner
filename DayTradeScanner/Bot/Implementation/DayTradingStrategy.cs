using System;
using System.Collections.Generic;
using ExchangeSharp;

namespace DayTradeScanner.Bot.Implementation
{
	public class DayTradingStrategy : IStrategy
	{
		private enum StrategyState
		{
			Scanning,
			Opened,
			Rebuy1,
			Rebuy2
		}
		private const decimal RebuyPercentage = 0.9825m; //1.75%
		private const decimal SecondBuyFactor = 2m; // 2x
		private const decimal ThirdBuyFactor = 4m;  // 4x 

		private readonly string _symbol;
		private readonly ITradeManager _tradeManager;

		private ITrade _trade = null;
		private decimal _bundleSize;
		private StrategyState _state = StrategyState.Scanning;

		public DayTradingStrategy(string symbol, ITradeManager tradeManager)
		{
			_symbol = symbol;
			_tradeManager = tradeManager;
		}

		/// <summary>
		/// Perform strategy
		/// </summary>
		/// <param name="candles">candle history</param>
		/// <param name="bar">currentcandle</param>
		public void OnCandle(List<MarketCandle> candles, int bar)
		{
			switch (_state)
			{
				case StrategyState.Scanning:
					ScanForEntry(candles, bar);
					break;

				case StrategyState.Opened:
					CloseTradeIfPossible(candles, bar);
					if (_trade == null) return;

					if (CanRebuy(candles, bar))
					{
						var candle = candles[bar];
						if (DoRebuy(candle, SecondBuyFactor))
						{
							_state = StrategyState.Rebuy1;
						}
					}
					break;

				case StrategyState.Rebuy1:
					CloseTradeIfPossible(candles, bar);
					if (_trade == null) return;

					if (CanRebuy(candles, bar))
					{
						var candle = candles[bar];
						if (DoRebuy(candle, ThirdBuyFactor))
						{
							_state = StrategyState.Rebuy2;
						}
					}
					break;

				case StrategyState.Rebuy2:
					CloseTradeIfPossible(candles, bar);
					break;
			}
		}

		/// <summary>
		/// Adds more to the current position by buying (or selling) more coins.
		/// </summary>
		/// <returns><c>true</c>, if rebuy was done, <c>false</c> otherwise.</returns>
		private bool DoRebuy(MarketCandle candle, decimal factor)
		{
			var investment = factor * _bundleSize;
			var coins = investment / candle.ClosePrice;

			switch (_trade.TradeType)
			{
				case TradeType.Long:
					return _tradeManager.BuyMore(_trade, coins);

				case TradeType.Short:
					return _tradeManager.SellMore(_trade, coins);
			}
			return false;
		}

		/// <summary>
		/// Checks if a valid entry appeard
		/// </summary>
		/// <returns><c>true</c>, if valid entry was found, <c>false</c> otherwise.</returns>
		/// <param name="candles">History of candles</param>
		/// <param name="bar">The current candle</param>
		/// <param name="tradeType">returns trade type.</param>
		public bool IsValidEntry(List<MarketCandle> candles, int bar, out TradeType tradeType)
		{
			var candle = candles[bar];
			var bbands = new BollingerBands(candles, bar);
			var stoch = new Stochastics(candles, bar);
			tradeType = TradeType.Long;

			// is bolling bands width > 2%
			if (bbands.Bandwidth >= 2m)
			{
				if (candle.ClosePrice < bbands.Lower && stoch.K < 20 && stoch.D < 20)
				{
					// open buy order when price closes below lower bollinger bands
					// and stochastics K & D are both below 20
					tradeType = TradeType.Long;
					return true;
				}
				else if (candle.ClosePrice > bbands.Upper && stoch.K > 80 && stoch.D > 80)
				{
					// open sell order when price closes above upper bollinger bands
					// and stochastics K & D are both above 80
					tradeType = TradeType.Short;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Scans for possibility to open a new trade.
		/// </summary>
		/// <param name="candles">candle history</param>
		/// <param name="bar">currentcandle</param>
		private void ScanForEntry(List<MarketCandle> candles, int bar)
		{
			TradeType tradeType;
			if (IsValidEntry(candles, bar, out tradeType))
			{
				var candle = candles[bar];
				_bundleSize = _tradeManager.AccountBalance / (1m + SecondBuyFactor + ThirdBuyFactor);
				var coins = _bundleSize / candle.ClosePrice;
				switch (tradeType)
				{
					case TradeType.Long:
						_trade = _tradeManager.BuyMarket(_symbol, coins);
						break;
					case TradeType.Short:
						_trade = _tradeManager.SellMarket(_symbol, coins);
						break;

				}
				if (_trade != null)
				{
					_state = StrategyState.Opened;
				}
			}
		}

		/// <summary>
		/// Returns if the conditions for a rebuy are met.
		/// </summary>
		/// <returns><c>true</c>, if rebuy is possible, <c>false</c> otherwise.</returns>
		/// <param name="candles">candle history</param>
		/// <param name="bar">currentcandle</param>
		private bool CanRebuy(List<MarketCandle> candles, int bar)
		{
			var bbands = new BollingerBands(candles, bar);
			var stoch = new Stochastics(candles, bar);
			var candle = candles[bar];

			// for long we do a rebuy when price closes under the lower bollinger bands and both stochastics are below 20
			// and price is 1.75% below the previous buy
			if (_trade.TradeType == TradeType.Long && candle.ClosePrice < bbands.Lower && stoch.K < 20 && stoch.D < 20)
			{
				var price = _trade.OpenPrice * RebuyPercentage;
				if (_trade.RebuyCount > 0) price = price * RebuyPercentage;
				return candle.ClosePrice <= price;
			}

			// for short we do a rebuy when price closes above the upper bollinger bands and both stochastics are above 80
			// and price is 1.75% above the previous sell
			if (_trade.TradeType == TradeType.Short && candle.ClosePrice > bbands.Upper && stoch.K > 80 && stoch.D > 80)
			{
				var factor = 1m + (1m - RebuyPercentage);
				var price = _trade.OpenPrice * factor;
				if (_trade.RebuyCount > 0) price = price * factor;
				return candle.ClosePrice >= price;
			}
			return false;
		}

		/// <summary>
		/// Closes the trade if price crosses the upper/lower bollinger band.
		/// </summary>
		/// <param name="candles">candle history</param>
		/// <param name="bar">currentcandle</param>
		private void CloseTradeIfPossible(List<MarketCandle> candles, int bar)
		{
			var candle = candles[bar];
			var bbands = new BollingerBands(candles, bar);

			// for long we close the trade if price  gets above the upper bollinger bands
			if (_trade.TradeType == TradeType.Long && candle.HighPrice > bbands.Upper)
			{
				if (_tradeManager.Close(_trade, candle.HighPrice))
				{
					_state = StrategyState.Scanning;
					_trade = null;
					return;
				}
			}

			// for short we close the trade if price  gets below the lowe bollinger bands
			if (_trade.TradeType == TradeType.Short && candle.LowPrice < bbands.Lower)
			{
				if (_tradeManager.Close(_trade, candle.LowPrice))
				{
					_state = StrategyState.Scanning;
					_trade = null;
					return;
				}
			}
		}
	}
}
