using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TRADEPROUSOpen : Robot
    {
        [Parameter(DefaultValue = 10)]
        public double TP { get; set; }

        [Parameter(DefaultValue = 1000)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 180)]
        public double ExpiryMinutes { get; set; }

        [Parameter(DefaultValue = 10)]
        public int SL { get; set; }

        [Parameter(DefaultValue = 13)]
        public int UTChour { get; set; }

        [Parameter(DefaultValue = 30)]
        public int UTCminute { get; set; }

        public double price;
        public bool trade = false;
        public DateTime taken;

        protected override void OnStart()
        {
        }

        protected override void OnTick()
        {
            if ((Server.TimeInUtc.Month == 11 && Server.TimeInUtc.Day < 7) || Server.TimeInUtc.Month == 10)
            {
                if (Time.Hour == UTChour && Time.Minute == UTCminute)
                {
                    price = Bars.OpenPrices.LastValue;
                    trade = false;
                    taken = Server.TimeInUtc;
                }

                if (taken.AddMinutes(5) == Server.TimeInUtc)
                    trade = true;

                if (trade)
                {
                    if (Symbol.Ask < price)
                        PlaceLimitOrder(TradeType.Sell, SymbolName, Volume, price, "sell", SL, TP, taken.AddMinutes(ExpiryMinutes));
                    else
                        PlaceLimitOrder(TradeType.Buy, SymbolName, Volume, price, "buy", SL, TP, taken.AddMinutes(ExpiryMinutes));
                    trade = false;
                }
            }
        }

        protected override void OnBar()
        {
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
