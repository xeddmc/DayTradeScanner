using DayTradeScanner;
using DayTradeScanner.Backtest;
using DayTradeScanner.Bot.Implementation;
using ExchangeSharp;

namespace Backtester.Console.App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // create a virtual trade manager
            var virtualTradeManager = new VirtualTradeManager();

            // create day trading strategy
            var strategy = new DayTradingStrategy("NEOUSD");

            // create new backtester
            var tester = new BackTester();

            // test the strategy on bitfinex
            tester.Test(new ExchangeBitfinexAPI(), strategy);
        }
    }
}