using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Alexis : Robot
    {
        [Parameter("fastMA", DefaultValue = 10)]
        public int periodFast { get; set; }
        [Parameter("slowMA", DefaultValue = 20)]
        public int periodSlow { get; set; }
        [Parameter("Source")]
        public DataSeries SourceSeries { get; set; }
        private ExponentialMovingAverage slowMa;
        private ExponentialMovingAverage fastMa;

        [Parameter("BB", DefaultValue = 20)]
        public int periodBB { get; set; }
        [Parameter("Standard deviations", DefaultValue = 2)]
        public int stD { get; set; }
        [Parameter("MA Type", DefaultValue = "simple")]
        public MovingAverageType typeMA { get; set; }

        [Parameter("SL1", DefaultValue = 50)]
        public double SL1 { get; set; }
        [Parameter("SL2", DefaultValue = 100)]
        public double SL2 { get; set; }

        [Parameter("Risk%", DefaultValue = 2)]
        public double StopLossRisk { get; set; }

        public BollingerBands bollingerBands;

        public string traded = "none";

        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(SourceSeries, periodBB, stD, typeMA);

            fastMa = Indicators.ExponentialMovingAverage(SourceSeries, periodFast);
            slowMa = Indicators.ExponentialMovingAverage(SourceSeries, periodSlow);
        }

        protected override void OnTick()
        {
            if (fastMa.Result.LastValue > slowMa.Result.LastValue && Symbol.Ask <= bollingerBands.Bottom.LastValue && traded == "none" && Positions.Count == 0)
            {
                traded = "buy";
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume(SL1), "buy1", SL1, 0);
                ExecuteMarketOrder(TradeType.Buy, SymbolName, Volume(SL2), "buy2", SL2, 0);
            }

            else if (fastMa.Result.LastValue < slowMa.Result.LastValue && Symbol.Ask >= bollingerBands.Top.LastValue && traded == "none" && Positions.Count == 0)
            {
                traded = "sell";
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume(SL1), "sell1", SL1, 0);
                ExecuteMarketOrder(TradeType.Sell, SymbolName, Volume(SL2), "sell2", SL2, 0);
            }

            if (Symbol.Ask <= bollingerBands.Main.LastValue && Positions.Count == 2 && traded == "sell")
            {
                traded = "sellprofit";
                Positions.Find("sell2").ModifyStopLossPips(-2);
                Positions.Find("sell1").Close();
            }

            if (Symbol.Ask >= bollingerBands.Main.LastValue && Positions.Count == 2 && traded == "buy")
            {
                traded = "buyprofit";
                Positions.Find("buy2").ModifyStopLossPips(-2);
                Positions.Find("buy1").Close();
            }

            if (Positions.Count == 1 && (traded == "sell" || traded == "buy"))
                Positions.Find(traded + "2").ModifyTakeProfitPrice(bollingerBands.Main.LastValue);

            if (Positions.Count == 1 && traded == "buyprofit" && Symbol.Ask >= bollingerBands.Top.LastValue)
                Positions.Find("buy2").Close();

            if (Positions.Count == 1 && traded == "sellprofit" && Symbol.Ask <= bollingerBands.Bottom.LastValue)
            {
                Positions.Find("sell2").Close();
            }

            Print(bollingerBands.Bottom.LastValue);
        }

        protected override void OnBar()
        {
            if (Server.TimeInUtc.Hour == 21 && Positions.Count == 0)
                traded = "none";
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        double Volume(double pips)
        {
            double costPerPip = (double)((int)(Symbol.PipValue * 10000000)) / 100;
            double positionSizeForRisk = (Account.Balance * StopLossRisk / 100) / (pips * costPerPip);

            var lots = (Math.Round(positionSizeForRisk, 2));

            return lots * 100000;
        }
    }
}
