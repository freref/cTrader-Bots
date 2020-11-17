using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PredatorBox : Robot
    {
        [Parameter(DefaultValue = 20)]
        public double StopLoss { get; set; }

        [Parameter(DefaultValue = 20)]
        public double TakeProfit { get; set; }

        [Parameter(DefaultValue = 1)]
        public double RiskPercentage { get; set; }

        public bool posTrade = true;

        public bool trade = false;

        public bool sell = false;

        public bool buy = false;

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnBar()
        {
            Bars bars = MarketData.GetBars(TimeFrame.Minute5);
            DateTime time = Server.TimeInUtc;
            time = time.AddMinutes(-4);
            int index = bars.OpenTimes.GetIndexByTime(time);
            Bar bar = bars[index];
            double low = CalculateLow();
            double high = CalculateHigh();
            double costPerPip = (double)((int)(Symbol.PipValue * 10000000)) / 100;
            double risk = (Account.Balance * RiskPercentage / 100) / (StopLoss * costPerPip);

            risk = Math.Round(risk, 2) * 100000;

            Print("------");
            Print("high: " + high);
            Print("low: " + low);
            Print("close: " + bar.Close);
            Print("open: " + bar.Open);
            Print("bar high: " + bar.High);
            Print("bar low: " + bar.Low);
            Print("------");

            if (Server.TimeInUtc.Hour >= 7 & Server.TimeInUtc.Hour <= 12)
            {
                posTrade = false;
            }
            else
            {
                posTrade = true;
                trade = false;
            }

            if (posTrade == false & trade == false)
            {
                if (bar.High < low && bar.Low < low)
                {
                    if (bar.Close < low && bar.Open < low)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Chart.SymbolName, risk, "sell", StopLoss, TakeProfit);
                        trade = true;
                    }
                }

                if (bar.High > high && bar.Low > high && bar.Close > high && bar.Open > high)
                {
                    ExecuteMarketOrder(TradeType.Buy, Chart.SymbolName, risk, "buy", StopLoss, TakeProfit);
                    trade = true;
                }
            }

        }

        private double CalculateHigh()
        {
            Bars series = MarketData.GetBars(TimeFrame.Hour);
            DateTime now = Server.TimeInUtc;
            DateTime boxTime;

            now = now.AddMilliseconds(-now.Millisecond);
            now = now.AddSeconds(-now.Second);
            now = now.AddMinutes(-now.Minute);

            boxTime = now.AddHours(-now.Hour).AddHours(6);

            int index = series.OpenTimes.GetIndexByTime(boxTime);

            return series[index].High;
        }

        private double CalculateLow()
        {
            Bars series = MarketData.GetBars(TimeFrame.Hour);
            DateTime now = Server.TimeInUtc;
            DateTime boxTime;

            now = now.AddMilliseconds(-now.Millisecond);
            now = now.AddSeconds(-now.Second);
            now = now.AddMinutes(-now.Minute);

            boxTime = now.AddHours(-now.Hour).AddHours(6);

            int index = series.OpenTimes.GetIndexByTime(boxTime);

            return series[index].Low;
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
