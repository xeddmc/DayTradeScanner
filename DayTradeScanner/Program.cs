using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DayTradeScanner.Backtest;
using DayTradeScanner.Bot.Implementation;
using ExchangeSharp;
using Microsoft.Extensions.Configuration;

namespace DayTradeScanner
{
	class Program
	{
		static void Main(string[] args)
		{
			// Uncomment next 2 lines for backtesting
			// var tester = new BackTester();
			// tester.Test(new ExchangeBitfinexAPI(), "NEOUSD");

            
			var scanner = new Scanner();
			Task.Run(async () =>
			{
				await scanner.FindCoinsWithEnoughVolume();
				while (true)
				{
					await scanner.Scan();
                    await Task.Delay(5000);
				}
			}).Wait();
		}
	}
}
