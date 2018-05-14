using System;
using System.Threading;
using DayTradeScanner;

namespace DayTrader.ConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var scanner = new Scanner("Bitfinex", "USD", 400000);
			while (true)
			{
				Console.WriteLine("scanning...");
				foreach (var symbol in scanner.Symbols)
				{
					var task = scanner.ScanAsync(symbol);
					task.Wait();
					var signal = task.Result;
					if (signal != null)
					{
						Console.WriteLine($"{signal.Symbol} {signal.Trade}");
					}
				}
				Console.WriteLine("wait...");
				Thread.Sleep(5000);
			}
		}
	}
}
