using System;
using System.Collections.Generic;
using DayTradeScanner.Bot;
using DayTradeScanner.Bot.Interfaces;

namespace DayTradeScanner.Backtest
{
	public class VirtualTrade : ITrade
	{
		public VirtualTrade()
		{
			Rebuys = new List<IRebuy>();
		}

		/// <summary>
		/// Gets or sets the symbol.
		/// </summary>
		/// <value>The symbol.</value>
		public string Symbol { get; set; }

		/// <summary>
		/// Gets or sets the type of the trade (long or short)
		/// </summary>
		/// <value>The type of the trade.</value>
		public TradeType TradeType { get; set; }

		/// <summary>
		/// Gets or sets the date the trade was opened.
		/// </summary>
		/// <value>The date the trade was opened.</value>
		public DateTime OpenDate { get; set; }

		/// <summary>
		/// Gets or sets the open price.
		/// </summary>
		/// <value>The open price.</value>
		public decimal OpenPrice { get; set; }

		/// <summary>
		/// Gets or sets the date the trade was closed
		/// </summary>
		/// <value>date the trade was closed.</value>
		public DateTime CloseDate { get; set; }


		/// <summary>
		/// Gets or sets the close price.
		/// </summary>
		/// <value>The close price.</value>
		public decimal ClosePrice { get; set; }

		/// <summary>
		/// Gets or sets the trade profit in %.
		/// </summary>
		/// <value>trade profit in %.</value>
		public decimal ProfitPercentage { get; set; }

		/// <summary>
		/// Gets or sets the trade profit in dollars.
		/// </summary>
		/// <value>trade profit in dollars.</value>
		public decimal ProfitDollars { get; set; }

		/// <summary>
		/// Gets or sets the total amount of dollars invested in this trade.
		/// </summary>
		/// <value>amount of dollars invested in this trade.</value>
		public decimal Investment { get; set; }

		/// <summary>
		/// Gets or sets the amount of coins bought/sold in this trade.
		/// </summary>
		/// <value>The amount of coins bought/sold in this trade.</value>
		public decimal Coins { get; set; }

		/// <summary>
		/// Gets or sets the total amount of fees paid for this trade
		/// </summary>
		/// <value>total amount of fees paid for this trade.</value>
		public decimal FeesPaid { get; set; }

		public decimal InitialCoins { get; set; }
		public decimal InitialInvestment { get; set; }
		public List<IRebuy> Rebuys { get; set; }

		public void Dump()
		{
			Console.WriteLine($"{OpenDate:dd-MM-yyyy HH:mm}/{CloseDate:dd-MM-yyyy HH:mm} {TradeType} open:{OpenPrice:0.000000} close: {ClosePrice:0.000000}  profit:{ProfitPercentage:0.00} %");
			Console.WriteLine($"    buy #1: coins:{InitialCoins:0.0000} ${InitialInvestment:0.00}");
			foreach (var rebuy in Rebuys)
			{
				Console.WriteLine($"  rebuy : {rebuy.Date:dd-MM-yyyy HH:mm} {rebuy.Price:0.000000} coins:{rebuy.Coins:0.0000} ${rebuy.Investment:0.00}");
			}
		}
	}
}
