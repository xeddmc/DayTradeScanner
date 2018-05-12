using System;
using System.Collections.Generic;
using System.IO;
using ExchangeSharp;
using MessagePack;

namespace DayTradeScanner
{
	[MessagePackObject]
	public class CacheCandle
	{
		[Key(0)]
		public DateTime Date { get; set; }
		[Key(1)]
		public decimal Open { get; set; }
		[Key(2)]
		public decimal Close { get; set; }
		[Key(3)]
		public decimal High { get; set; }
		[Key(4)]
		public decimal Low { get; set; }

		public CacheCandle()
		{
		}

		public CacheCandle(MarketCandle candle)
		{
			Date = candle.Timestamp;
			Open = candle.OpenPrice;
			Close = candle.ClosePrice;
			High = candle.HighPrice;
			Low = candle.LowPrice;
		}

		public MarketCandle ToMarketCandle()
		{
			return new MarketCandle()
			{
				Timestamp = Date,
				OpenPrice = Open,
				ClosePrice = Close,
				HighPrice = High,
				LowPrice = Low
			};
		}
	}

	[MessagePackObject]
	public class CacheCandleList
	{
        [Key(0)]
		public CacheCandle[] Candles;
	}

    public class Cache
    {
		public List<MarketCandle> LoadCandlesFromCache(string symbol)
        {
            if (File.Exists($"{symbol}-candles.dat"))
            {
                // load candles from disk
                using (var file = File.OpenRead($"{symbol}-candles.dat"))
                {
                    var bytes = new byte[file.Length];
                    file.Read(bytes, 0, bytes.Length);
					var cache = MessagePackSerializer.Deserialize<CacheCandleList>(bytes);

					var result = new List<MarketCandle>();
					foreach(var candle in cache.Candles){
						result.Add(candle.ToMarketCandle());
					}
					return result;
                }
            }
            return null;
        }

        public void SaveCandlesToCache(string symbol, List<MarketCandle> marketCandles)
        {
			// store candles on disk
			var cache = new CacheCandleList();
			int idx = 0;
			cache.Candles = new CacheCandle[marketCandles.Count]; 
			foreach(var candle in marketCandles){
				cache.Candles[idx++] = new CacheCandle(candle);

			}
            var bytes = MessagePackSerializer.Serialize(cache);
            using (var file = File.OpenWrite($"{symbol}-candles.dat"))
            {
                file.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
