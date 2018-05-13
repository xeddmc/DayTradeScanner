using System;
using DayTradeScanner.Bot;

namespace DayTradeScanner.Backtest
{
	public class VirtualTrade : ITrade
	{
		public VirtualTrade()
		{
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

		/// <summary>
		/// Gets or sets the number rebuys done in this trade
		/// </summary>
		/// <value>the number rebuys done in this trade.</value>
		public int RebuyCount { get; set; }

		public decimal InitialCoins { get; set; }
		public decimal InitialInvestment { get; set; }

		public decimal RebuyInvestment1 { get; set; }
		public decimal RebuyCoins1 { get; set; }
		public decimal RebuyPrice1 { get; set; }
		public DateTime RebuyDate1 { get; set; }

		public decimal RebuyInvestment2 { get; set; }
		public decimal RebuyCoins2 { get; set; }
		public decimal RebuyPrice2 { get; set; }
		public DateTime RebuyDate2 { get; set; }


		public void Dump()
		{
			Console.WriteLine($"{OpenDate:dd-MM-yyyy HH:mm}/{CloseDate:dd-MM-yyyy HH:mm} {TradeType} open:{OpenPrice:0.000000} close: {ClosePrice:0.000000}  profit:{ProfitPercentage:0.00} %");
			Console.WriteLine($"    buy #1: coins:{InitialCoins:0.0000} ${InitialInvestment:0.00}");
			switch (RebuyCount)
			{
				case 0:
					break;

				case 1:
					Console.WriteLine($"  rebuy #1: {RebuyDate1:dd-MM-yyyy HH:mm} {RebuyPrice1:0.000000} coins:{RebuyCoins1:0.0000} ${RebuyInvestment1:0.00}");
					break;

				case 2:
					Console.WriteLine($"  rebuy #1: {RebuyDate1:dd-MM-yyyy HH:mm} {RebuyPrice1:0.000000} coins:{RebuyCoins1:0.0000} ${RebuyInvestment1:0.00}");
					Console.WriteLine($"  rebuy #2: {RebuyDate2:dd-MM-yyyy HH:mm} {RebuyPrice2:0.000000} coins:{RebuyCoins2:0.0000} ${RebuyInvestment2:0.00}");
					break;
			}
		}
	}
}
