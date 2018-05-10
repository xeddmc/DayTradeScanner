using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeSharp;

namespace DayTradeScanner
{
	public class BollingerBands
	{
		public BollingerBands(List<MarketCandle> candles, int length = 20, int deviations = 2)
		{
			var prices = candles.Select(e => e.ClosePrice).Take(length).ToList();
            var sd = GetStandardDeviation(prices);

			Middle = prices.Average();
			Lower = Middle - deviations * sd;
			Upper = Middle + deviations * sd;

			Bandwidth = ((Upper - Lower) / Middle) * 100.0m;
		}

		public decimal Bandwidth { get; internal set; }
		public decimal Lower { get; internal set; }
		public decimal Middle { get; internal set; }
        public decimal Upper { get; internal set; }

		private decimal GetStandardDeviation(List<decimal> numberSet)
		{
            var mean = numberSet.Average();
            
			return (decimal)Math.Sqrt(numberSet.Sum(x => Math.Pow((double)(x - mean), 2)) / numberSet.Count);
		}
	}
}
