using System;
namespace DayTradeScanner.Bot.Interfaces
{
	public interface IRebuy
	{
		decimal Investment { get; set; }
		decimal Coins { get; set; }
		decimal Price { get; set; }
		DateTime Date { get; set; }
	}
}
