    using System;
using System.Collections.Generic;
using DayTradeScanner.Bot.Interfaces;

namespace DayTradeScanner.Bot
{
	public interface ITrade
	{
		/// <summary>
		/// Gets or sets the symbol.
		/// </summary>
		/// <value>The symbol.</value>
		string Symbol { get; set; }

		/// <summary>
		/// Gets or sets the type of the trade (long or short)
		/// </summary>
		/// <value>The type of the trade.</value>
		TradeType TradeType { get; set; }

		/// <summary>
		/// Gets or sets the date the trade was opened.
		/// </summary>
		/// <value>The date the trade was opened.</value>
		DateTime OpenDate { get; set; }

		/// <summary>
		/// Gets or sets the open price.
		/// </summary>
		/// <value>The open price.</value>
		decimal OpenPrice { get; set; }

		/// <summary>
		/// Gets or sets the date the trade was closed
		/// </summary>
		/// <value>date the trade was closed.</value>
		DateTime CloseDate { get; set; }


		/// <summary>
		/// Gets or sets the close price.
		/// </summary>
		/// <value>The close price.</value>
		decimal ClosePrice { get; set; }

		/// <summary>
		/// Gets or sets the trade profit in %.
		/// </summary>
		/// <value>trade profit in %.</value>
		decimal ProfitPercentage { get; set; }

		/// <summary>
		/// Gets or sets the trade profit in dollars.
		/// </summary>
		/// <value>trade profit in dollars.</value>
		decimal ProfitDollars { get; set; }

		/// <summary>
		/// Gets or sets the total amount of dollars invested in this trade.
		/// </summary>
		/// <value>amount of dollars invested in this trade.</value>
		decimal Investment { get; set; }

		/// <summary>
		/// Gets or sets the amount of coins bought/sold in this trade.
		/// </summary>
		/// <value>The amount of coins bought/sold in this trade.</value>
		decimal Coins { get; set; }

		/// <summary>
		/// Gets or sets the total amount of fees paid for this trade
		/// </summary>
		/// <value>total amount of fees paid for this trade.</value>
		decimal FeesPaid { get; set; }

		/// <summary>
		/// Gets or sets the rebuys done for this trade
		/// </summary>
		/// <value>the number rebuys done for this trade.</value>
		List<IRebuy> Rebuys { get; set; }
	}
}
