using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CortiMulti : Robot
    {
        [Parameter("Group 1", DefaultValue = "GBPUSD,GBPJPY")]
        public string Group1 { get; set; }

        [Parameter("Group 2", DefaultValue = "USDJPY,USDJPY")]
        public string Group2 { get; set; }

        [Parameter("Equity TP", DefaultValue = 25)]
        public double EquityTP { get; set; }

        [Parameter("Start", DefaultValue = "Monday")]
        public string Start { get; set; }
        [Parameter("End", DefaultValue = "Thursday")]
        public string End { get; set; }

        [Parameter(DefaultValue = 360)]
        public int Cooldown { get; set; }

        [Parameter("Cost per pip", DefaultValue = 0.04)]
        public double CostPerPip { get; set; }

        public string[] days = 
        {
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday",
            "Sunday"
        };

        public DayOfWeek currentDay;
        public int startIndex;
        public int endIndex;
        public int currentIndex;

        public bool correctDay = false;
        public bool group1_buy = true;
        public bool inTrade = false;

        public int timeUntil = 0;

        public string currentDay_string;

        public DateTime desiredTime;

        public string[] array1 = new string[] 
        {
            ""
        };
        public string[] array2 = new string[] 
        {
            ""
        };

        protected override void OnStart()
        {
            CostPerPip = CostPerPip * 100000;
            desiredTime = Server.Time;

            if (Positions.Count > 0)
                inTrade = true;

            array1 = Group1.Split(',');
            array2 = Group2.Split(',');
        }
        protected override void OnTick()
        {

            if (Account.Balance + EquityTP <= Account.Equity)
                ClosePositions();

            if (CorrectTime() && !inTrade)
                PlaceOrders();
        }
        protected override void OnBar()
        {

        }
        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
        public void PlaceOrders()
        {
            if (group1_buy)
            {
                foreach (string pair in array1)
                {
                    ExecuteMarketOrder(TradeType.Buy, pair, CalculateVolume(pair));
                }
                foreach (string pair in array2)
                {
                    ExecuteMarketOrder(TradeType.Sell, pair, CalculateVolume(pair));
                }
                group1_buy = false;
                inTrade = true;
            }
            else
            {
                foreach (string pair in array1)
                {
                    ExecuteMarketOrder(TradeType.Sell, pair, CalculateVolume(pair));
                }
                foreach (string pair in array2)
                {
                    ExecuteMarketOrder(TradeType.Buy, pair, CalculateVolume(pair));
                }
                group1_buy = true;
                inTrade = true;
            }
        }
        public void ClosePositions()
        {
            foreach (var position in Positions)
            {
                ClosePositionAsync(position);
                inTrade = false;
            }
            desiredTime = Server.Time.AddMinutes(Cooldown);
        }
        public bool CorrectTime()
        {
            currentDay = Server.Time.DayOfWeek;
            currentDay_string = currentDay.ToString();

            startIndex = Array.IndexOf(days, Start);
            endIndex = Array.IndexOf(days, End);
            currentIndex = Array.IndexOf(days, currentDay_string);

            correctDay = (currentIndex >= startIndex && currentIndex <= endIndex);

            if (currentDay_string == Start)
            {
                if (Server.Time.Hour < 11)
                    return false;
            }

            int compare = DateTime.Compare(desiredTime, Server.Time);
            return (correctDay && compare < 0);
        }
        public double CalculateVolume(string pair)
        {
            var symbol = Symbols.GetSymbol(pair);
            double x = CostPerPip / symbol.PipValue;
            x = x / 10000;
            double result = x % 1000 >= 500 ? x + 1000 - x % 1000 : x - x % 1000;
            return result;
        }
    }
}
