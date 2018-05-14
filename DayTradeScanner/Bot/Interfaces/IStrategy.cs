    using System;
using System.Collections.Generic;
using ExchangeSharp;

namespace DayTradeScanner.Bot
{
	public interface IStrategy
	{
		/// <summary>
		/// Perform strategy
		/// </summary>
		/// <param name="candles">history of candles</param>
		/// <param name="candle">current candle</param>
		/// <param name="tradeManager">tradeManager</param>
		void OnCandle(List<MarketCandle> candles, int candle, ITradeManager tradeManager);

		/// <summary>
		/// Checks if a valid entry appeard
		/// </summary>
		/// <returns><c>true</c>, if valid entry was found, <c>false</c> otherwise.</returns>
		/// <param name="candles">History of candles</param>
		/// <param name="candle">The current candle</param>
		/// <param name="tradeType">returns trade type.</param>
		bool IsValidEntry(List<MarketCandle> candles, int candle, out TradeType tradeType);

        /// <summary>
        /// Symbol to test
        /// </summary>
        /// <value>The symbol.</value>
		string Symbol { get; }
	}
}
