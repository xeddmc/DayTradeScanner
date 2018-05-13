using System;
using System.Collections.Generic;

namespace DayTradeScanner.Bot
{
	public interface ITradeManager
	{
		/// <summary>
        /// Open a new buy order at market price
        /// </summary>
        /// <returns>The trade</returns>
        /// <param name="symbol">Symbol</param>
        /// <param name="amountOfCoins">Amount of coins to buy</param>
		ITrade BuyMarket(string symbol, decimal amountOfCoins);

        /// <summary>
        /// Open a new sell order at market price
        /// </summary>
        /// <returns>The trade</returns>
        /// <param name="symbol">Symbol</param>
        /// <param name="amountOfCoins">Amount of coins to sell</param>
		ITrade SellMarket(string symbol, decimal amountOfCoins);

        /// <summary>
        /// Increase the trade by buying more coins
        /// </summary>
        /// <returns><c>true</c>, if more was bought, <c>false</c> otherwise.</returns>
        /// <param name="trade">Trade.</param>
        /// <param name="coins">amount of coins to buy</param>
		bool BuyMore(ITrade trade, decimal coins);

        /// <summary>
		/// Increase the trade by selling more coins.
        /// </summary>
        /// <returns><c>true</c>, if more was sold, <c>false</c> otherwise.</returns>
        /// <param name="trade">Trade.</param>
        /// <param name="coins">Coins.</param>
		bool SellMore(ITrade trade, decimal coins);

        /// <summary>
        /// Close the specified trade.
        /// </summary>
        /// <returns>The close.</returns>
		/// <param name="trade">Trade.</param>
        /// <param name="price">Price to close.</param>
		bool Close(ITrade trade, decimal price);


        /// <summary>
        /// Write statistics for all trades to the console
        /// </summary>
		void DumpStatistics();

        /// <summary>
        /// Trade history
        /// </summary>
        /// <value>List of all trades done.</value>
		List<ITrade> Trades { get; }


        /// <summary>
        /// Gets the account balance.
        /// </summary>
        /// <value>The account balance.</value>
		decimal AccountBalance { get; }
	}
}
