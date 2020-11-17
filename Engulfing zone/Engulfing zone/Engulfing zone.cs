using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Engulfingzone : Robot
    {
        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int Pips { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnBar()
        {
            if (bullish_engulfing())
            {
                PlaceLimitOrder(TradeType.Buy, SymbolName, Volume, Bars.OpenPrices.Last(2), "buy", calculate(), calculate());
            }
        }

        bool bullish_engulfing()
        {
            if (Bars.ClosePrices.Last(2) < Bars.OpenPrices.Last(2) && Bars.ClosePrices.Last(1) > Bars.OpenPrices.Last(1))
            {
                if (Bars.ClosePrices.Last(1) > Bars.HighPrices.Last(2) && (Bars.ClosePrices.Last(1) - Bars.OpenPrices.Last(1)) / Symbol.PipSize > Pips)
                    return true;
            }
            return false;
        }

        double calculate()
        {
            double lowest = Math.Min(Bars.LowPrices.Last(1), Bars.LowPrices.Last(2));
            Print(Bars.ClosePrices.Last(1));
            Print(lowest);
            return (Bars.OpenPrices.Last(2) - lowest) / Symbol.PipSize;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
