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
			// Uncomment next  lines for backtesting
            /*
            var virtualTradeManager = new VirtualTradeManager();
			var strategy = new DayTradingStrategy("NEOUSD");
			var tester = new BackTester();
			tester.Test(new ExchangeBitfinexAPI(),  strategy);
            */
            
			var scanner = new Scanner();
			Task.Run(async () =>
			{
				await scanner.FindCoinsWithEnoughVolumeAsync();
				while (true)
				{
					await scanner.ScanAsync();
                    await Task.Delay(5000);
				}
			}).Wait();
		}
	}
}
