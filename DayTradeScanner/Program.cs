using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeSharp;

namespace DayTradeScanner
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Daytrader scanner 1.0 for Bitfinex");
			Console.WriteLine("Scanning 5min charts for daytrading signals on USD coins with a volume > $500,000");
			Task.Run(async () =>
		   {
			   var api = new ExchangeBitfinexAPI();
			   var allSymbols = await api.GetSymbolsAsync();


			   var symbols = new List<string>();
			   foreach (var symbol in allSymbols)
			   {
				   if (!symbol.Contains("USD"))
				   {
					   // only scan USD pairs
					   continue;
				   }
				   var ticker = await api.GetTickerAsync(symbol);
				   if (ticker.Volume.ConvertedVolume < 500000)
				   {
					   // skip coins with a 24hr trading volume < $1,000,000 dollar
					   continue;
				   }
					symbols.Add(symbol);
					Console.WriteLine($"Adding {symbol} to scan list");
			   }

			   var date = DateTime.Now;
			   while (true)
			   {
					Console.WriteLine($"scanning {0} symbols...", symbols.Count);
				   date = DateTime.Now;
				   foreach (var symbol in symbols)
				   {
					   var candles = (await api.GetCandlesAsync(symbol, 60 * 5, DateTime.Now.Date)).Reverse().ToList();

					   var bbands = new BollingerBands(candles);
					   var stoch = new Stochastics(candles);

					   if (bbands.Bandwidth >= 2m)
					   {
						   if (stoch.K < 20 && stoch.D < 20)
						   {
							   if (candles[0].ClosePrice < bbands.Lower)
							   {
								   Console.WriteLine($"{symbol} long signal");
							   }
						   }
						   else if (stoch.K > 80 && stoch.D > 80)
						   {
							   if (candles[0].ClosePrice > bbands.Upper)
							   {
								   Console.WriteLine($"{symbol} short signal");
							   }
						   }
					   }
				   }

				   Console.WriteLine("waiting...");
				   var ts = DateTime.Now - date;
				   while (ts.TotalMinutes < 5) await Task.Delay(5000);
			   }
		   }).Wait();
		}
	}
}
