using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Clients;
using Binance.Net.Enums;
using CryptoExchange.Net.Authentication;
using Skender.Stock.Indicators;
using Binance.Net.Interfaces;
using bsk = Binance.Net.Objects.Models.Spot.BinanceSpotKline;
using MathNet.Numerics;
using System.Threading;
using System.Timers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using LinqStatistics;
using Accord.Statistics.Models.Regression.Linear;
using Binance.Net.Objects;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;
using MathNet.Numerics.Distributions;
using System.Security.Cryptography;
using Accord;
using System.Globalization;
using System.Collections.ObjectModel;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.CommonObjects;
using Binance.Net.Objects.Options;
using CryptoExchange.Net.Objects.Options;
using Binance.Net;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.Defaults;
using SkiaSharp;
using System.Windows.Forms;

namespace BinanceBotStrategy
{
    static class Program
    {
        enum FVGType
        {
            MagnetGapDeepRetracement,
            MagnetGapConsolidation,
            BreakAwayGapExpansion,
            BreakAwayGapSneaky,
            MagnetGapSimple
        }
        enum TradeType
        {
            Long,
            Short
        }
        enum PositionType
        {
            Market,
            Limit
        }
        enum BiasType
        {
            Bullish,
            Bearish
        }
        enum EventType
        {
            PivotHigh,
            PivotLow,
            BOSUP,
            BOSDOWN,
            CHoCHUP,
            CHoCHDOWN
        }

        enum VolumeNodeType
        {
            HVN,
            LVN
        }

        class EntryModelTF
        {
            public KlineInterval HighKlineInterval { get; set; }
            public KlineInterval LowKlineInterval { get; set; }
        }
        class FVG
        {
            public IBinanceKline Kline { get; set; }
            public BiasType BiasType { get; set; }
            public bool Mitigated { get; set; }
        }

        class OpenOrder
        {
            public string Symbol { get; set; }
            public decimal Size { get; set; }
            public decimal EntryPrice { get; set; }
            public decimal StopLoss { get; set; }
            public decimal Target { get; set; }
            public DateTime OpenTime { get; set; }
            public TradeType TradeType { get; set; }
        }

        class Position : OpenOrder
        {
            public decimal PNL { get; set; }
            public bool Won { get; set; }
            public decimal Fees { get; set; }
            public decimal CloseTime { get; set; }
            public EntryModelTF EntryTimeframe { get; set; }
        }

        class FairValueGap
        {
            public KlineInterval FVGTimeframe { get; set; }
            public DateTime FVGOpenTime { get; set; }
            public DateTime FVGCloseTime { get; set; }
            public List<IBinanceKline> Structure { get; set; }
            public decimal FvgAreaLow { get; set; }
            public decimal FVGAreaMiddle { get; set; }
            public decimal FvgAreaHigh { get; set; }
            public decimal TopStructure { get; set; }
            public decimal BottomStructure { get; set; }
            public BiasType BiasType { get; set; }
            /// <summary>
            /// Signifies if the FVG has been "forgotten" by another FVG on the same timeframe created after it
            /// </summary>
            public bool IsForgotten { get; set; }
            /// <summary>
            /// Signifies if the FVG has been mitigated by the price crossing its bottom
            /// </summary>
            public bool IsMitigated { get; set; }
            /// <summary>
            /// Signifies if the Liquidity of an FVG was already taken after the price goes inside it and taps the FVG area
            /// </summary>
            public bool IsLiquidityTakenAfterTap { get; set; }
            /// <summary>
            /// Signifies if the FVG has its FVG area tapped
            /// </summary>
            public bool IsTapped { get; set; }
            public FVGType FVGType { get; set; }
        }

        class VolumeNode
        {
            public Tuple<decimal, decimal> Interval { get; set; }
            public VolumeNodeType Type { get; set; }
        }

        class LatePivot
        {
            public IBinanceKline Candle { get; set; }
            public int ReverseCandlesAhead { get; set; }
        }
        class MarketStructure
        {
            public EventType CurrEvent { get; set; }
            public DateTime Date { get; set; }
            public decimal Price { get; set; }
            public BiasType CurrBias { get; set; }            
        }

        class fvgFutureEntry
        {
            public decimal? EntryPrice { get; set; }
            public decimal? StopLoss { get; set; }
            public decimal? TakeProfit { get; set; }
            public DateTime InitialDate { get; set; }
            public string Type { get; set; }
            public TradeType TradeType { get; set; }
            public PositionType PositionType { get; set; }
            public bool Invalidated { get; set; }
            public decimal PositionSize { get; set; }
            public EntryModelTF EntryModelTF { get; set; }         
            public DateTime HTF_FVG_CANDLE_TIME { get; set; }
        }

        class fibRatios
        {
            public decimal ret0 { get; set; }
            public decimal ret0236 { get; set; }
            public decimal ret0382 { get; set; }
            public decimal ret05 { get; set; }
            public decimal ret0618 { get; set; }
            public decimal ret065 { get; set; }
            public decimal ret0705 { get; set; }
            public decimal ret1 { get; set; }
            public decimal vpSum { get; set; }
        }

        class futureEntry
        {
            public decimal? entryPrice { get; set; }
            public decimal? FVGEntryPrice { get; set; }            
            public decimal? FVGStopLoss { get; set; }
            public DateTime FVGStopLossTime { get; set; }
            public DateTime InitialDate { get; set; }
            public DateTime Ret1Date { get; set; }
            public decimal? ret0 { get; set; }
            public decimal? ret0236 { get; set; }
            public decimal? ret0382 { get; set; }
            public decimal? ret05 { get; set; }
            public decimal? ret0618 { get; set; }
            public decimal? ret065 { get; set; }
            public decimal? ret0786 { get; set; }
            public decimal? ret1 { get; set; }
            public decimal? ret1618 { get; set; }
            public DateTime topFib { get; set; }
            public decimal rsiRet0 {get; set;}
            public decimal rsiMom { get; set; }
            public bool Invalidated { get; set; }
            public string Bias { get; set; }
        }

        class futureEntrySt2
        {
            public decimal? entryPrice { get; set; }
            public decimal? targetPrice { get; set; }
            public DateTime InitialDate { get; set; }
            public DateTime Ret1Date { get; set; }
            public decimal? ret0 { get; set; }
            public decimal? retMinus0618 { get; set; }
            public decimal? ret0764 { get; set; }
            public decimal? ret1 { get; set; }
            public bool Invalidated { get; set; }
        }
        class zzResult
        {
            public Quote buffer { get; set; }
            public decimal lowBuffer { get; set; }
            public decimal highBuffer { get; set; }
            public decimal zigZagBuffer { get; set; }
            public DateTime date { get; set; }
            public string type { get; set; }
        }
        class openFuture
        {
            public DateTime startTime { get; set; }
            public decimal price { get; set; }
            public decimal targetPrice { get; set; }
            public decimal? stopLimitPrice { get; set; }
            public decimal? targetRatioOne { get; set; }
            public decimal riskRatio { get; set; }
            public string type { get; set; }
            public decimal? slDiff { get; set; }
            public decimal? rsi { get; set; }
            public decimal slTpFactor { get; set; }
            public bool liquidated { get; set; }
            public decimal investment { get; set; }
        }

        public class webhookEvent
        {
            public string uuid { get; set; }
            public string content { get; set; }
            public DateTime createdAt { get; set; }
            public bool used { get; set; }
            public string dotColor { get; set; }
            public double moneyFlow { get; set; }
            public double cloud1 { get; set; }
            public double cloud2 { get; set; }
        }

        class ProfitTarget
        {
            public decimal Price { get; set; }
            public decimal Quantity { get; set; }
            public bool Alive { get; set; }
        }
        class binanceOpenFuture
        {
            public DateTime startTime { get; set; }
            public decimal price { get; set; }
            public decimal targetPrice { get; set; }
            public decimal? stopLimitPrice { get; set; }
            public decimal riskRatio { get; set; }
            public string type { get; set; }
            public long id { get; set; }
            public decimal Quantity { get; set; }              
            public decimal EntryPrice { get; set; }
            public decimal? ret0 { get; set; }
            public decimal rsiRet0 { get; set; }
            public decimal rsiMom { get; set; }
            public List<ProfitTarget> ProfitTargets { get; set; } //Profit Target 1
        }

        class positionHistory
        {
            public binanceOpenFuture posData { get; set; }
            public int hitTargets { get; set; }
            public decimal profit { get; set; }
            public bool stoppedLoss { get; set; }
        }

        class insaneEntry
        {
            public DateTime time { get; set; }
            public string type { get; set; }
        }

        class BPR
        {
            public int Low { get; set; }
            public int High { get; set; }
            public int Depth { get; set; }
        }


        class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        class VPRetracementEntry
        {
            public decimal Ret1 { get; set; }
            public DateTime Ret1Time { get; set; }
            public decimal Ret0 { get; set; }
            public DateTime Ret0Time { get; set; }
            public DateTime FoundTime { get; set; }
            public decimal EntryPrice { get; set; }
            public decimal StopLoss { get; set; }
            public decimal TargetPrice { get; set; }
            public TradeType Type { get; set; }
            public bool Used { get; set; }
        }        

        class Swing
        {
            private IBinanceKline binanceKline;
            private int type;

            public Swing(IBinanceKline binanceKline, int type)
            {
                this.binanceKline = binanceKline;
                this.type = type;
            }
        }

        //public static decimal investment = 0.005M; //$300
        public static decimal investment = 0.3M;//0.018M; //$300
        public static decimal AccountValue = 1000;
        public static decimal RiskPercent = 3;
        public static int losses = 0;
        public static int wins = 0;
        public static decimal? totalProfit = 0;
        public static decimal? fee = 0.03M;
        public static decimal? takerFee = 0.017M;
        public static decimal? makerFee = 0.000M;
        public static decimal rr = 2m;
        static List<Position> OpenPositions = new List<Position>();
        static List<Position> PastPositions = new List<Position>();

        // Ex: collection.TakeLast(5);
        static Program()
        {
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        async static Task Main(string[] args)
        {
            Thread th = new Thread(priceSocket);
            th.Start();

            //vpt.Start();

            //Thread who = new Thread(GetHooks);
            //who.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            // If you need to set a custom base address, configure the HttpClient used by the BinanceClient
            //client.UsdFuturesApi.SetHttpClient(new HttpClient { BaseAddress = new Uri("https://fapi.binance.com") });


            //var lineDataOneMin = await GetAllKlinesInterval("BTCUSDC", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow);
            //var lineData = await GetAllKlinesInterval("BTCUSDC", KlineInterval.ThirtyMinutes, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow);

            Dictionary<KlineInterval, List<IBinanceKline>> timeFrameData = new Dictionary<KlineInterval, List<IBinanceKline>>();
            timeFrameData = await getAllData(timeFrameData, 1100, "BTCUSDT");

            //BACKTEST13_FVG_GALORE(timeFrameData, 80);
            //BACKTEST14_FVG_ENHANCED(timeFrameData, 50);
            BACKTEST15_FVG_ENHANCED2(timeFrameData, 1100);
            //FIB_OVER_VP(timeFrameData[KlineInterval.FiveMinutes], new DateTime(2024, 5, 30, 12, 0, 0));   
            //var lineData = await GetAllKlinesInterval("BTCUSDC", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-1).AddHours(0), DateTime.UtcNow);
            //BACKTEST16_Stochastic_Quad_Rotation(lineData);

            var kkk = 0;

            //var lineDataDaily = await GetAllKlinesInterval("BTCUSDT", KlineInterval.OneDay, DateTime.UtcNow.AddDays(-100), DateTime.UtcNow);

            //IdentifyUnmitigatedFVGs(lineDataDaily);
            //var lineData = await GetAllKlinesInterval("BTCUSDT", KlineInterval.OneMinute, DateTime.UtcNow.AddMinutes(-35), DateTime.UtcNow);

            /*volumeProfileCalculation(lineDataOneMin.Where(it 
                => it.OpenTime >= new DateTime(2023, 12, 20, 15, 14, 0) 
                && it.OpenTime <= new DateTime(2023, 12, 26, 21, 0, 0)).ToList());*/
            /*
            decimal ret0 = 39600;
            var vp = volumeProfileCalculation(lineDataOneMin.Where(it
                => it.OpenTime >= new DateTime(2024, 1, 11, 14, 51, 0)
                && it.OpenTime <= new DateTime(2025, 12, 26, 21, 0, 0)).ToList(), write: false, ret0: ret0);
            decimal ret1 = lineDataOneMin.Where(it
                => it.OpenTime >= new DateTime(2024, 1, 11, 14, 51, 0)
                && it.OpenTime <= new DateTime(2025, 12, 26, 21, 0, 0)).Max(it => it.HighPrice);


            DisplayVolumeProfileWithRatios(vp, ret1, ret0);
            */

            //BPRFinder(lineData);

            /*FinonacciFinder(lineData
                                    , quickswitch: false
                                    , oneLevel: 43450.1m
                                    , direction: -1
                                    , startTime: new DateTime(2023, 12, 14, 17, 48, 0)
                                    , endTime: new DateTime(2023, 12, 16, 1, 45, 0));*/

            //await liveStrat10TheOne();
            //await liveStrat10TheOne();
            //await liveStrat11VolumeProfileReversalFib();
            //BACKTEST10TheOne(lineData, lineDataOneMin);
            //BACKTEST12TheOne_FibProfile(lineData, lineDataOneMin);
            //BACKTEST11(lineData, lineDataOneMin);
            //BACKTEST12_FVG_NY_OPEN(lineData, lineDataOneMin);
            //BACKTEST11TheOneSMC(lineData, lineDataOneMin);


            var ttt = PastPositions.Count();

            var groupedPastPositions = PastPositions
                .GroupBy(p => p.EntryTimeframe.HighKlineInterval)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var group in groupedPastPositions)
            {
                var totalTrades = group.Value.Count;
                var winningTrades = group.Value.Count(p => p.Won);
                var winRate = (decimal)winningTrades / totalTrades * 100;

                Console.WriteLine($"Kline Interval: {group.Key}");
                Console.WriteLine($"Total Trades: {totalTrades}");
                Console.WriteLine($"Winning Trades: {winningTrades}");
                Console.WriteLine($"Win Rate: {winRate:F2}%");
            }

            Console.WriteLine($"Wins: {wins} | Losses: {losses} | Total Profit: {totalProfit} | Percent winrate: {Math.Round((double)wins / (wins + losses) * 100, 2)}%");

            //foreach (var child in winTradesRsi) Console.Write($"{Math.Round((double)child, 1)},");
            //Console.WriteLine();
            //foreach (var child in lossTradesRsi) Console.Write($"{Math.Round((double)child, 1)},");

            //Console.WriteLine(winTradesRsi.Average());
            //Console.WriteLine(lossTradesRsi.Average());

            List<EmaResult> ema20 = null;
            List<EmaResult> ema50 = null;
            List<EmaResult> ema100 = null;
            List<FractalResult> willf = null;
            List<AdxResult> adx = null;
            List<openFuture> openPositions = new List<openFuture>();

            bool candleStarted = false;
            Console.ReadLine();
        }
        
        private static void FIB_OVER_VP(List<IBinanceKline> data, DateTime startDate)
        {
            var lineData = data.Where(it => it.OpenTime >= startDate).ToList();

            var VP = GenerateVolumeProfile(lineData, 1000);

            PlotVolumeProfileWithCandles(VP, lineData);
            var fff = 0;
        }
        
        private static void PlotVolumeProfileWithCandles(List<VolumeProfile> volumeProfile, List<IBinanceKline> lineData)
        {
            var cartesianChart = new CartesianChart
            {
                Dock = DockStyle.Fill
            };

            // Create the volume profile series
            var areaSeries = new LineSeries<Point>
            {
                Values = volumeProfile.Select(vp => new Point
                {
                    X = (double)vp.sum,
                    Y = (double)((vp.interval.Item1 + vp.interval.Item2) / 2)
                }).ToList(),
                Fill = new SolidColorPaint(new SKColor(135, 206, 250, 90)) // LightBlue with transparency
            };

            cartesianChart.Series = new ISeries[] { areaSeries };
            
            // Configure the X axis as volume
            cartesianChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Volume",
                    MinLimit = 0,
                    MaxLimit = volumeProfile.Max(vp => (double)vp.sum)
                }
            };

            // Configure the Y axis as price
            cartesianChart.YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Price",
                    MinLimit = (double)lineData.Min(k => k.LowPrice),
                    MaxLimit = (double)lineData.Max(k => k.HighPrice)
                }
            };

            var form = new Form
            {
                Text = "Volume Profile with Candles",
                Width = 1920,
                Height = 1080
            };
            form.Controls.Add(cartesianChart);
            Application.Run(form);
        }
        
        static List<VolumeProfile> GenerateVolumeProfile(List<IBinanceKline> lineData, int stepLevels)
        {
            List<VolumeProfile> volumeProfile = new List<VolumeProfile>();

            decimal minPrice = lineData.Min(k => k.LowPrice);
            decimal maxPrice = lineData.Max(k => k.HighPrice);
            decimal stepSize = (maxPrice - minPrice) / stepLevels;

            for (decimal price = minPrice; price <= maxPrice; price += stepSize)
            {
                decimal lowerBound = price;
                decimal upperBound = price + stepSize;
                decimal volumeSum = lineData
                    .Where(k => k.LowPrice <= upperBound && k.HighPrice >= lowerBound)
                    .Sum(k => k.Volume);

                volumeProfile.Add(new VolumeProfile
                {
                    interval = Tuple.Create(lowerBound, upperBound),
                    sum = volumeSum,
                    isInterestZone = false
                });
            }

            return volumeProfile;
        }

        private static List<ZigZagResult> GetStochZigZag(List<StochResult> stochResults)
        {
            var zigZagResults = new List<ZigZagResult>();
            for (int i = 1; i < stochResults.Count - 1; i++)
            {
                if (stochResults[i].K > stochResults[i - 1].K && stochResults[i].K > stochResults[i + 1].K)
                {
                    // Local maximum
                    zigZagResults.Add(new ZigZagResult(stochResults[i].Date)
                    {
                        PointType = "H",
                        ZigZag = (decimal)stochResults[i].K
                    });
                }
                else if (stochResults[i].K < stochResults[i - 1].K && stochResults[i].K < stochResults[i + 1].K)
                {
                    // Local minimum
                    zigZagResults.Add(new ZigZagResult(stochResults[i].Date)
                    {
                        PointType = "L",
                        ZigZag = (decimal)stochResults[i].K
                    });
                }
            }
            return zigZagResults;
        }

        private static BiasType? DetectDivergence(IEnumerable<Quote> quotes, IEnumerable<StochResult> stochResults, int lookbackPeriod = 5)
        {
            // Calculate the ZigZag for Stochastic Oscillator values

            var stochZigZag = GetStochZigZag(stochResults.ToList());
            var stochZigZagHighs = stochZigZag.Where(it => it.PointType == "H").ToList();
            var stochZigZagLows = stochZigZag.Where(it => it.PointType == "L").ToList();

            // Get the current ZigZag points
            var currentStochZigZag = stochZigZag.Last();

            // Check for Bullish Divergence

            //need to change the following to check the price for the quoteLast < the quote that is on the same date as the stochZigZagLow 
            bool isBullishDivergence = false;
            for (int j = 1; j <= lookbackPeriod; j++)
            {
                var stochZigZagPoint = stochZigZagLows[stochZigZagLows.Count - j - 1];
                var priceAtStochZigZag = quotes.SingleOrDefault(it => it.Date == stochZigZagPoint.Date);
                if (quotes.Last().Low < priceAtStochZigZag.Low 
                    && (decimal)stochResults.Last().K > stochZigZagPoint.ZigZag
                    && (quotes.Last().Date - stochZigZagPoint.Date).TotalMinutes >= 9)
                {
                    return BiasType.Bullish;
                }
            }

            // Check for Bearish Divergence
            bool isBearishDivergence = false;
            for (int j = 1; j <= lookbackPeriod; j++)
            {
                var stochZigZagPoint = stochZigZagHighs[stochZigZagHighs.Count - j - 1];
                var priceAtStochZigZag = quotes.SingleOrDefault(it => it.Date == stochZigZagPoint.Date);
                if (quotes.Last().Close > priceAtStochZigZag.High
                    && (decimal)stochResults.Last().K < stochZigZagPoint.ZigZag
                    && (quotes.Last().Date - stochZigZagPoint.Date).TotalMinutes >= 9)
                {
                    return BiasType.Bearish;
                }
            }
            return null;
        }

        private static void BACKTEST16_Stochastic_Quad_Rotation(List<IBinanceKline> lineData)
        {
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var positionHistories = new List<positionHistory>();
            var lineSim = new List<IBinanceKline>();


            var buEntries = new List<futureEntry>();
            var beEntries = new List<futureEntry>();
            int tresh = 800;

            totalProfit = 3000;
            for (int d = 0; d <= lineData.Count - tresh; d++)
            {
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();

                if (true)
                {
                    candleStarted = true;
                    lineSim = lineData.Skip(d).Take(tresh).ToList();

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim); //.Take(390).ToList()

                    var stoch_9_3 = quotes.GetStoch(9, 3);
                    var stoch_14_3 = quotes.GetStoch(14, 3);
                    var stoch_40_4 = quotes.GetStoch(40, 3);
                    var stoch_60_10 = quotes.GetStoch(60, 10);

                    if (quotes.Last().Date.Hour == 16 && quotes.Last().Date.Minute == 14)
                    {
                        var gg = 0;
                    }

                    if (stoch_9_3.Last().K >= 80 && stoch_14_3.Last().K >= 80 && stoch_40_4.Last().K >= 80 && stoch_60_10.Last().K >= 80)
                    {
                        var result = DetectDivergence(quotes, stoch_9_3, 5);
                        if (result != null)
                        {
                            var gg = 0;
                        }
                    }

                    if (stoch_9_3.Last().K <= 20 && stoch_14_3.Last().K <= 20 && stoch_40_4.Last().K <= 20 && stoch_60_10.Last().K <= 20)
                    {
                        var result = DetectDivergence(quotes, stoch_9_3, 5);
                        if (result != null)
                        {
                            var gg = 0;
                        }
                    }
                }
            }
        }

        public class ProfitOverTime
        {
            public DateTime Date { get; set; }
            public decimal Value { get; set; }
        }
        public static List<ProfitOverTime> CreateProfitOverTimeList(Dictionary<KlineInterval, List<IBinanceKline>> timeFrameData)
        {
            // Find the earliest and most recent dates in the timeFrameData
            DateTime earliestDate = timeFrameData.Values.SelectMany(list => list).Min(kline => kline.OpenTime).Date;
            DateTime mostRecentDate = timeFrameData.Values.SelectMany(list => list).Max(kline => kline.OpenTime).Date;

            // Create the list to store the day and decimal value
            List<ProfitOverTime> profitOverTimeList = new List<ProfitOverTime>();

            // Iterate through each day in the interval and add an entry to the list
            for (DateTime date = earliestDate; date <= mostRecentDate; date = date.AddDays(1))
            {
                profitOverTimeList.Add(new ProfitOverTime { Date = date, Value = 0m }); // Initialize with a default decimal value
            }

            return profitOverTimeList;
        }    
        private static void BACKTEST15_FVG_ENHANCED2(Dictionary<KlineInterval, List<IBinanceKline>> timeFrameData, int v)
        {
            List<fvgFutureEntry> fvgFutureEntries = new List<fvgFutureEntry>();
            List<EntryModelTF> entryModels = new List<EntryModelTF>();
            //entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneMonth, LowKlineInterval = KlineInterval.OneDay });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneWeek, LowKlineInterval = KlineInterval.FourHour });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneDay, LowKlineInterval = KlineInterval.OneHour });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.FourHour, LowKlineInterval = KlineInterval.FifteenMinutes });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneHour, LowKlineInterval = KlineInterval.FiveMinutes });

            Dictionary<KlineInterval, List<IBinanceKline>> sections = new Dictionary<KlineInterval, List<IBinanceKline>>();
            Dictionary<KlineInterval, List<FairValueGap>> FVGS = new Dictionary<KlineInterval, List<FairValueGap>>();

            FVGS[KlineInterval.FiveMinutes] = new List<FairValueGap>();
            FVGS[KlineInterval.FifteenMinutes] = new List<FairValueGap>();
            FVGS[KlineInterval.OneHour] = new List<FairValueGap>();
            FVGS[KlineInterval.FourHour] = new List<FairValueGap>();
            FVGS[KlineInterval.OneDay] = new List<FairValueGap>();
            FVGS[KlineInterval.OneWeek] = new List<FairValueGap>();
            FVGS[KlineInterval.OneMonth] = new List<FairValueGap>();
            //var limitDate = DateTime.Now.AddDays(-daysBackStart);
            var usedTFs = new List<KlineInterval>()
                            {
                                KlineInterval.FiveMinutes,
                                KlineInterval.FifteenMinutes,
                                KlineInterval.OneHour,
                                KlineInterval.FourHour,
                                KlineInterval.OneDay,
                                KlineInterval.OneWeek,
                                KlineInterval.OneMonth
                            };

            //var profitOverTime = CreateProfitOverTimeDictionary(timeFrameData);

            foreach (var child in timeFrameData[KlineInterval.FiveMinutes])
            {
                if (child.OpenTime == new DateTime(2024, 7, 15, 0, 25, 0))
                {
                    var ttsd = 0;
                }
                sections[KlineInterval.FiveMinutes] = timeFrameData[KlineInterval.FiveMinutes].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.FifteenMinutes] = timeFrameData[KlineInterval.FifteenMinutes].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneHour] = timeFrameData[KlineInterval.OneHour].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.FourHour] = timeFrameData[KlineInterval.FourHour].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneDay] = timeFrameData[KlineInterval.OneDay].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneWeek] = timeFrameData[KlineInterval.OneWeek].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneMonth] = timeFrameData[KlineInterval.OneMonth].Where(it => it.CloseTime <= child.CloseTime).ToList();

                updateFVGList(FVGS[KlineInterval.FiveMinutes], KlineInterval.FiveMinutes, sections[KlineInterval.FiveMinutes].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.FifteenMinutes], KlineInterval.FifteenMinutes, sections[KlineInterval.FifteenMinutes].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneHour], KlineInterval.OneHour, sections[KlineInterval.OneHour].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.FourHour], KlineInterval.FourHour, sections[KlineInterval.FourHour].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneDay], KlineInterval.OneDay, sections[KlineInterval.OneDay].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneWeek], KlineInterval.OneWeek, sections[KlineInterval.OneWeek].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneMonth], KlineInterval.OneMonth, sections[KlineInterval.OneMonth].TakeLast(3), child);

                //var quotes15 = GetQuoteFromKlines(sections[KlineInterval.OneHour]);
                ///var ema50 = quotes15.GetEma(50);
                //var ema200 = quotes15.GetEma(200);

                //order maintenance
                if (fvgFutureEntries.Count > 0) //nu mai intra aici daca openPositions > 0
                {
                    fvgFutureEntries.RemoveAll(it => it.Invalidated == true);
                    //first invalidate any Open Orders that may have already reached target without becoming a Position
                    foreach (var order in fvgFutureEntries) //.Where(it => it.Invalidated == false)
                    {
                        var range = timeFrameData[KlineInterval.FiveMinutes].Where(it => it.OpenTime > order.InitialDate && it.OpenTime <= child.OpenTime);
                        var maxHigh = range.Max(it => it.HighPrice);
                        var minLow = range.Min(it => it.LowPrice);
                        if (IsOverlappingEquals(order.TakeProfit.Value, order.TakeProfit.Value, minLow, maxHigh))
                        {
                            order.Invalidated = true;
                        } 
                    }
                    //check if any valid Open Orders can become a position
                    if (OpenPositions.Count == 0)
                    {
                        if (!(child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday))
                        {
                            foreach (var order in fvgFutureEntries.Where(it => it.Invalidated == false))
                            {
                                if (IsOverlappingEquals(order.EntryPrice.Value, order.EntryPrice.Value, child.LowPrice, child.HighPrice))
                                {
                                    //if (!
                                    //    (
                                    //    (order.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                    //    ||
                                    //    (order.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                    //{
                                    //    order.Invalidated = false;
                                    //}
                                    OpenPositionIfPossible(order, child.OpenTime);

                                    fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                                    {
                                        it.Invalidated = true;
                                        return it;
                                    }).ToList();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            /*fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                            {
                                it.Invalidated = true;
                                return it;
                            }).ToList();*/
                        }
                    }
                }

                //position maintenance
                if (OpenPositions.Count > 0)
                {
                    var openPos = OpenPositions.First();
                    if (IsOverlappingEquals(openPos.Target, openPos.Target, child.LowPrice, child.HighPrice))
                    {
                        openPos.PNL = calculatePnLNoFees(openPos.EntryPrice, openPos.Target, openPos.Size, 0, openPos.TradeType == TradeType.Long ? 1 : -1).Value;
                        var fees = calculateFees(openPos.Target, openPos.Size, makerFee).Value;
                        openPos.Fees += fees;
                        openPos.Won = true;
                        MessageLog($"Won current position:\nTime: {child.OpenTime}\nPNL: {openPos.PNL}\nComission: {fees}", ConsoleColor.DarkGreen);
                        AccountValue += openPos.PNL;
                        AccountValue -= openPos.Fees;
                        MessageLog($"Current Account Value: {AccountValue}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        if (IsOverlappingEquals(openPos.StopLoss, openPos.StopLoss, child.LowPrice, child.HighPrice))
                        {
                            openPos.PNL = calculatePnLNoFees(openPos.EntryPrice, openPos.StopLoss, openPos.Size, 0, openPos.TradeType == TradeType.Long ? 1 : -1).Value;
                            var fees = calculateFees(openPos.Target, openPos.Size, takerFee).Value;
                            openPos.Fees += fees;
                            openPos.Won = false;
                            MessageLog($"Lost current position:\nTime: {child.OpenTime}\nPNL: {openPos.PNL}\nComission: {fees}", ConsoleColor.DarkRed);
                            AccountValue += openPos.PNL;
                            AccountValue -= openPos.Fees;
                            MessageLog($"Current Account Value: {AccountValue}", ConsoleColor.Cyan);
                        }
                    }

                    if (openPos.PNL != 0)
                    {
                        PastPositions.Add(openPos);
                        OpenPositions.Clear();
                        fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                        {
                            it.Invalidated = true;
                            return it;
                        }).ToList();
                    }
                }
                else
                {
                    // LONGS SECTION
                    // DO SOMEHOW TO MAKE HTF FVG SMALLER AFTER EACH ENTRY - AFTER EACH ENTRY, RESIZE THE FVG
                    foreach (var eM in entryModels)
                    {
                        var quotesLTF = GetQuoteFromKlines(sections[eM.LowKlineInterval]);
                        List<FairValueGap> HighSectionFVGs = FVGS[eM.HighKlineInterval];

                        if (!HighSectionFVGs.Any(it => it.BiasType == BiasType.Bullish && it.IsTapped)) continue; // check to see if any of the HTF FVGs of this entryModel are tapped, if not, skip

                        // MAYBE CHECK IF THERE IS A LTF OPPOSING FVG THAT ENTERED THE HTF FVG BEFORE BEING REJECTED

                        var LTFquote = sections[eM.LowKlineInterval].TakeLast(3).ToList();
                        var LTFFVG = LTFquote.Count >= 3 ? IsFairValueGap(LTFquote) : null;
                        if (LTFFVG == null || LTFFVG != BiasType.Bullish) continue;

                        var closestBullishHTF_FVG = HighSectionFVGs.First(it => it.BiasType == BiasType.Bullish && it.IsTapped && it.IsMitigated == false && it.IsForgotten == false && it.IsLiquidityTakenAfterTap == false);

                        if (closestBullishHTF_FVG != null) //a bullish HTF FVG exists in the vecinity
                        {
                            var htfFVGTop = closestBullishHTF_FVG.TopStructure;

                            closestBullishHTF_FVG.IsTapped = false; // reset the IsTapped property for the next entry

                            if (LTFquote[0].OpenTime < closestBullishHTF_FVG.FVGCloseTime) continue; // making sure the LTF FVG forms after the formation of the HTF FVG

                            // check if EMAs are in the right order
                            var ema50 = quotesLTF.GetEma(50);
                            var ema200 = quotesLTF.GetEma(200);
                            if (!(ema50.Last().Ema > ema200.Last().Ema)) continue;

                            var entrybool = true;
                            var LTFFVG_Type = GetFVGType(LTFquote, 1);
                            var entry = new fvgFutureEntry();
                            entry.Invalidated = false;
                            entry.HTF_FVG_CANDLE_TIME = closestBullishHTF_FVG.FVGOpenTime;
                            entry.StopLoss = Math.Min(LTFquote[0].LowPrice, LTFquote[1].LowPrice);//LowSectionFVGs[j].FvgAreaLow; //NO WICK NOW
                            if (LTFFVG_Type == FVGType.BreakAwayGapExpansion || LTFFVG_Type == FVGType.BreakAwayGapSneaky)
                            {
                                entry.PositionType = PositionType.Market;
                                entry.EntryPrice = LTFquote[2].ClosePrice;
                            }
                            else
                            {
                                entry.PositionType = PositionType.Limit;
                                entry.EntryPrice = LTFquote[2].LowPrice;
                            }
                            entry.TakeProfit = entry.EntryPrice + (entry.EntryPrice - entry.StopLoss) * rr;
                            entry.InitialDate = LTFquote[2].CloseTime;
                            entry.PositionSize = CalculatePositionSize(AccountValue, entry.EntryPrice, entry.StopLoss, RiskPercent);
                            entry.TradeType = TradeType.Long;
                            entry.EntryModelTF = eM;

                            if (child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday) entry.Invalidated = true;

                            if (entry.TakeProfit > closestBullishHTF_FVG.TopStructure) entry.Invalidated = true; //MAYBE CHANGE HERE IN ORDER TO HAVE a > ~1.8 RATIO if that means it beats the top

                            var largerTFs = usedTFs.Where(it => it > eM.HighKlineInterval).ToList();
                            foreach (var tf in largerTFs)
                            {
                                var opposingLargerTFFVG = FVGS[tf].FirstOrDefault(it => it.BiasType == BiasType.Bearish);
                                if (opposingLargerTFFVG != null && IsOverlappingEquals(entry.EntryPrice.Value, entry.TakeProfit.Value, opposingLargerTFFVG.FvgAreaLow, opposingLargerTFFVG.FvgAreaHigh))
                                {
                                    entry.Invalidated = true; break;
                                }
                            }

                            if (entry.Invalidated == false && entry.PositionType == PositionType.Market && OpenPositions.Count == 0)
                            {
                                bool opened = OpenPositionIfPossible(entry, LTFquote[2].CloseTime);
                            }
                            fvgFutureEntries.Add(entry);
                        }
                    }

                    // SHORTS SECTION
                    // DO SOMEHOW TO MAKE HTF FVG SMALLER AFTER EACH ENTRY - AFTER EACH ENTRY, RESIZE THE FVG
                    foreach (var eM in entryModels)
                    {
                        var quotesLTF = GetQuoteFromKlines(sections[eM.LowKlineInterval]);
                        List<FairValueGap> HighSectionFVGs = FVGS[eM.HighKlineInterval];

                        if (!HighSectionFVGs.Any(it => it.BiasType == BiasType.Bearish && it.IsTapped)) continue; // check to see if any of the HTF FVGs of this entryModel are tapped, if not, skip

                        // MAYBE CHECK IF THERE IS A LTF OPPOSING FVG THAT ENTERED THE HTF FVG BEFORE BEING REJECTED

                        var LTFquote = sections[eM.LowKlineInterval].TakeLast(3).ToList();
                        var LTFFVG = LTFquote.Count >= 3 ? IsFairValueGap(LTFquote) : null;
                        if (LTFFVG == null || LTFFVG != BiasType.Bearish) continue;

                        var closestBearishHTF_FVG = HighSectionFVGs.First(it => it.BiasType == BiasType.Bearish && it.IsTapped && it.IsMitigated == false && it.IsForgotten == false && it.IsLiquidityTakenAfterTap == false);

                        if (closestBearishHTF_FVG != null) //a bullish HTF FVG exists in the vecinity
                        {
                            var htfFVGTop = closestBearishHTF_FVG.BottomStructure;

                            closestBearishHTF_FVG.IsTapped = false; // reset the IsTapped property for the next entry

                            if (LTFquote[0].OpenTime < closestBearishHTF_FVG.FVGCloseTime) continue; // making sure the LTF FVG forms after the formation of the HTF FVG

                            // check if EMAs are in the right order
                            var ema50 = quotesLTF.GetEma(50);
                            var ema200 = quotesLTF.GetEma(200);
                            if (!(ema50.Last().Ema < ema200.Last().Ema)) continue;

                            var entrybool = true;
                            var LTFFVG_Type = GetFVGType(LTFquote, -1);
                            var entry = new fvgFutureEntry();
                            entry.Invalidated = false;
                            entry.HTF_FVG_CANDLE_TIME = closestBearishHTF_FVG.FVGOpenTime;
                            entry.StopLoss = Math.Max(LTFquote[0].HighPrice, LTFquote[1].HighPrice);//LowSectionFVGs[j].FvgAreaLow; //NO WICK NOW
                            if (LTFFVG_Type == FVGType.BreakAwayGapExpansion || LTFFVG_Type == FVGType.BreakAwayGapSneaky)
                            {
                                entry.PositionType = PositionType.Market;
                                entry.EntryPrice = LTFquote[2].ClosePrice;
                            }
                            else
                            {
                                entry.PositionType = PositionType.Limit;
                                entry.EntryPrice = LTFquote[2].HighPrice;
                            }
                            entry.TakeProfit = entry.EntryPrice + (entry.EntryPrice - entry.StopLoss) * rr;
                            entry.InitialDate = LTFquote[2].CloseTime;
                            entry.PositionSize = CalculatePositionSize(AccountValue, entry.EntryPrice, entry.StopLoss, RiskPercent);
                            entry.TradeType = TradeType.Short;
                            entry.EntryModelTF = eM;

                            if (child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday) entry.Invalidated = true;

                            if (entry.TakeProfit < closestBearishHTF_FVG.BottomStructure) entry.Invalidated = true; //MAYBE CHANGE HERE IN ORDER TO HAVE a > ~1.8 RATIO if that means it beats the top

                            var largerTFs = usedTFs.Where(it => it > eM.HighKlineInterval).ToList();
                            foreach (var tf in largerTFs)
                            {
                                var opposingLargerTFFVG = FVGS[tf].FirstOrDefault(it => it.BiasType == BiasType.Bullish);
                                if (opposingLargerTFFVG != null && IsOverlappingEquals(entry.EntryPrice.Value, entry.TakeProfit.Value, opposingLargerTFFVG.FvgAreaLow, opposingLargerTFFVG.FvgAreaHigh))
                                {
                                    entry.Invalidated = true; break;
                                }
                            }

                            if (entry.Invalidated == false && entry.PositionType == PositionType.Market && OpenPositions.Count == 0)
                            {
                                bool opened = OpenPositionIfPossible(entry, LTFquote[2].CloseTime);
                            }
                            fvgFutureEntries.Add(entry);
                        }
                    }
                }
            }
        }

        private static void BACKTEST13_FVG_GALORE(Dictionary<KlineInterval, List<IBinanceKline>> timeFrameData, int daysBackStart)
        {
            List<fvgFutureEntry> fvgFutureEntries = new List<fvgFutureEntry>();
            List<EntryModelTF> entryModels = new List<EntryModelTF>();
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneMonth, LowKlineInterval = KlineInterval.FourHour });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneWeek, LowKlineInterval = KlineInterval.OneHour });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneDay, LowKlineInterval = KlineInterval.FifteenMinutes });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.FourHour, LowKlineInterval = KlineInterval.FiveMinutes });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneHour, LowKlineInterval = KlineInterval.FiveMinutes });

            Dictionary<KlineInterval, List<IBinanceKline>> sections = new Dictionary<KlineInterval, List<IBinanceKline>>();
            Dictionary<KlineInterval, List<FairValueGap>> FVGS = new Dictionary<KlineInterval, List<FairValueGap>>();

            FVGS[KlineInterval.FiveMinutes] = new List<FairValueGap>();
            FVGS[KlineInterval.FifteenMinutes] = new List<FairValueGap>();
            FVGS[KlineInterval.OneHour] = new List<FairValueGap>();
            FVGS[KlineInterval.FourHour] = new List<FairValueGap>();
            FVGS[KlineInterval.OneDay] = new List<FairValueGap>();
            FVGS[KlineInterval.OneWeek] = new List<FairValueGap>();
            FVGS[KlineInterval.OneMonth] = new List<FairValueGap>();
            //var limitDate = DateTime.Now.AddDays(-daysBackStart);

            foreach (var child in timeFrameData[KlineInterval.FiveMinutes])            
            {
                if (child.OpenTime == new DateTime(2024, 1, 2, 11, 40, 0))
                {
                    var ttsd = 0;
                }
                sections[KlineInterval.FiveMinutes] = timeFrameData[KlineInterval.FiveMinutes].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.FifteenMinutes] = timeFrameData[KlineInterval.FifteenMinutes].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneHour] = timeFrameData[KlineInterval.OneHour].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.FourHour] = timeFrameData[KlineInterval.FourHour].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneDay] = timeFrameData[KlineInterval.OneDay].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneWeek] = timeFrameData[KlineInterval.OneWeek].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneMonth] = timeFrameData[KlineInterval.OneMonth].Where(it => it.CloseTime <= child.CloseTime).ToList();

                updateFVGList(FVGS[KlineInterval.FiveMinutes], KlineInterval.FiveMinutes, sections[KlineInterval.FiveMinutes].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.FifteenMinutes], KlineInterval.FifteenMinutes, sections[KlineInterval.FifteenMinutes].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneHour], KlineInterval.OneHour, sections[KlineInterval.OneHour].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.FourHour], KlineInterval.FourHour, sections[KlineInterval.FourHour].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneDay], KlineInterval.OneDay, sections[KlineInterval.OneDay].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneWeek], KlineInterval.OneWeek, sections[KlineInterval.OneWeek].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneMonth], KlineInterval.OneMonth, sections[KlineInterval.OneMonth].TakeLast(3), child);

                var quotes15 = GetQuoteFromKlines(sections[KlineInterval.OneHour]);
                var ema50 = quotes15.GetEma(50);
                var ema200 = quotes15.GetEma(200);

                //order maintenance
                if (fvgFutureEntries.Count > 0) //nu mai intra aici daca openPositions > 0
                {
                    fvgFutureEntries.RemoveAll(it => it.Invalidated == true);
                    //first invalidate any Open Orders that may have already reached target without becoming a Position
                    foreach (var order in fvgFutureEntries) //.Where(it => it.Invalidated == false)
                    {
                        var range = timeFrameData[KlineInterval.FiveMinutes].Where(it => it.OpenTime > order.InitialDate && it.OpenTime <= child.OpenTime);
                        var maxHigh = range.Max(it => it.HighPrice);
                        var minLow = range.Min(it => it.LowPrice);
                        if (IsOverlappingEquals(order.TakeProfit.Value, order.TakeProfit.Value, minLow, maxHigh))
                        {
                            order.Invalidated = true;
                        }
                    }
                    //check if any valid Open Orders can become a position
                    if (OpenPositions.Count == 0)
                    {
                        if (!(child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday))
                        {
                            foreach (var order in fvgFutureEntries.Where(it => it.Invalidated == false))
                            {
                                if (IsOverlappingEquals(order.EntryPrice.Value, order.EntryPrice.Value, child.LowPrice, child.HighPrice))
                                {
                                    //if (!
                                    //    (
                                    //    (order.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                    //    ||
                                    //    (order.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                    //{
                                    //    order.Invalidated = false;
                                    //}
                                    OpenPositionIfPossible(order, child.OpenTime);

                                    fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                                    {
                                        it.Invalidated = true;
                                        return it;
                                    }).ToList();
                                    break;
                                }
                            }
                        } else
                        {
                            fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                            {
                                it.Invalidated = true;
                                return it;
                            }).ToList();
                        }
                    }
                }

                //position maintenance
                if (OpenPositions.Count > 0)
                {
                    var openPos = OpenPositions.First();
                    if (openPos.EntryPrice == 43512.10m)
                    {
                        var tty = 0;
                    }
                    if (IsOverlappingEquals(openPos.Target, openPos.Target, child.LowPrice, child.HighPrice))
                    {
                        openPos.PNL = calculatePnLNoFees(openPos.EntryPrice, openPos.Target, openPos.Size, 0, openPos.TradeType == TradeType.Long ? 1 : -1).Value;
                        var fees = calculateFees(openPos.Target, openPos.Size, makerFee).Value;
                        openPos.Fees += fees;
                        MessageLog($"Won current position:\nTime: {child.OpenTime}\nPNL: {openPos.PNL}\nComission: {fees}", ConsoleColor.DarkGreen);
                        AccountValue += openPos.PNL;
                        AccountValue -= openPos.Fees;
                        MessageLog($"Current Account Value: {AccountValue}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        if (IsOverlappingEquals(openPos.StopLoss, openPos.StopLoss, child.LowPrice, child.HighPrice))
                        {
                            openPos.PNL = calculatePnLNoFees(openPos.EntryPrice, openPos.StopLoss, openPos.Size, 0, openPos.TradeType == TradeType.Long ? 1 : -1).Value;
                            var fees = calculateFees(openPos.Target, openPos.Size, takerFee).Value;
                            openPos.Fees += fees;
                            MessageLog($"Lost current position:\nTime: {child.OpenTime}\nPNL: {openPos.PNL}\nComission: {fees}", ConsoleColor.DarkRed);
                            AccountValue += openPos.PNL;
                            AccountValue -= openPos.Fees;
                            MessageLog($"Current Account Value: {AccountValue}", ConsoleColor.Cyan);
                        }
                    }

                    if (openPos.PNL != 0)
                    {
                        OpenPositions.Clear();
                        fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                        {
                            it.Invalidated = true;
                            return it;
                        }).ToList();
                    }
                }
                else
                {
                    // LONGS SECTION
                    foreach (var eM in entryModels)
                    {
                        //List<FairValueGap> HighSectionFVGs = FindFVGs(sections[eM.HighKlineInterval], eM.HighKlineInterval);
                        List<FairValueGap> HighSectionFVGs = FVGS[eM.HighKlineInterval];

                        // ADAUGA IMPREUNA INTR-O LISTA, TOATE FVG-uRILE DE PE TIMEFRAMES MAI MARI SAU EGALE CU CURENTUL eM
                        // SAU MAI SIMPLU, ADAUGA TOATE FVGs mai mari sau egale cu 4H

                        var closestBullishHTF_FVG = HighSectionFVGs.Where(it => !it.IsMitigated && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice)
                                                                             && it.BiasType == BiasType.Bullish)
                                                               .OrderByDescending(it => it.FvgAreaHigh).FirstOrDefault();

                        var closestBearishHTF_FVG = HighSectionFVGs.Where(it => !it.IsMitigated //&& !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice)
                                                                             && it.BiasType == BiasType.Bearish)
                                                               .OrderBy(it => it.FvgAreaLow).FirstOrDefault(); //schimba aici sa fie si FVG-uri in care esti deja ?? cred ca le iau pe toate, nu doar cele overlapping si gata

                        if (closestBullishHTF_FVG != null) //a bullish FVG exists in the vecinity
                        {
                            var firstTap = sections[eM.LowKlineInterval].Where(it => it.OpenTime > closestBullishHTF_FVG.Structure[2].CloseTime)
                                                  .FirstOrDefault(it => it.LowPrice < closestBullishHTF_FVG.FvgAreaHigh);

                            if (firstTap != null)
                            {
                                List<FairValueGap> LowSectionFVGs = FVGS[eM.LowKlineInterval].Where(it => it.FVGOpenTime > firstTap.OpenTime).ToList();
                                LowSectionFVGs = LowSectionFVGs.Where(it => it.IsMitigated == false).ToList();

                                if (LowSectionFVGs.Where(it => it.BiasType == BiasType.Bullish).Count() >= 2)
                                {
                                    // trebuie sa iei ultimul FVG bullish non mitigated DAR tapped pentru primul FVG din LTF entry model
                                    var lastGoodFirstFVG = LowSectionFVGs.LastOrDefault(it => it.IsMitigated == false
                                                                                    && it.IsTapped == true
                                                                                    && it.BiasType == BiasType.Bullish);
                                    var i = LowSectionFVGs.IndexOf(lastGoodFirstFVG);
                                    if (lastGoodFirstFVG != null)
                                    {
                                        for (int j = i + 1; j < LowSectionFVGs.Count; j++)
                                        {
                                            if (LowSectionFVGs[j].BiasType == BiasType.Bullish) // maybe check if all bearish ones between these two have been mitigated
                                            {
                                                var insideFVGs = LowSectionFVGs.Where(it => it.FVGCloseTime > LowSectionFVGs[i].Structure[2].CloseTime //verific asa ca sa ia bine ce e intre
                                                                                            && it.FVGCloseTime < LowSectionFVGs[j].Structure[2].CloseTime).ToList();

                                                //if (true || !(insideFVGs.Count(it => it.BiasType == BiasType.Bearish && it.IsMitigated == false) > 0))
                                                if (insideFVGs.Count(it => it.BiasType == BiasType.Bullish && it.IsMitigated == false) == 0)
                                                //se verifica sa nu fie bearish FVGs nemitigate intre cele doua bullish
                                                {
                                                    var found = true;
                                                    var baseFVG = LowSectionFVGs.FirstOrDefault(it => it.BiasType == BiasType.Bullish && it.IsTapped && !it.IsMitigated);
                                                    if (child.CloseTime == LowSectionFVGs[j].FVGCloseTime
                                                        && baseFVG != null
                                                        && LowSectionFVGs[i].FVGOpenTime == baseFVG.FVGOpenTime
                                                        && !insideFVGs.Exists(it => it.BiasType == BiasType.Bullish && it.IsMitigated == false)) //first unmitigated but tapped Bullish FVG
                                                    {
                                                        if (eM.HighKlineInterval == KlineInterval.OneDay)
                                                        {
                                                            var letsgo = true;
                                                        }
                                                        var entrybool = true;
                                                        var entry = new fvgFutureEntry();
                                                        entry.Invalidated = false;
                                                        entry.HTF_FVG_CANDLE_TIME = closestBullishHTF_FVG.FVGOpenTime;
                                                        entry.StopLoss = LowSectionFVGs[j].Structure[0].LowPrice;//LowSectionFVGs[j].FvgAreaLow; //NO WICK NOW
                                                        if ((LowSectionFVGs[j].FVGType == FVGType.BreakAwayGapExpansion || LowSectionFVGs[j].FVGType == FVGType.BreakAwayGapSneaky))
                                                        {
                                                            entry.PositionType = PositionType.Market;
                                                            entry.EntryPrice = LowSectionFVGs[j].Structure[2].ClosePrice;
                                                        }
                                                        else
                                                        {
                                                            entry.PositionType = PositionType.Limit;
                                                            entry.EntryPrice = LowSectionFVGs[j].Structure[2].LowPrice;
                                                        }
                                                        entry.TakeProfit = entry.EntryPrice + (entry.EntryPrice - entry.StopLoss) * rr;
                                                        entry.InitialDate = LowSectionFVGs[j].Structure[2].CloseTime;
                                                        entry.PositionSize = CalculatePositionSize(AccountValue, entry.EntryPrice, entry.StopLoss, RiskPercent);
                                                        entry.TradeType = TradeType.Long;
                                                        entry.EntryModelTF = eM;

                                                        //var hasBreathingRoom = HighSectionFVGs.FirstOrDefault(it => it.BiasType == BiasType.Bearish && it.IsMitigated == false && entry.TakeProfit > it.FvgAreaLow);
                                                        if (closestBearishHTF_FVG == null)
                                                        {
                                                            entry.TakeProfit = entry.EntryPrice + (entry.EntryPrice - entry.StopLoss) * rr;
                                                        }
                                                        else
                                                        {
                                                            if (closestBearishHTF_FVG.FvgAreaLow < entry.TakeProfit) //checking to see if trade doesn't have a same-magnitude opposing HTF FVG before TP
                                                            {
                                                                entry.Invalidated = true;
                                                            }
                                                            else
                                                            {
                                                                entry.TakeProfit = entry.EntryPrice + (entry.EntryPrice - entry.StopLoss) * rr;
                                                                if (closestBearishHTF_FVG.FvgAreaLow < entry.TakeProfit) //checking to see if trade doesn't have a same-magnitude opposing HTF FVG before TP
                                                                {
                                                                    entry.TakeProfit = closestBearishHTF_FVG.FvgAreaLow - 1;
                                                                }
                                                            }
                                                        }

                                                        if (child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday)
                                                        {
                                                            entry.Invalidated = true;
                                                        }

                                                        if (!isFullFVG(LowSectionFVGs[j].Structure, BiasType.Bullish))
                                                        {
                                                            entry.Invalidated = true;
                                                        }

                                                        if (entry.Invalidated == false && entry.PositionType == PositionType.Market && OpenPositions.Count == 0)
                                                        {
                                                            //if (!
                                                            //(
                                                            //    (entry.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                                            //||
                                                            //    (entry.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                                            //{
                                                            //    entry.Invalidated = false;
                                                            //}
                                                            bool opened = OpenPositionIfPossible(entry, LowSectionFVGs[j].FVGCloseTime);
                                                        }
                                                        fvgFutureEntries.Add(entry);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //var closestBearishHTF_FVG = HighSection.Where(it => !it.IsMitigated && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice))
                        //                                       .OrderBy(it => it.FvgAreaLow).FirstOrDefault();
                    }

                    // SHORTS SECTION
                    foreach (var eM in entryModels)
                    {
                        List<FairValueGap> HighSectionFVGs = FVGS[eM.HighKlineInterval];

                        var closestBullishHTF_FVG = HighSectionFVGs.Where(it => !it.IsMitigated //&& !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice)
                                                                             && it.BiasType == BiasType.Bullish)
                                                               .OrderByDescending(it => it.FvgAreaHigh).FirstOrDefault();


                        var closestBearishHTF_FVG = HighSectionFVGs.Where(it => !it.IsMitigated && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice)
                                                                             && it.BiasType == BiasType.Bearish)
                                                               .OrderBy(it => it.FvgAreaLow).FirstOrDefault();

                        if (closestBearishHTF_FVG != null) //a bullish FVG exists in the vecinity
                        {
                            var firstTap = sections[eM.LowKlineInterval].Where(it => it.OpenTime > closestBearishHTF_FVG.Structure[2].CloseTime)
                                                                              .FirstOrDefault(it => it.HighPrice > closestBearishHTF_FVG.FvgAreaLow);

                            if (firstTap != null)
                            {
                                List<FairValueGap> LowSectionFVGs = FVGS[eM.LowKlineInterval].Where(it => it.FVGOpenTime > firstTap.OpenTime).ToList();
                                LowSectionFVGs = LowSectionFVGs.Where(it => it.IsMitigated == false).ToList();

                                if (LowSectionFVGs.Where(it => it.BiasType == BiasType.Bearish).Count() >= 2)
                                {
                                    // trebuie sa iei ultimul FVG bullish non mitigated DAR tapped pentru primul FVG din LTF entry model
                                    var lastGoodFirstFVG = LowSectionFVGs.LastOrDefault(it => it.IsMitigated == false
                                                                                    && it.IsTapped == true
                                                                                    && it.BiasType == BiasType.Bearish);
                                    var i = LowSectionFVGs.IndexOf(lastGoodFirstFVG);                                    

                                    if (lastGoodFirstFVG != null)
                                    {
                                        for (int j = i + 1; j < LowSectionFVGs.Count; j++)
                                        {
                                            if (LowSectionFVGs[j].BiasType == BiasType.Bearish) // maybe check if all bearish ones between these two have been mitigated
                                            {
                                                var insideFVGs = LowSectionFVGs.Where(it => it.FVGCloseTime > LowSectionFVGs[i].Structure[2].CloseTime //verific asa ca sa ia bine ce e intre
                                                                                            && it.FVGCloseTime < LowSectionFVGs[j].Structure[2].CloseTime).ToList();

                                                if (insideFVGs.Count(it => it.BiasType == BiasType.Bearish && it.IsMitigated == false) == 0)
                                                //se verifica sa nu fie bullish FVGs nemitigate intre cele doua bullish
                                                {
                                                    var found = true;
                                                    var baseFVG = LowSectionFVGs.FirstOrDefault(it => it.BiasType == BiasType.Bearish && it.IsTapped && !it.IsMitigated);
                                                    if (child.CloseTime == LowSectionFVGs[j].FVGCloseTime
                                                        && baseFVG != null
                                                        && LowSectionFVGs[i].FVGOpenTime == baseFVG.FVGOpenTime
                                                        && !insideFVGs.Exists(it => it.BiasType == BiasType.Bearish && it.IsMitigated == false)) //first unmitigated but tapped Bullish FVG
                                                    {
                                                        var entrybool = true;
                                                        var entry = new fvgFutureEntry();
                                                        entry.Invalidated = false;
                                                        entry.HTF_FVG_CANDLE_TIME = closestBearishHTF_FVG.FVGOpenTime;
                                                        entry.StopLoss = LowSectionFVGs[j].Structure[0].HighPrice;//LowSectionFVGs[j].FvgAreaHigh; // NO WICK NOW
                                                        if ((LowSectionFVGs[j].FVGType == FVGType.BreakAwayGapExpansion || LowSectionFVGs[j].FVGType == FVGType.BreakAwayGapSneaky))
                                                        {
                                                            entry.PositionType = PositionType.Market;
                                                            entry.EntryPrice = LowSectionFVGs[j].Structure[2].ClosePrice;
                                                        }
                                                        else
                                                        {
                                                            entry.PositionType = PositionType.Limit;
                                                            entry.EntryPrice = LowSectionFVGs[j].Structure[2].HighPrice;
                                                        }

                                                        entry.TakeProfit = entry.EntryPrice - (entry.StopLoss - entry.EntryPrice) * rr;
                                                        entry.InitialDate = LowSectionFVGs[j].Structure[2].CloseTime;
                                                        entry.PositionSize = CalculatePositionSize(AccountValue, entry.EntryPrice, entry.StopLoss, RiskPercent);
                                                        entry.TradeType = TradeType.Short;
                                                        entry.EntryModelTF = eM;

                                                        if (closestBullishHTF_FVG == null)
                                                        {
                                                            entry.TakeProfit = entry.EntryPrice - (entry.StopLoss - entry.EntryPrice) * rr;
                                                        }
                                                        else
                                                        {
                                                            if (closestBullishHTF_FVG.FvgAreaHigh > entry.TakeProfit) //checking to see if trade doesn't have a same-magnitude opposing HTF FVG before TP
                                                            {
                                                                entry.Invalidated = true;
                                                            }
                                                            else
                                                            {
                                                                entry.TakeProfit = entry.EntryPrice - (entry.StopLoss - entry.EntryPrice) * rr;
                                                                if (closestBullishHTF_FVG.FvgAreaHigh > entry.TakeProfit) //checking to see if trade doesn't have a same-magnitude opposing HTF FVG before TP
                                                                {
                                                                    entry.TakeProfit = closestBullishHTF_FVG.FvgAreaHigh + 1;
                                                                }
                                                            }
                                                        }
                                                        if (child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday)
                                                        {
                                                            entry.Invalidated = true;
                                                        }

                                                        if (!isFullFVG(LowSectionFVGs[j].Structure, BiasType.Bearish))
                                                        {
                                                            entry.Invalidated = true;
                                                        }

                                                        if (entry.Invalidated == false && entry.PositionType == PositionType.Market && OpenPositions.Count == 0)
                                                        {
                                                            //if (!
                                                            //(
                                                            //    (entry.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                                            //||
                                                            //    (entry.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                                            //{
                                                            //    entry.Invalidated = false;
                                                            //}
                                                            bool opened = OpenPositionIfPossible(entry, LowSectionFVGs[j].FVGCloseTime);
                                                        }
                                                        fvgFutureEntries.Add(entry);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //var closestBearishHTF_FVG = HighSection.Where(it => !it.IsMitigated && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice))
                        //                                       .OrderBy(it => it.FvgAreaLow).FirstOrDefault();
                    }
                }
            } 
        }

        private static void BACKTEST14_FVG_ENHANCED(Dictionary<KlineInterval, List<IBinanceKline>> timeFrameData, int daysBackStart)
        {
            List<fvgFutureEntry> fvgFutureEntries = new List<fvgFutureEntry>();
            List<EntryModelTF> entryModels = new List<EntryModelTF>();
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneMonth, LowKlineInterval = KlineInterval.OneDay });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneWeek, LowKlineInterval = KlineInterval.FourHour });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneDay, LowKlineInterval = KlineInterval.OneHour });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.FourHour, LowKlineInterval = KlineInterval.FifteenMinutes });
            entryModels.Add(new EntryModelTF { HighKlineInterval = KlineInterval.OneHour, LowKlineInterval = KlineInterval.FiveMinutes });

            Dictionary<KlineInterval, List<IBinanceKline>> sections = new Dictionary<KlineInterval, List<IBinanceKline>>();
            Dictionary<KlineInterval, List<FairValueGap>> FVGS = new Dictionary<KlineInterval, List<FairValueGap>>();

            FVGS[KlineInterval.FiveMinutes] = new List<FairValueGap>();
            FVGS[KlineInterval.FifteenMinutes] = new List<FairValueGap>();
            FVGS[KlineInterval.OneHour] = new List<FairValueGap>();
            FVGS[KlineInterval.FourHour] = new List<FairValueGap>();
            FVGS[KlineInterval.OneDay] = new List<FairValueGap>();
            FVGS[KlineInterval.OneWeek] = new List<FairValueGap>();
            FVGS[KlineInterval.OneMonth] = new List<FairValueGap>();
            //var limitDate = DateTime.Now.AddDays(-daysBackStart);

            foreach (var child in timeFrameData[KlineInterval.FiveMinutes])
            {
                if (child.OpenTime == new DateTime(2024, 1, 14, 9, 55, 0))
                {
                    var ttsd = 0;
                }
                sections[KlineInterval.FiveMinutes] = timeFrameData[KlineInterval.FiveMinutes].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.FifteenMinutes] = timeFrameData[KlineInterval.FifteenMinutes].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneHour] = timeFrameData[KlineInterval.OneHour].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.FourHour] = timeFrameData[KlineInterval.FourHour].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneDay] = timeFrameData[KlineInterval.OneDay].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneWeek] = timeFrameData[KlineInterval.OneWeek].Where(it => it.CloseTime <= child.CloseTime).ToList();
                sections[KlineInterval.OneMonth] = timeFrameData[KlineInterval.OneMonth].Where(it => it.CloseTime <= child.CloseTime).ToList();

                updateFVGList(FVGS[KlineInterval.FiveMinutes], KlineInterval.FiveMinutes, sections[KlineInterval.FiveMinutes].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.FifteenMinutes], KlineInterval.FifteenMinutes, sections[KlineInterval.FifteenMinutes].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneHour], KlineInterval.OneHour, sections[KlineInterval.OneHour].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.FourHour], KlineInterval.FourHour, sections[KlineInterval.FourHour].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneDay], KlineInterval.OneDay, sections[KlineInterval.OneDay].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneWeek], KlineInterval.OneWeek, sections[KlineInterval.OneWeek].TakeLast(3), child);
                updateFVGList(FVGS[KlineInterval.OneMonth], KlineInterval.OneMonth, sections[KlineInterval.OneMonth].TakeLast(3), child);

                //var quotes15 = GetQuoteFromKlines(sections[KlineInterval.OneHour]);
                ///var ema50 = quotes15.GetEma(50);
                //var ema200 = quotes15.GetEma(200);

                //order maintenance
                if (fvgFutureEntries.Count > 0) //nu mai intra aici daca openPositions > 0
                {
                    fvgFutureEntries.RemoveAll(it => it.Invalidated == true);
                    //first invalidate any Open Orders that may have already reached target without becoming a Position
                    foreach (var order in fvgFutureEntries) //.Where(it => it.Invalidated == false)
                    {
                        var range = timeFrameData[KlineInterval.FiveMinutes].Where(it => it.OpenTime > order.InitialDate && it.OpenTime <= child.OpenTime);
                        var maxHigh = range.Max(it => it.HighPrice);
                        var minLow = range.Min(it => it.LowPrice);
                        if (IsOverlappingEquals(order.TakeProfit.Value, order.TakeProfit.Value, minLow, maxHigh))
                        {
                            order.Invalidated = true;
                        }
                    }
                    //check if any valid Open Orders can become a position
                    if (OpenPositions.Count == 0)
                    {
                        if (!(child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday))
                        {
                            foreach (var order in fvgFutureEntries.Where(it => it.Invalidated == false))
                            {
                                if (IsOverlappingEquals(order.EntryPrice.Value, order.EntryPrice.Value, child.LowPrice, child.HighPrice))
                                {
                                    //if (!
                                    //    (
                                    //    (order.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                    //    ||
                                    //    (order.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                    //{
                                    //    order.Invalidated = false;
                                    //}
                                    OpenPositionIfPossible(order, child.OpenTime);

                                    fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                                    {
                                        it.Invalidated = true;
                                        return it;
                                    }).ToList();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                            {
                                it.Invalidated = true;
                                return it;
                            }).ToList();
                        }
                    }
                }

                //position maintenance
                if (OpenPositions.Count > 0)
                {
                    var openPos = OpenPositions.First();
                    if (openPos.EntryPrice == 43512.10m)
                    {
                        var tty = 0;
                    }
                    if (IsOverlappingEquals(openPos.Target, openPos.Target, child.LowPrice, child.HighPrice))
                    {
                        openPos.PNL = calculatePnLNoFees(openPos.EntryPrice, openPos.Target, openPos.Size, 0, openPos.TradeType == TradeType.Long ? 1 : -1).Value;
                        var fees = calculateFees(openPos.Target, openPos.Size, makerFee).Value;
                        openPos.Fees += fees;
                        MessageLog($"Won current position:\nTime: {child.OpenTime}\nPNL: {openPos.PNL}\nComission: {fees}", ConsoleColor.DarkGreen);
                        AccountValue += openPos.PNL;
                        AccountValue -= openPos.Fees;
                        MessageLog($"Current Account Value: {AccountValue}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        if (IsOverlappingEquals(openPos.StopLoss, openPos.StopLoss, child.LowPrice, child.HighPrice))
                        {
                            openPos.PNL = calculatePnLNoFees(openPos.EntryPrice, openPos.StopLoss, openPos.Size, 0, openPos.TradeType == TradeType.Long ? 1 : -1).Value;
                            var fees = calculateFees(openPos.Target, openPos.Size, takerFee).Value;
                            openPos.Fees += fees;
                            MessageLog($"Lost current position:\nTime: {child.OpenTime}\nPNL: {openPos.PNL}\nComission: {fees}", ConsoleColor.DarkRed);
                            AccountValue += openPos.PNL;
                            AccountValue -= openPos.Fees;
                            MessageLog($"Current Account Value: {AccountValue}", ConsoleColor.Cyan);
                        }
                    }

                    if (openPos.PNL != 0)
                    {
                        OpenPositions.Clear();
                        fvgFutureEntries = fvgFutureEntries.Select(it => //set all Invalidated properties to true because we selected this entry
                        {
                            it.Invalidated = true;
                            return it;
                        }).ToList();
                    }
                }
                else
                {
                    // LONGS SECTION
                    foreach (var eM in entryModels)
                    {
                        var quotesLTF = GetQuoteFromKlines(sections[eM.LowKlineInterval]);
                        var ema50 = quotesLTF.GetEma(50);
                        var ema200 = quotesLTF.GetEma(200);

                        if (ema50.Count() == 0 || ema200.Count() == 0 || ema50.Last().Ema < ema200.Last().Ema) continue;

                        List<FairValueGap> HighSectionFVGs = FVGS[eM.HighKlineInterval];

                        // * FVG-ul HTF bullish trebuie sa fie in ultimul leg Low to High
                        // * FVG-ul LTF bullish trebuie sa fie dupa ultimul leg Low to High
                        // * High-ul ultimului FVG HTF nu trebuie sa fie batut in leg-ul Low to High (pentru ca altfel i s-a luat deja liqudity-ul)

                        var LTFquote = sections[eM.LowKlineInterval].TakeLast(3).ToList();
                        var LTFFVG = LTFquote.Count >= 3 ? IsFairValueGap(LTFquote) : null;
                        if (LTFFVG == null || LTFFVG != BiasType.Bullish) continue;

                        //FindFVGs(section.ToList(), timeframe);
                        /*
                        var pivots = zigZagsHTF.Where(it => it.HighPoint != null || it.LowPoint != null);
                        if (pivots.Count() >= 2)
                        {

                        }
                        var lastPivots = pivots.Count() >= 2 ? pivots.TakeLast(2).ToList() : null;
                        if (lastPivots != null)
                        {
                            var ttt = 0;
                            // daca ultimul pivot nu a fost HighPoint, atunci nu a avut timp sa se formeze pentru ca s-ar fi format in urmatoarea candela de HTF dar nu a avut timp
                            // asa ca voi adauga High-ul maxim din range-ul de la Low pivot pana in momentul asta ca un High Pivot
                        }*/

                        var closestBullishHTF_FVG = HighSectionFVGs.Where(it => !it.IsMitigated// && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice) // FVG out of which it already got out
                                                                                                     && it.BiasType == BiasType.Bullish)
                                                                                       .OrderByDescending(it => it.FvgAreaHigh).FirstOrDefault();
                        

                        if (closestBullishHTF_FVG != null) //a bullish FVG exists in the vecinity
                        {
                            var htfFVGTop = closestBullishHTF_FVG.Structure.Max(it => it.HighPrice);
                            if (LTFquote[0].OpenTime < closestBullishHTF_FVG.FVGCloseTime) continue; // making sure the LTF FVG forms after the formation of the HTF FVG
                            var wasBullishHTFFVGLiquidated = sections[KlineInterval.FiveMinutes].Where(it => it.OpenTime >= closestBullishHTF_FVG.FVGOpenTime).Max(it => it.HighPrice) > htfFVGTop;
                            if (wasBullishHTFFVGLiquidated) continue;

                            var firstTap = sections[eM.LowKlineInterval].Where(it => it.OpenTime > closestBullishHTF_FVG.Structure[2].CloseTime)
                                                  .FirstOrDefault(it => it.LowPrice < closestBullishHTF_FVG.FvgAreaHigh);

                            if (firstTap != null)
                            {
                                //CHECK TO SEE IF BIG FVG WENT INTO A HIGHER OPPOSING FVG OR NOT
                                var maxFVG = Math.Max(closestBullishHTF_FVG.Structure[1].HighPrice, closestBullishHTF_FVG.Structure[2].HighPrice);


                                var firstUnmitigatedLTFFVG = FVGS[eM.LowKlineInterval].Where(it => it.FVGOpenTime >= firstTap.OpenTime && it.BiasType == BiasType.Bullish && !it.IsMitigated).FirstOrDefault();
                                if (firstUnmitigatedLTFFVG == null || firstUnmitigatedLTFFVG.FVGOpenTime != LTFquote[0].OpenTime) continue;

                                var indexofLTFFVG = sections[eM.LowKlineInterval].IndexOf(firstUnmitigatedLTFFVG.Structure[1]);
                                var barsBack = sections[eM.LowKlineInterval].Count - indexofLTFFVG;

                                if (barsBack < 50)
                                {

                                    var entrybool = true;
                                    var LTFFVG_Type = GetFVGType(LTFquote, 1);
                                    var entry = new fvgFutureEntry();
                                    entry.Invalidated = false;
                                    entry.HTF_FVG_CANDLE_TIME = closestBullishHTF_FVG.FVGOpenTime;
                                    entry.StopLoss = LTFquote[0].LowPrice;//LowSectionFVGs[j].FvgAreaLow; //NO WICK NOW
                                    if (LTFFVG_Type == FVGType.BreakAwayGapExpansion || LTFFVG_Type == FVGType.BreakAwayGapSneaky)
                                    {
                                        entry.PositionType = PositionType.Market;
                                        entry.EntryPrice = LTFquote[2].ClosePrice;
                                    }
                                    else
                                    {
                                        entry.PositionType = PositionType.Limit;
                                        entry.EntryPrice = LTFquote[2].LowPrice;
                                    }
                                    entry.TakeProfit = entry.EntryPrice + (entry.EntryPrice - entry.StopLoss) * rr;
                                    entry.InitialDate = LTFquote[2].CloseTime;
                                    entry.PositionSize = CalculatePositionSize(AccountValue, entry.EntryPrice, entry.StopLoss, RiskPercent);
                                    entry.TradeType = TradeType.Long;
                                    entry.EntryModelTF = eM;

                                    if (child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        entry.Invalidated = true;
                                    }

                                    /*
                                    if (entry.TakeProfit > htfFVGTop) //check to see if we're not going over the HTF top
                                    {
                                        entry.Invalidated = true;
                                    }*/

                                    /*
                                    if (!isFullFVG(LowSectionFVGs[j].Structure, BiasType.Bullish)) //checking if the current entry FVG is all same bias candles
                                    {
                                        entry.Invalidated = true;
                                    }*/

                                    if (entry.Invalidated == false && entry.PositionType == PositionType.Market && OpenPositions.Count == 0)
                                    {
                                        //if (!
                                        //(
                                        //    (entry.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                        //||
                                        //    (entry.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                        //{
                                        //    entry.Invalidated = false;
                                        //}
                                        bool opened = OpenPositionIfPossible(entry, LTFquote[2].CloseTime);
                                    }
                                    fvgFutureEntries.Add(entry);
                                }
                            }
                        }

                        //var closestBearishHTF_FVG = HighSection.Where(it => !it.IsMitigated && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice))
                        //                                       .OrderBy(it => it.FvgAreaLow).FirstOrDefault();
                    }

                    // SHORTS SECTION
                    foreach (var eM in entryModels)
                    {
                        if (true) break;
                        var quotesLTF = GetQuoteFromKlines(sections[eM.LowKlineInterval]);
                        var ema50 = quotesLTF.GetEma(50);
                        var ema200 = quotesLTF.GetEma(200);

                        if (ema50.Count() == 0 || ema200.Count() == 0 || ema50.Last().Ema > ema200.Last().Ema) continue;
                        List<FairValueGap> HighSectionFVGs = FVGS[eM.HighKlineInterval];

                        // * FVG-ul HTF bullish trebuie sa fie in ultimul leg Low to High
                        // * FVG-ul LTF bullish trebuie sa fie dupa ultimul leg Low to High
                        // * High-ul ultimului FVG HTF nu trebuie sa fie batut in leg-ul Low to High (pentru ca altfel i s-a luat deja liqudity-ul)

                        var LTFquote = sections[eM.LowKlineInterval].TakeLast(3).ToList();
                        var LTFFVG = LTFquote.Count >= 3 ? IsFairValueGap(LTFquote) : null;
                        if (LTFFVG == null || LTFFVG != BiasType.Bearish) continue;

                        //FindFVGs(section.ToList(), timeframe);
                        /*
                        var pivots = zigZagsHTF.Where(it => it.HighPoint != null || it.LowPoint != null);
                        if (pivots.Count() >= 2)
                        {

                        }
                        var lastPivots = pivots.Count() >= 2 ? pivots.TakeLast(2).ToList() : null;
                        if (lastPivots != null)
                        {
                            var ttt = 0;
                            // daca ultimul pivot nu a fost HighPoint, atunci nu a avut timp sa se formeze pentru ca s-ar fi format in urmatoarea candela de HTF dar nu a avut timp
                            // asa ca voi adauga High-ul maxim din range-ul de la Low pivot pana in momentul asta ca un High Pivot
                        }*/

                        var closestBearishHTF_FVG = HighSectionFVGs.Where(it => !it.IsMitigated //&& !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice)
                                                                             && it.BiasType == BiasType.Bearish)
                                                               .OrderBy(it => it.FvgAreaLow).FirstOrDefault();


                        if (closestBearishHTF_FVG != null) //a bearish FVG exists in the vecinity
                        {
                            var htfFVGBottom = closestBearishHTF_FVG.Structure.Min(it => it.LowPrice);
                            if (LTFquote[0].OpenTime < closestBearishHTF_FVG.FVGCloseTime) continue; // making sure the LTF FVG forms after the formation of the HTF FVG
                            var wasBearishHTFFVGLiquidated = sections[KlineInterval.FiveMinutes].Where(it => it.OpenTime >= closestBearishHTF_FVG.FVGOpenTime).Min(it => it.LowPrice) > htfFVGBottom;
                            if (wasBearishHTFFVGLiquidated) continue;

                            var firstTap = sections[eM.LowKlineInterval].Where(it => it.OpenTime > closestBearishHTF_FVG.Structure[2].CloseTime)
                                                  .FirstOrDefault(it => it.HighPrice > closestBearishHTF_FVG.FvgAreaLow);

                            if (firstTap != null) 
                                //check to see that we are doing this on the 
                            {

                                var firstUnmitigatedLTFFVG = FVGS[eM.LowKlineInterval].Where(it => it.FVGOpenTime >= firstTap.OpenTime && it.BiasType == BiasType.Bearish && !it.IsMitigated).FirstOrDefault();
                                if (firstUnmitigatedLTFFVG == null || firstUnmitigatedLTFFVG.FVGOpenTime != LTFquote[0].OpenTime) continue;

                                var indexofLTFFVG = sections[eM.LowKlineInterval].IndexOf(firstUnmitigatedLTFFVG.Structure[1]);
                                var barsBack = sections[eM.LowKlineInterval].Count - indexofLTFFVG;

                                if (barsBack < 50)
                                {

                                    var entrybool = true;
                                    var LTFFVG_Type = GetFVGType(LTFquote, -1);
                                    var entry = new fvgFutureEntry();
                                    entry.Invalidated = false;
                                    entry.HTF_FVG_CANDLE_TIME = closestBearishHTF_FVG.FVGOpenTime;
                                    entry.StopLoss = LTFquote[0].HighPrice;//LowSectionFVGs[j].FvgAreaLow; //NO WICK NOW
                                    if (LTFFVG_Type == FVGType.BreakAwayGapExpansion || LTFFVG_Type == FVGType.BreakAwayGapSneaky)
                                    {
                                        entry.PositionType = PositionType.Market;
                                        entry.EntryPrice = LTFquote[2].ClosePrice;
                                    }
                                    else
                                    {
                                        entry.PositionType = PositionType.Limit;
                                        entry.EntryPrice = LTFquote[2].HighPrice;
                                    }
                                    entry.TakeProfit = entry.EntryPrice - (entry.StopLoss - entry.EntryPrice) * rr;
                                    entry.InitialDate = LTFquote[2].CloseTime;
                                    entry.PositionSize = CalculatePositionSize(AccountValue, entry.EntryPrice, entry.StopLoss, RiskPercent);
                                    entry.TradeType = TradeType.Short;
                                    entry.EntryModelTF = eM;

                                    if (child.OpenTime.DayOfWeek == DayOfWeek.Saturday || child.OpenTime.DayOfWeek == DayOfWeek.Sunday)
                                    {
                                        entry.Invalidated = true;
                                    }

                                    /*
                                    if (entry.TakeProfit > htfFVGTop) //check to see if we're not going over the HTF top
                                    {
                                        entry.Invalidated = true;
                                    }*/

                                    /*
                                    if (!isFullFVG(LowSectionFVGs[j].Structure, BiasType.Bullish)) //checking if the current entry FVG is all same bias candles
                                    {
                                        entry.Invalidated = true;
                                    }*/

                                    if (entry.Invalidated == false && entry.PositionType == PositionType.Market && OpenPositions.Count == 0)
                                    {
                                        //if (!
                                        //(
                                        //    (entry.TradeType == TradeType.Long && ema50.Last().Ema > ema200.Last().Ema)
                                        //||
                                        //    (entry.TradeType == TradeType.Short && ema50.Last().Ema < ema200.Last().Ema)))
                                        //{
                                        //    entry.Invalidated = false;
                                        //}
                                        bool opened = OpenPositionIfPossible(entry, LTFquote[2].CloseTime);
                                    }
                                    fvgFutureEntries.Add(entry);
                                }
                            }
                        }

                        //var closestBearishHTF_FVG = HighSection.Where(it => !it.IsMitigated && !IsOverlapping(it.FvgAreaLow, it.FvgAreaHigh, child.LowPrice, child.HighPrice))
                        //                                       .OrderBy(it => it.FvgAreaLow).FirstOrDefault();
                    }
                }
            }
        }

        private static bool isFullFVG(List<IBinanceKline> structure, BiasType biasType)
        {
            var sign = biasType == BiasType.Bullish ? 1 : -1;

            return structure.Count(it => it.OpenPrice * sign < it.ClosePrice * sign) == 3;
        }

        private static void updateFVGList(List<FairValueGap> fairValueGaps, KlineInterval timeframe, IEnumerable<IBinanceKline> section, IBinanceKline candle)
        {
            if (section.Count() != 3) return;

            var newFVG = FindFVGs(section.ToList(), timeframe);
            if (newFVG.Count != 0)
            {
                if (fairValueGaps.Count == 0 || (newFVG.First().FVGOpenTime != fairValueGaps.Last().FVGOpenTime))
                {
                    if (timeframe == KlineInterval.OneDay)
                    {
                        var teer = 0;
                    }
                    fairValueGaps.Add(newFVG.First());
                }
            }

            // FVG Forgotten
            if (newFVG.Count != 0)
            {
                //fairValueGaps = fairValueGaps.Where(it => it.FVGTimeframe == timeframe && it.FVGOpenTime != newFVG.First().FVGOpenTime).Select(s => { s.IsForgotten = true; return s; }).ToList();
                foreach (var fvg in fairValueGaps)
                {
                    if (fvg.FVGTimeframe == timeframe && fvg.BiasType == newFVG.First().BiasType && fvg.FVGOpenTime != newFVG.First().FVGOpenTime)
                    {
                        fvg.IsForgotten = true;
                    }
                }
            }

            // FVG Liquidity Taken
            var priceMax = section.Max(it => it.HighPrice);
            var priceMin = section.Min(it => it.LowPrice);
            //fairValueGaps = fairValueGaps.Where(it => (it.BiasType == BiasType.Bullish && it.IsTapped && it.TopStructure < priceMax)
            //                                       || (it.BiasType == BiasType.Bearish && it.IsTapped && it.BottomStructure > priceMin))
            //                                        .Select(s => { s.IsLiquidityTakenAfterTap = true; return s; }).ToList();

            foreach (var fvg in fairValueGaps)
            {
                if ((fvg.BiasType == BiasType.Bullish && fvg.IsTapped && fvg.TopStructure < priceMax)
                 || (fvg.BiasType == BiasType.Bearish && fvg.IsTapped && fvg.BottomStructure > priceMin))
                {
                    fvg.IsLiquidityTakenAfterTap = true;
                }
            }

            // FVG Mitigation
            foreach (var fvg in fairValueGaps)
            {
                if (fvg.BiasType == BiasType.Bullish && fvg.BottomStructure > priceMin)//IsOverlappingEquals(fvg.FvgAreaLow, fvg.FvgAreaLow, candle.LowPrice, candle.HighPrice))
                {
                    fvg.IsMitigated = true;
                }
            }

            foreach (var fvg in fairValueGaps)
            {
                if (fvg.BiasType == BiasType.Bearish && fvg.TopStructure < priceMax)//IsOverlappingEquals(fvg.FvgAreaHigh, fvg.FvgAreaHigh, candle.LowPrice, candle.HighPrice))
                {
                    fvg.IsMitigated = true;
                }
            }

            // FVG Tapping
            foreach (var fvg in fairValueGaps)
            {
                if (IsOverlapping(fvg.FvgAreaLow, fvg.FvgAreaHigh, candle.LowPrice, candle.HighPrice))
                {
                    // remove "eaten" part of the FVG
                    if (fvg.BiasType == BiasType.Bullish) { 
                        fvg.FvgAreaHigh = candle.LowPrice; 
                    }
                    if (fvg.BiasType == BiasType.Bearish)
                    {
                        fvg.FvgAreaLow = candle.HighPrice;
                    }
                    fvg.FVGAreaMiddle = (fvg.FvgAreaHigh + fvg.FvgAreaLow) / 2;
                    fvg.IsTapped = true;
                }
            }

            fairValueGaps.RemoveAll(it => 
                                        it.IsLiquidityTakenAfterTap
                                        || it.IsMitigated
                                        || it.IsForgotten);
        }

        private static bool OpenPositionIfPossible(fvgFutureEntry entry, DateTime time)
        {
            if (!entry.Invalidated && OpenPositions.Count == 0 && entry.PositionSize < AccountValue / 5000)
            {
                Position p = new Position();
                p.StopLoss = entry.StopLoss.Value;
                p.Target = entry.TakeProfit.Value;
                p.Size = entry.PositionSize;
                p.EntryPrice = entry.EntryPrice.Value;
                p.OpenTime = time;
                p.Symbol = "BTCUSDT";
                p.TradeType = entry.TradeType;
                p.EntryTimeframe = entry.EntryModelTF;
                OpenPositions.Add(p);

                var fees = calculateFees(p.EntryPrice, p.Size, entry.PositionType == PositionType.Market ? takerFee : makerFee);
                p.Fees = fees.Value;
                MessageLog($"Entered {p.TradeType} on {entry.EntryModelTF.HighKlineInterval} / {entry.EntryModelTF.LowKlineInterval} entry model:\nTime: {p.OpenTime}\nHTF FVG OFF OF: {entry.HTF_FVG_CANDLE_TIME}\nPosition Size: {p.Size}\nEntry Price: {p.EntryPrice}\nStopLoss Price: {p.StopLoss}\nComission: {fees}", p.TradeType == TradeType.Long ? ConsoleColor.White : ConsoleColor.DarkGray);
                return true;
            } else
            {
                return false;
            }
        }

        private static void MessageLog(string v, ConsoleColor cc)
        {
            Console.ForegroundColor = cc;
            Console.WriteLine($"{v}\n\n ----\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static decimal CalculatePositionSize(decimal accountValue, decimal? entryPrice, decimal? stopLoss, decimal riskPercent)
        {
            return (accountValue * riskPercent / 100) / Math.Abs(entryPrice.Value - stopLoss.Value);
        }

        private static List<FairValueGap> FindFVGs(List<IBinanceKline> binanceKlines, KlineInterval timeframe)
        {
            List<FairValueGap> retFVG = new List<FairValueGap>();
            //var quote = binanceKlines.Take(3).ToList();
            for (int i = 0; i < binanceKlines.Count - 2; i++)
            {
                var quote = binanceKlines.Skip(i).Take(3).ToList();
                var isFVG = IsFairValueGap(quote);

                if (isFVG != null)
                {
                    FairValueGap fvg = new FairValueGap();
                    fvg.Structure = quote;
                    fvg.BiasType = isFVG.Value;
                    fvg.FvgAreaHigh = isFVG == BiasType.Bullish ? quote[2].LowPrice : Math.Max(quote[0].HighPrice, quote[1].HighPrice);
                    fvg.FvgAreaLow = isFVG == BiasType.Bullish ? Math.Min(quote[0].LowPrice, quote[1].LowPrice) : quote[2].HighPrice;
                    fvg.FVGAreaMiddle = (fvg.FvgAreaHigh + fvg.FvgAreaLow) / 2;
                    fvg.FVGOpenTime = quote[0].OpenTime;
                    fvg.FVGCloseTime = quote[2].CloseTime;
                    fvg.TopStructure = Math.Max(quote[0].HighPrice, Math.Max(quote[1].HighPrice, quote[2].HighPrice));
                    fvg.BottomStructure = Math.Min(quote[0].LowPrice, Math.Min(quote[1].LowPrice, quote[2].LowPrice));
                    fvg.IsLiquidityTakenAfterTap = false;
                    fvg.IsForgotten = false;
                    fvg.FVGTimeframe = timeframe;
                    fvg.IsMitigated = false;
                    fvg.IsTapped = false;

                    fvg.FVGType = GetFVGType(quote, sign: isFVG == BiasType.Bullish ? 1 : -1);
                    var sign = isFVG == BiasType.Bullish ? 1 : -1;

                    retFVG.Add(fvg);
                }

                //foreach (var child in retFVG)
                //{
                //    if (IsOverlapping(child.FvgAreaLow, child.FvgAreaHigh, binanceKlines[i + 2].LowPrice, binanceKlines[i + 2].HighPrice))
                //    {
                //        child.IsTapped = true;
                //    }
                //}
            }

            List<FairValueGap> retFVG2 = new List<FairValueGap>();
            foreach (var child in retFVG)
            {
                var section = binanceKlines.Where(it => it.OpenTime > child.Structure[2].OpenTime);
                retFVG2.Add(child);
                if (section.Count() > 0)
                {
                    var maxValue = section.Max(it => it.HighPrice);
                    var minValue = section.Min(it => it.LowPrice);
                    if ((child.BiasType == BiasType.Bullish && minValue < child.Structure[0].LowPrice)
                        || (child.BiasType == BiasType.Bearish && maxValue > child.Structure[0].HighPrice))
                    {
                        child.IsMitigated = true;
                        retFVG2.Remove(child);
                    }

                    if (IsOverlapping(child.FvgAreaLow, child.FvgAreaHigh, minValue, maxValue))
                    {
                        child.IsTapped = true;
                    }
                }
            }

            return retFVG2;
        }

        private static FVGType GetFVGType(List<IBinanceKline> quote, int sign)
        {
            if (quote[2].ClosePrice * sign > (sign == 1 ? quote[1].HighPrice : quote[1].LowPrice) * sign)
            {
                return FVGType.BreakAwayGapExpansion;
            }
            else
            {
                if (quote[2].ClosePrice * sign > quote[1].ClosePrice * sign && quote[2].ClosePrice * sign < (sign == 1 ? quote[1].HighPrice : quote[1].LowPrice) * sign)
                {
                    return FVGType.MagnetGapConsolidation;
                }
                else
                {
                    if (quote[2].ClosePrice * sign < (quote[1].ClosePrice + quote[1].OpenPrice) / 2 * sign)
                    {
                        return FVGType.MagnetGapDeepRetracement;
                    }
                    else
                    {
                        if ((sign == 1 ? quote[2].LowPrice : quote[2].HighPrice) * sign < ((quote[1].ClosePrice + quote[1].OpenPrice) / 2) * sign)
                        {
                            return FVGType.BreakAwayGapSneaky;
                        }
                        else
                        {
                            return FVGType.MagnetGapSimple;
                        }
                    }
                }
            }
            return FVGType.MagnetGapSimple;
        }

        private static async Task<Dictionary<KlineInterval, List<IBinanceKline>>> getAllData(Dictionary<KlineInterval, List<IBinanceKline>> timeFrameData, int days, string symbol)
        {
            var lineData = await GetAllKlinesInterval(symbol, KlineInterval.FiveMinutes, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);         
            timeFrameData.Add(KlineInterval.FiveMinutes, lineData);
            lineData = await GetAllKlinesInterval(symbol, KlineInterval.FifteenMinutes, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);
            timeFrameData.Add(KlineInterval.FifteenMinutes, lineData);
            lineData = await GetAllKlinesInterval(symbol, KlineInterval.OneHour, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);
            timeFrameData.Add(KlineInterval.OneHour, lineData);
            lineData = await GetAllKlinesInterval(symbol, KlineInterval.FourHour, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);
            timeFrameData.Add(KlineInterval.FourHour, lineData);
            lineData = await GetAllKlinesInterval(symbol, KlineInterval.OneDay, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);
            timeFrameData.Add(KlineInterval.OneDay, lineData);
            lineData = await GetAllKlinesInterval(symbol, KlineInterval.OneWeek, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);
            timeFrameData.Add(KlineInterval.OneWeek, lineData);
            lineData = await GetAllKlinesInterval(symbol, KlineInterval.OneMonth, DateTime.UtcNow.AddDays(-days).AddHours(-5), DateTime.UtcNow);
            timeFrameData.Add(KlineInterval.OneMonth, lineData);
            return timeFrameData;
        }

        private static bool IsOverlapping (decimal x1, decimal x2, decimal y1, decimal y2)
        {
            return Math.Max(x1, y1) < Math.Min(x2, y2);
        }
        private static bool IsOverlappingEquals(decimal x1, decimal x2, decimal y1, decimal y2)
        {
            return Math.Max(x1, y1) <= Math.Min(x2, y2);
        }

        private static void IdentifyUnmitigatedFVGs(List<IBinanceKline> lineDataDaily, bool excludeMitigated = true)
        {
            var FVGs = new List<FVG>();
            for (int i = 1; i < lineDataDaily.Count - 1; i++)
            {
                //bullish MAGNET FVG
                if (lineDataDaily[i].OpenPrice < lineDataDaily[i].ClosePrice)
                {
                    if (lineDataDaily[i - 1].HighPrice >= lineDataDaily[i].OpenPrice && lineDataDaily[i - 1].HighPrice <= lineDataDaily[i].ClosePrice // check for structure 1, 2, 3 candle
                     && lineDataDaily[i + 1].LowPrice <= lineDataDaily[i].ClosePrice && lineDataDaily[i + 1].LowPrice >= lineDataDaily[i].OpenPrice)
                    {
                        if (lineDataDaily[i + 1].ClosePrice < lineDataDaily[i].HighPrice) // check if magnet FVG (that it closes not above the 2FVG candle high)
                        {
                            FVGs.Add(new FVG { Kline = lineDataDaily[i], BiasType = BiasType.Bullish, Mitigated = false });
                        }
                    }
                }

                //bearish MAGNET FVG
                if (lineDataDaily[i].OpenPrice > lineDataDaily[i].ClosePrice)
                {
                    if (lineDataDaily[i - 1].LowPrice <= lineDataDaily[i].OpenPrice && lineDataDaily[i - 1].LowPrice >= lineDataDaily[i].ClosePrice // check for structure 1, 2, 3 candle
                     && lineDataDaily[i + 1].HighPrice >= lineDataDaily[i].ClosePrice && lineDataDaily[i + 1].HighPrice <= lineDataDaily[i].OpenPrice)
                    {
                        if (lineDataDaily[i + 1].ClosePrice > lineDataDaily[i].LowPrice) // check if magnet FVG (that it closes not below the 2VFG candle low)
                        {
                            FVGs.Add(new FVG { Kline = lineDataDaily[i], BiasType = BiasType.Bearish, Mitigated = false });
                        }
                    }
                }
            }

            if (excludeMitigated)
            {
                foreach (var child in FVGs)
                {
                    var section = lineDataDaily.Where(it => it.OpenTime >= child.Kline.OpenTime).Skip(2).ToList();
                    foreach (var child2 in section)
                    {
                        if (FindOverlapping(child.Kline.OpenPrice, child.Kline.ClosePrice, child2.LowPrice, child2.HighPrice) > 0)
                        {
                            child.Mitigated = true;
                        }
                    }
                    if (child.Mitigated == false)
                    {
                        var ttg = 0;
                    }
                }
            }
            
            foreach (var child in FVGs)
            {
                if (!child.Mitigated)
                {
                    Console.WriteLine($"Unmitigated Magnet FVG of BiasType {(child.BiasType == BiasType.Bullish ? "Bullish" : "Bearish")} @ Time: {child.Kline.OpenTime}");
                }
            }
        }

        private static void DisplayVolumeProfileWithRatios(List<VolumeProfile> vp, decimal ret1, decimal ret0)
        {
            decimal diff = ret1 - ret0;
            decimal ret0236 = ret0 + diff * 0.236m;
            decimal ret0382 = ret0 + diff * 0.382m;
            decimal ret05 = ret0 + diff * 0.5m;
            decimal ret0618 = ret0 + diff * 0.618m;
            decimal ret065 = ret0 + diff * 0.65m;
            decimal ret0705 = ret0 + diff * 0.705m;
            decimal ret0786 = ret0 + diff * 0.786m;
            for (int i = vp.Count - 1; i >= 0; i--)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                bool ok = false;
                if (vp[i].interval.Item1 <= ret0 && vp[i].interval.Item2 >= ret0)       { Console.Write("RET 0     :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret0236 && vp[i].interval.Item2 >= ret0236) { Console.Write("RET 0236  :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret0382 && vp[i].interval.Item2 >= ret0382) { Console.Write("RET 0382  :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret05 && vp[i].interval.Item2 >= ret05)     { Console.Write("RET 05    :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret0618 && vp[i].interval.Item2 >= ret0618) { Console.Write("RET 0618  :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret065 && vp[i].interval.Item2 >= ret065)   { Console.Write("RET 065   :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret0705 && vp[i].interval.Item2 >= ret0705) { Console.Write("RET 0705  :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret0786 && vp[i].interval.Item2 >= ret0786) { Console.Write("RET 0786  :  "); ok = true; }
                if (vp[i].interval.Item1 <= ret1 && vp[i].interval.Item2 >= ret1)       { Console.Write("RET 1     :  "); ok = true; }

                if (!ok)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("          :  ");
                }

                Console.Write("{0} - {1}: ", vp[i].interval.Item1, vp[i].interval.Item2);
                for (int j = 0; j < vp[i].sum / 600; j++)
                {
                    Console.Write(".");
                }

                if (ok)
                {
                    Console.Write("                    ");
                    for (int j = 0; j < 300 - vp[i].sum / 600; j++)
                    {
                        Console.Write("-");
                    }
                }
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static void BPRFinder(List<IBinanceKline> lineData)
        {
            lineData.Reverse();
            int[] arr = Enumerable.Repeat(0, 40000).ToArray();
            var bprs = new List<BPR>();

            foreach (var child in lineData)
            {
                var low = (int)Math.Floor(child.LowPrice);
                var high = (int)Math.Floor(child.HighPrice);
                var open = (int)Math.Floor(child.OpenPrice);
                var close = (int)Math.Floor(child.ClosePrice);

                arr = AddToArray(arr, -1, Math.Min(open, close), Math.Max(open, close));
                arr = AddToArray(arr, 1000, low, Math.Min(open, close) - 1);
                arr = AddToArray(arr, 1000, Math.Max(open, close) + 1, high);

                for (int st = -3; st >= -5; st--)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i] <= st)
                        {
                            int j = i;
                            for (; j + 1 < arr.Length && arr[j + 1] <= st; j++) ;

                            if (j - i + 1 > 30)
                            {
                                //Console.WriteLine($"Found {st} BPR between {i} and {j} starting from {child.OpenTime} - it has a length of {j - i + 1}");
                                var bpr = bprs.FirstOrDefault(it => it.Low == i && it.High == j && it.Depth == st);

                                if (bpr == null)
                                {
                                    var newBpr = new BPR();
                                    newBpr.Depth = st;
                                    newBpr.Low = i;
                                    newBpr.High = j;
                                    bprs.Add(newBpr);
                                }
                            }
                            i = j;
                        }
                    }
                }
            }


            bprs = bprs.OrderBy(it => it.Depth).ToList();
            return;
        }

        private static int[] AddToArray(int[] arr, int value, int start, int finish)
        {
            for (int i = start; i <= finish; i++)
            {
                arr[i] += value;
            }
            return arr;
        }

        public static async void GetHooks()
        {
            while (true)
            {
                if (DateTime.UtcNow.Second == 8 && DateTime.UtcNow.Minute != minuteWebHook)
                {
                    minuteWebHook = DateTime.UtcNow.Minute;
                    OnCheckWebhook();
                }
            }
        }

        static public int secondSwitch10 = -1;
        static public int secondSwitch30 = -1;
        static public int minuteWebHook = -1;

        private static async Task liveStrat11VolumeProfileReversalFib()
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            var lineData = await GetAllKlinesInterval("BTCUSDT", KlineInterval.OneMinute, DateTime.UtcNow.AddMinutes(-398), DateTime.UtcNow);//"BTCUSDT"
            //lineData = lineData.Skip(Math.Max(0, lineData.Count() - 400)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            volumeProfileCalculation(lineData);

            var openPositions = new List<binanceOpenFuture>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var buEntries = new List<futureEntry>();
            var beEntries = new List<futureEntry>();

            await socketClient.SpotApi.ExchangeData.SubscribeToKlineUpdatesAsync("BTCUSDT", KlineInterval.OneSecond, async data =>
            {
                //Console.WriteLine($"BTCUSDT PRICE: # {BTCUSDTprice}");
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();

                //Console.WriteLine("Lalalala");

                //TREBUIE MUTATA LOGICA IN AFARA CA SA SE POATA FACE UPDATE PE VALORI LA FIECARE TICK

                if (existingKline == null)
                {
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    existingKline = new bsk()
                    {
                        CloseTime = data.Data.Data.CloseTime,
                        OpenTime = data.Data.Data.OpenTime
                    };

                    //modify stop loss for open position
                    lineSim = lineData.ToList();
                    lineData.Add(existingKline);
                    lineData.RemoveAt(0); //when adding a new Kline, remove the first in the list so it stays the same length
                    Console.WriteLine($"New kline: {existingKline.OpenTime}-{existingKline.CloseTime} | No K: {lineData.Count} | Low: {data.Data.Data.LowPrice} | High: {data.Data.Data.HighPrice} | Volume: {data.Data.Data.Volume}");

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim); //.Take(390).ToList()
                }

                //var childOP = openPositions.FirstOrDefault();

                // Update the data of the object
                existingKline.HighPrice = data.Data.Data.HighPrice;
                existingKline.LowPrice = data.Data.Data.LowPrice;
                existingKline.ClosePrice = data.Data.Data.ClosePrice;
                existingKline.OpenPrice = data.Data.Data.OpenPrice;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.Volume = data.Data.Data.Volume;

                if (candleStarted)
                {
                    candleStarted = false;

                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
        }

        private static async Task liveStrat10TheOne()
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.ThirtyMinutes, DateTime.UtcNow.AddDays(-25), DateTime.UtcNow);//"BTCUSDT"
            lineData = lineData.Skip(Math.Max(0, lineData.Count() - 400)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var buEntries = new List<futureEntry>();
            var beEntries = new List<futureEntry>();

            await socketClient.UsdFuturesApi.ExchangeData.SubscribeToKlineUpdatesAsync("BTCBUSD", KlineInterval.OneMinute, async data =>
            {
                //Console.WriteLine($"BTCUSDT PRICE: # {BTCUSDTprice}");
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();

                //TREBUIE MUTATA LOGICA IN AFARA CA SA SE POATA FACE UPDATE PE VALORI LA FIECARE TICK

                if (existingKline == null)
                {
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    existingKline = new bsk()
                    {
                        CloseTime = data.Data.Data.CloseTime,
                        OpenTime = data.Data.Data.OpenTime
                    };

                    //modify stop loss for open position
                    lineSim = lineData.ToList();
                    lineData.Add(existingKline);
                    lineData.RemoveAt(0); //when adding a new Kline, remove the first in the list so it stays the same length
                    Console.WriteLine($"Added new kline: {existingKline.OpenTime}-{existingKline.CloseTime} | Number of Klines: {lineData.Count}");

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim); //.Take(390).ToList()

                    if (quotes.Count() >= 80)
                    {
                        for (int q = quotes.Count() - 130; q <= quotes.Count(); q++)
                        {
                            bool BullDFlag = false;
                            bool BearDFlag = false;
                            int MinPeriod = 4;
                            int Period = 120;

                            //quotes = quotes.Where(it => it.Date < new DateTime(2023, 05, 25, 11, 08, 00)).ToList();
                            var quotes1 = quotes.Take(q);

                            var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 12, Deviation: 5, BackStep: 2);
                            //var t = 0;

                            var pivots = zigZags.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                            var _rsi = quotes1.Use(CandlePart.OHLC4).GetRsi(21);
                            var _momentum = quotes1.GetRoc(20);
                            var _cmo = quotes1.GetCmo(14);

                            for (int i = MinPeriod; i <= Period; i++)
                            {
                                if (_cmo.Last().Cmo > _cmo.Reverse().Skip(i).FirstOrDefault().Cmo)
                                {
                                    if (quotes1.Last().Close < quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                    {
                                        if (quotes1.Last().Low <= quotes1.Reverse().Take(i).Min(it => it.Low))
                                        {
                                            if (_rsi.Reverse().Take(i).Min(it => it.Rsi) <= 40.0 && _rsi.Last().Rsi <= 40.0)
                                            {
                                                BullDFlag = true;
                                            }
                                        }
                                    }
                                }
                                else if (_cmo.Last().Cmo < _cmo.Reverse().Skip(i).FirstOrDefault().Cmo)
                                {
                                    if (quotes1.Last().Close > quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                    {
                                        if (quotes1.Last().High >= quotes1.Reverse().Take(i).Max(it => it.High))
                                        {
                                            if (_rsi.Reverse().Take(i).Max(it => it.Rsi) >= 60.0 && _rsi.Last().Rsi >= 60.0)
                                            {
                                                BearDFlag = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (BullDFlag)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine($"BULLISH FLAG - {quotes1.Last().Date} - Posted @ {quotes1.Last().Date}");
                                Console.ForegroundColor = ConsoleColor.White;

                                var highs = pivots.Where(it => it.PointType == "H" && it.Date <= quotes1.Last().Date).Reverse().ToList();

                                int cnt = 0;
                                while (cnt + 1 < highs.Count)
                                {
                                    var theCurrLow = cnt == 0 ? quotes1.Last().Low : pivots[pivots.FindIndex(it => it.Date == highs[cnt].Date) + 1].ZigZag;
                                    var theNextLow = pivots[pivots.FindIndex(it => it.Date == highs[cnt + 1].Date) + 1].ZigZag;
                                    if (highs[cnt].ZigZag > highs[cnt + 1].ZigZag && theNextLow < theCurrLow)
                                    {
                                        break;
                                    }
                                    cnt++;
                                }
                                var theHighValue = highs.Take(cnt + 1).Max(it => it.ZigZag);
                                Console.WriteLine($"The TOP of the Fibonacci is @ {highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date}");

                                if (buEntries.LastOrDefault(it => !it.Invalidated) == null || buEntries.Last(it => !it.Invalidated).ret0 > quotes1.Last().Low)
                                {
                                    foreach (var child in buEntries) child.Invalidated = true; //Invalidate all previous Bull entries
                                    foreach (var child in beEntries) child.Invalidated = true; //Invalidate all previous Bear entries


                                    var bullEntry = new futureEntry(); //Create new Bullish entry
                                    bullEntry.InitialDate = quotes1.Last().Date;
                                    bullEntry.ret0 = quotes1.Last().Low;
                                    bullEntry.ret1 = highs.FirstOrDefault(it => it.ZigZag == theHighValue).ZigZag.Value;
                                    decimal? diff = bullEntry.ret1 - bullEntry.ret0;
                                    bullEntry.ret0236 = bullEntry.ret0 + diff * 0.236m;
                                    bullEntry.ret0382 = bullEntry.ret0 + diff * 0.382m;
                                    bullEntry.ret05 = bullEntry.ret0 + diff * 0.5m;
                                    bullEntry.ret0618 = bullEntry.ret0 + diff * 0.618m;
                                    bullEntry.ret0786 = bullEntry.ret0 + diff * 0.786m;
                                    bullEntry.ret1618 = bullEntry.ret0 + diff * 1.618m;
                                    bullEntry.entryPrice = bullEntry.ret0236 + ((bullEntry.ret0382 - bullEntry.ret0236) / 5); //20% towards the next retracement (0.382)
                                    bullEntry.Invalidated = false;
                                    bullEntry.topFib = highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date;
                                    buEntries.Add(bullEntry);
                                }
                            }
                            if (BearDFlag)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"BEARISH FLAG - {quotes1.Last().Date} - Posted @ {quotes1.Last().Date}");
                                Console.ForegroundColor = ConsoleColor.White;

                                var lows = pivots.Where(it => it.PointType == "L" && it.Date <= quotes1.Last().Date).Reverse().ToList();

                                int cnt = 0;
                                while (cnt + 1 < lows.Count)
                                {
                                    var theCurrHigh = cnt == 0 ? quotes1.Last().High : pivots[pivots.FindIndex(it => it.Date == lows[cnt].Date) + 1].ZigZag;
                                    var theNextHigh = pivots[pivots.FindIndex(it => it.Date == lows[cnt + 1].Date) + 1].ZigZag;
                                    if (lows[cnt].ZigZag < lows[cnt + 1].ZigZag && theNextHigh > theCurrHigh)
                                    {
                                        break;
                                    }
                                    cnt++;
                                }
                                var theLowValue = lows.Take(cnt + 1).Min(it => it.ZigZag);
                                Console.WriteLine($"The BOTTOM of the Fibonacci is @ {lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date}");

                                if (beEntries.LastOrDefault(it => !it.Invalidated) == null || beEntries.Last(it => !it.Invalidated).ret0 < quotes1.Last().High)
                                {
                                    foreach (var child in buEntries) child.Invalidated = true; //Invalidate all previous Bull entries
                                    foreach (var child in beEntries) child.Invalidated = true; //Invalidate all previous Bear entries

                                    var bearEntry = new futureEntry(); //Create new Bearish entry
                                    bearEntry.InitialDate = quotes1.Last().Date;
                                    bearEntry.ret0 = quotes1.Last().High;
                                    bearEntry.ret1 = lows.FirstOrDefault(it => it.ZigZag == theLowValue).ZigZag.Value;
                                    decimal? diff = bearEntry.ret0 - bearEntry.ret1;
                                    bearEntry.ret0236 = bearEntry.ret0 - diff * 0.236m;
                                    bearEntry.ret0382 = bearEntry.ret0 - diff * 0.382m;
                                    bearEntry.ret05 = bearEntry.ret0 - diff * 0.5m;
                                    bearEntry.ret0618 = bearEntry.ret0 - diff * 0.618m;
                                    bearEntry.ret0786 = bearEntry.ret0 - diff * 0.786m;
                                    bearEntry.ret1618 = bearEntry.ret0 - diff * 1.618m;
                                    bearEntry.entryPrice = bearEntry.ret0236 - ((bearEntry.ret0236 - bearEntry.ret0382) / 5); //20% towards the next retracement (0.382)
                                    bearEntry.Invalidated = false;
                                    bearEntry.topFib = lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date;
                                    beEntries.Add(bearEntry);
                                }
                            }
                        }
                        var gg = 0;

                        //Invalidate Bear if needed
                        if (beEntries.LastOrDefault(it => !it.Invalidated) != null)
                        {
                            var maxAfterEntry = quotes.Where(it => it.Date >= beEntries.Last(it2 => !it2.Invalidated).InitialDate).Max(it => it.High);
                            if (maxAfterEntry > beEntries.Last(it => !it.Invalidated).ret0)
                            {
                                beEntries.Last(it => !it.Invalidated).Invalidated = true;
                                Console.WriteLine($"Invalidated latest bearish entry");
                            }
                        }

                        //Invalidate Bull if needed
                        if (buEntries.LastOrDefault(it => !it.Invalidated) != null)
                        {
                            var minAfterEntry = quotes.Where(it => it.Date >= buEntries.Last(it2 => !it2.Invalidated).InitialDate).Min(it => it.Low);
                            if (minAfterEntry < buEntries.Last(it => !it.Invalidated).ret0)
                            {
                                buEntries.Last(it => !it.Invalidated).Invalidated = true;
                                Console.WriteLine($"Invalidated latest bullish entry");
                            }
                        }

                        Console.WriteLine("-------------------------------------------------");
                        buEntries.RemoveAll(it => it.Invalidated);
                        beEntries.RemoveAll(it => it.Invalidated);
                        foreach (var child in buEntries)
                        {
                            Console.WriteLine($"BULLISH POSSIBLE ENTRY @ - {child.InitialDate} - BETWEEN {child.ret0} AND {child.ret1}");
                        }
                        foreach (var child in beEntries)
                        {
                            Console.WriteLine($"BEARISH POSSIBLE ENTRY @ - {child.InitialDate} - BETWEEN {child.ret0} AND {child.ret1}");
                        }
                        if (buEntries.Count == 0 && beEntries.Count == 0)
                        {
                            Console.WriteLine("CURRENTLY - NO ENTRIES");
                        }
                    }
                    if (true) //switch from true / false - LIVE / TESTING
                    {
                        var realOpenPos = await client.UsdFuturesApi.Account.GetPositionInformationAsync("BTCBUSD");
                        if (realOpenPos.Success == true
                            && realOpenPos.Data.FirstOrDefault().Quantity <= 0)// && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0) /// MINUTE == MINUTE
                        {
                            // check for LONG POSITION
                            var bullEntry = buEntries.LastOrDefault(it => !it.Invalidated);
                            //LONG CHECK
                            if (bullEntry != null) Console.WriteLine($"Current Bull Entry: {bullEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");
                            if (bullEntry != null
                                && lineSim.Last().OpenPrice <= bullEntry.entryPrice
                                && lineSim.Last().ClosePrice >= bullEntry.entryPrice)
                                //&& quotes.Reverse().Skip(1).First().Open <= bullEntry.entryPrice
                                //&& quotes.Reverse().Skip(1).First().Close >= bullEntry.entryPrice)
                            {
                                Console.WriteLine($"LONG SIGNAL!");
                                Console.WriteLine("Clearing other orders ...");
                                var canRes = await client.UsdFuturesApi.Trading.CancelAllOrdersAsync("BTCBUSD");
                                Console.WriteLine($"Executed with status: {canRes.Success}");
                                if (realOpenPos.Data.FirstOrDefault().Quantity < 0)
                                {
                                    //close previous position
                                    Console.WriteLine($"Sending request to CLOSE previous SHORT position ... Current price is: {BTCUSDTprice}");

                                    var ClosePosition = await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD"
                                        , OrderSide.Buy
                                        , FuturesOrderType.Market
                                        , quantity: Math.Abs(realOpenPos.Data.FirstOrDefault().Quantity)
                                        , workingType: WorkingType.Mark);

                                    if (ClosePosition.Success)
                                    {
                                        Console.WriteLine("Position closed!");
                                    }
                                }

                                Console.WriteLine($"Sending long ORDER request ... Current price is: {BTCUSDTprice}");

                                var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, FuturesOrderType.Market, quantity: investment);

                                if (result.Success)//false
                                {
                                    //var tradeInfo = new futureEntry(bullEntry);
                                    realOpenPos = await client.UsdFuturesApi.Account.GetPositionInformationAsync("BTCBUSD");
                                    var position = realOpenPos.Data.First();

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = DateTime.UtcNow,
                                        price = position.EntryPrice,
                                        riskRatio = (decimal)1.5,
                                        stopLimitPrice = bullEntry.ret0, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                        type = "long",
                                    });

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"LONG OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    //Console.Write($"LONG OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                                    try
                                    {
                                        var SL = await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                                                                , FuturesOrderType.StopMarket
                                                                                , quantity: investment
                                                                                , stopPrice: Roundto1(bullEntry.ret0 - 15)
                                                                                , timeInForce: TimeInForce.GoodTillCanceled
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark);
                                        var TP1 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, Roundto1(bullEntry.ret0382)); //make approximations to ONE DECIMAL
                                        if (TP1.Success) Console.WriteLine($"Successfully placed TP1 at {Roundto1(bullEntry.ret0382)}"); else Console.WriteLine($"TP1 at {Roundto1(bullEntry.ret0382)}: {TP1.Error}");
                                        var TP2 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, Roundto1(bullEntry.ret05));
                                        if (TP2.Success) Console.WriteLine($"Successfully placed TP2 at {Roundto1(bullEntry.ret05)}"); else Console.WriteLine($"TP2 at {Roundto1(bullEntry.ret05)}: {TP2.Error}");
                                        var TP3 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, Roundto1(bullEntry.ret0618));
                                        if (TP3.Success) Console.WriteLine($"Successfully placed TP3 at {Roundto1(bullEntry.ret0618)}"); else Console.WriteLine($"TP3 at {Roundto1(bullEntry.ret0618)}: {TP3.Error}");
                                        var TP4 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, Roundto1(bullEntry.ret0786));
                                        if (TP4.Success) Console.WriteLine($"Successfully placed TP4 at {Roundto1(bullEntry.ret0786)}"); else Console.WriteLine($"TP4 at {Roundto1(bullEntry.ret0786)}: {TP4.Error}");
                                        var TP5 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, Roundto1(bullEntry.ret1));
                                        if (TP5.Success) Console.WriteLine($"Successfully placed TP5 at {Roundto1(bullEntry.ret1)}"); else Console.WriteLine($"TP5 at {Roundto1(bullEntry.ret1)}: {TP5.Error}");
                                        var TP6 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, Roundto1(bullEntry.ret1618));
                                        if (TP6.Success) Console.WriteLine($"Successfully placed TP6 at {Roundto1(bullEntry.ret1618)}"); else Console.WriteLine($"TP6 at {Roundto1(bullEntry.ret1618)}: {TP6.Error}");

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Place order: {result.Error}");
                                }
                            }
                        }

                        if (realOpenPos.Success == true
                            && realOpenPos.Data.FirstOrDefault().Quantity >= 0)//&& realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0)
                        {
                            var bearEntry = beEntries.LastOrDefault(it => !it.Invalidated);
                            if (bearEntry != null) Console.WriteLine($"Current Bear Entry: {bearEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");
                            if (bearEntry != null
                                && lineSim.Last().OpenPrice >= bearEntry.entryPrice
                                && lineSim.Last().ClosePrice <= bearEntry.entryPrice)
                                //&& quotes.Reverse().Skip(1).First().Open >= bearEntry.entryPrice
                                //&& quotes.Reverse().Skip(1).First().Close <= bearEntry.entryPrice)
                            {
                                Console.WriteLine($"SHORT SIGNAL!");
                                Console.WriteLine("Clearing other orders ...");
                                var canRes = await client.UsdFuturesApi.Trading.CancelAllOrdersAsync("BTCBUSD");
                                Console.WriteLine($"Executed with status: {canRes.Success}");
                                if (realOpenPos.Data.FirstOrDefault().Quantity > 0)
                                {
                                    //close previous position
                                    Console.WriteLine($"Sending request to CLOSE previous position ... Current price is: {BTCUSDTprice}");

                                    var ClosePosition = await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD"
                                        , OrderSide.Sell
                                        , FuturesOrderType.Market
                                        , quantity: Math.Abs(realOpenPos.Data.FirstOrDefault().Quantity)
                                        , workingType: WorkingType.Mark);

                                    if (ClosePosition.Success)
                                    {
                                        Console.WriteLine("Position closed!");
                                    }
                                }
                                Console.Write($"Sending short position request ... Current price is: {BTCUSDTprice}");

                                var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, FuturesOrderType.Market, quantity: investment);

                                if (result.Success)//false
                                {
                                    realOpenPos = await client.UsdFuturesApi.Account.GetPositionInformationAsync("BTCBUSD");
                                    var position = realOpenPos.Data.First();

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = DateTime.UtcNow,
                                        price = position.EntryPrice,
                                        riskRatio = (decimal)1.5,
                                        stopLimitPrice = bearEntry.ret0, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                        type = "short",
                                    });

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"SHORT OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    //Console.Write($"SHORT OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                                    try
                                    {
                                        var SL = await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                                , FuturesOrderType.StopMarket
                                                                                , quantity: investment
                                                                                , stopPrice: Roundto1(bearEntry.ret0 + 15)
                                                                                , timeInForce: TimeInForce.GoodTillCanceled
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark);
                                        var TP1 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, Roundto1(bearEntry.ret0382));//make approximations to ONE DECIMAL
                                        if (TP1.Success) Console.WriteLine($"Successfully placed TP1 at {Roundto1(bearEntry.ret0382)}"); else Console.WriteLine($"TP1 at {Roundto1(bearEntry.ret0382)}: {TP1.Error}"); //EROARE AICI CU TP1.ERROR
                                        var TP2 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, Roundto1(bearEntry.ret05));
                                        if (TP2.Success) Console.WriteLine($"Successfully placed TP2 at {Roundto1(bearEntry.ret05)}"); else Console.WriteLine($"TP2 at {Roundto1(bearEntry.ret05)}: {TP2.Error}");
                                        var TP3 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, Roundto1(bearEntry.ret0618));
                                        if (TP3.Success) Console.WriteLine($"Successfully placed TP3 at {Roundto1(bearEntry.ret0618)}"); else Console.WriteLine($"TP3 at {Roundto1(bearEntry.ret0618)}: {TP3.Error}");
                                        var TP4 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, Roundto1(bearEntry.ret0786));
                                        if (TP4.Success) Console.WriteLine($"Successfully placed TP4 at {Roundto1(bearEntry.ret0786)}"); else Console.WriteLine($"TP4 at {Roundto1(bearEntry.ret0786)}: {TP4.Error}");
                                        var TP5 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, Roundto1(bearEntry.ret1));
                                        if (TP5.Success) Console.WriteLine($"Successfully placed TP5 at {Roundto1(bearEntry.ret1)}"); else Console.WriteLine($"TP5 at {Roundto1(bearEntry.ret1)}: {TP5.Error}");
                                        var TP6 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, Roundto1(bearEntry.ret1618));
                                        if (TP6.Success) Console.WriteLine($"Successfully placed TP6 at {Roundto1(bearEntry.ret1618)}"); else Console.WriteLine($"TP6 at {Roundto1(bearEntry.ret1618)}: {TP6.Error}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Place order: {result.Error}");
                                }
                            }
                        }
                        Console.WriteLine("-------------------------");
                    }
                }


                //CLEAR OPEN POSITIONS AFTER BINANCE POSITIONS CLEARED

                if (false && openPositions.Count > 0 && DateTime.UtcNow.Second % 10 == 0 && DateTime.UtcNow.Second != secondSwitch30)
                {
                    secondSwitch30 = DateTime.UtcNow.Second;
                    var posToClear = client.UsdFuturesApi.Account.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                    if (posToClear.Quantity == 0 && (posToClear.EntryPrice == 0 || posToClear.EntryPrice == -1))
                    {
                        openPositions.Clear();
                        var closeOpenPosition = await client.UsdFuturesApi.Trading.CancelAllOrdersAsync("BTCBUSD");
                        if (closeOpenPosition.Success)
                        {
                            Console.WriteLine($"CLEARED OPEN POSITION | Clear Orders Message: {closeOpenPosition.Data}");
                        }
                        else
                        {
                            Console.WriteLine($"{closeOpenPosition.Error}");
                        }
                    }
                }

                //var childOP = openPositions.FirstOrDefault();

                // Update the data of the object
                existingKline.HighPrice = data.Data.Data.HighPrice;
                existingKline.LowPrice = data.Data.Data.LowPrice;
                existingKline.ClosePrice = data.Data.Data.ClosePrice;
                existingKline.OpenPrice = data.Data.Data.OpenPrice;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.Volume = data.Data.Data.Volume;

                if (candleStarted)
                {
                    candleStarted = false;

                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
        }

        private static void BACKTEST12_FVG_NY_OPEN(List<IBinanceKline> lineData, List<IBinanceKline> lineDataOneMin)
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneHour, DateTime.UtcNow.AddDays(-25), DateTime.UtcNow);//"BTCUSDT"
            //lineData = lineData.Skip(Math.Max(0, lineData.Count() - 400)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var positionHistories = new List<positionHistory>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var buEntries = new List<fvgFutureEntry>();
            var beEntries = new List<fvgFutureEntry>();
            int tresh = 300;

            totalProfit = 3000;
            int entriesInSilverBullet = 0;
            bool inDayOfWeek = false;
            bool inSilverBulletSession = false;
            bool inTradeSession = false;

            for (int d = 2; d < lineData.Count; d++)
            {
                if (lineData[d].OpenTime >= new DateTime(2023, 9, 14, 14, 0, 0))
                {
                    var yy = 0;
                }
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();

                if (lineData[d - 2].OpenTime.AddHours(-4).DayOfWeek != DayOfWeek.Saturday && lineData[d - 2].OpenTime.AddHours(-4).DayOfWeek != DayOfWeek.Sunday)
                {
                    inDayOfWeek = true;
                } else
                {
                    inDayOfWeek = false;
                }

                if (inDayOfWeek
                    && lineData[d - 2].OpenTime.AddHours(-4).Hour >= 10 && lineData[d - 2].OpenTime.AddHours(-4).Hour <= 11)
                {
                    inSilverBulletSession = true;
                } else
                {
                    inSilverBulletSession = false;
                    entriesInSilverBullet = 0;
                }

                if (inDayOfWeek 
                    && lineData[d - 2].OpenTime.AddHours(-4).Hour >= 10 && lineData[d - 2].OpenTime.AddHours(-4).Hour <= 16)
                {
                    inTradeSession = true;
                } else
                {
                    buEntries.Clear();
                    beEntries.Clear();
                    inTradeSession = false;
                }

                if (openPositions.Count > 0)
                {
                    var openPos = openPositions.First();
                    if (openPos.type == "long")
                    {
                        // CHECK STOPLOSS
                        if (openPos.stopLimitPrice > lineData[d].LowPrice || lineData[d].OpenTime.AddHours(-4).Hour >= 16)
                        {
                            if (lineData[d].OpenTime.AddHours(-4).Hour >= 16)
                            {
                                Console.WriteLine("STOPPED BECAUSE AFTER 4 PM New York Time");
                            }
                            Console.WriteLine($"Current Profit: {totalProfit}");
                            var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice - 5, openPos.Quantity, 1, 1);
                            totalProfit += cp;

                            //positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });

                            if (cp < 0)
                            {
                                losses++;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                wins++;
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            buEntries.Clear();
                            beEntries.Clear();
                            //Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                            openPositions.Clear();
                            continue;
                        }
                        else
                        {
                            // CHECK PROFIT TARGET
                            int cnt = 0;
                            if (openPos.targetPrice <= lineData[d].HighPrice)
                            {
                                wins++;
                                var cp = calculatePnL(openPos.EntryPrice, openPos.targetPrice, openPos.Quantity, 1, 1);
                                totalProfit += cp;

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Took PROFIT at {openPos.targetPrice}, Quantity of {openPos.Quantity}, @ {lineData[d].OpenTime}. WIN of {cp}");
                                Console.ForegroundColor = ConsoleColor.White;
                                openPositions.Clear();
                                buEntries.Clear();
                                beEntries.Clear();
                            }
                            cnt++;
                        }
                    }

                    if (openPos.type == "short")
                    {
                        // CHECK STOPLOSS
                        if (openPos.stopLimitPrice < lineData[d].HighPrice || lineData[d].OpenTime.AddHours(-4).Hour >= 16)
                        {
                            if (lineData[d].OpenTime.AddHours(-4).Hour >= 16)
                            {
                                Console.WriteLine("STOPPED BECAUSE AFTER 4 PM New York Time");
                            }
                            Console.WriteLine($"Current Profit: {totalProfit}");
                            var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice + 5, openPos.Quantity, 1, -1);
                            totalProfit += cp;

                            //positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });

                            if (cp < 0)
                            {
                                losses++;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                wins++;
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            buEntries.Clear();
                            beEntries.Clear();
                            //Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                            openPositions.Clear();
                            continue;
                        }
                        else
                        {
                            // CHECK PROFIT TARGET
                            int cnt = 0;
                            if (openPos.targetPrice >= lineData[d].LowPrice)
                            {
                                wins++;
                                var cp = calculatePnL(openPos.EntryPrice, openPos.targetPrice, openPos.Quantity, 1, -1);
                                totalProfit += cp;

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Took PROFIT at {openPos.targetPrice}, Quantity of {openPos.Quantity}, @ {lineData[d].OpenTime}. WIN of {cp}");
                                Console.ForegroundColor = ConsoleColor.White;
                                openPositions.Clear();
                                buEntries.Clear();
                                beEntries.Clear();
                            }
                            cnt++;
                        }
                    }
                }

                //TREBUIE MUTATA LOGICA IN AFARA CA SA SE POATA FACE UPDATE PE VALORI LA FIECARE TICK

                if (true)
                {
                    investment = totalProfit.Value / 10000m;
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    //modify stop loss for open position
                    lineSim = lineData.Skip(d - 2).Take(tresh).ToList();

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                    //CHECK IF FVG in the last 3 candles

                    if (true) //switch from true / false - LIVE / TESTING
                    {
                        if (openPositions.Count == 0)
                        {
                            // check for LONG POSITION
                            var bullEntry = buEntries.LastOrDefault(it => !it.Invalidated);
                            //LONG CHECK
                            if (bullEntry != null
                                && lineData[d].LowPrice <= bullEntry.EntryPrice
                                && lineData[d].HighPrice >= bullEntry.EntryPrice
                                && inTradeSession)
                            {
                                if (true)//cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) > ((2 * ret0Candle.HighPrice + ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 5, 30, 0, 0, 0))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"LONG SIGNAL TAPPED! FVG: {bullEntry.InitialDate}");
                                    Console.ForegroundColor = ConsoleColor.White;

                                    var targets = new List<ProfitTarget>();
                                    bullEntry.Invalidated = true;

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = lineData[d].OpenTime,
                                        stopLimitPrice = bullEntry.StopLoss,
                                        type = "long",
                                        EntryPrice = bullEntry.EntryPrice.Value,
                                        Quantity = investment,
                                        targetPrice = bullEntry.TakeProfit.Value
                                    }) ;
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened LONG position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }

                        if (openPositions.Count == 0)
                        {
                            // check for SHORT POSITION
                            var bearEntry = beEntries.LastOrDefault(it => !it.Invalidated);
                            //SHORT CHECK
                            if (bearEntry != null
                                && lineData[d].HighPrice >= bearEntry.EntryPrice
                                && lineData[d].LowPrice <= bearEntry.EntryPrice
                                && inTradeSession)
                            {
                                if (true)//cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) > ((2 * ret0Candle.HighPrice + ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 5, 30, 0, 0, 0))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"SHORT SIGNAL TAPPED! FVG: {bearEntry.InitialDate}");
                                    Console.ForegroundColor = ConsoleColor.White;

                                    var targets = new List<ProfitTarget>();
                                    bearEntry.Invalidated = false;

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = lineData[d].OpenTime,
                                        stopLimitPrice = bearEntry.StopLoss,
                                        type = "short",
                                        EntryPrice = bearEntry.EntryPrice.Value,
                                        Quantity = investment,
                                        targetPrice = bearEntry.TakeProfit.Value
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened SHORT position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }

                        if (inSilverBulletSession && entriesInSilverBullet == 0)
                        {
                            var fvgStructure = quotes.Take(3).ToList();
                            int FVG = IsFairValueGap(fvgStructure);

                            if (FVG != 0 && fvgStructure[0].Date.AddHours(-4).Hour >= 10)
                            {
                                int rr = 2;
                                if (FVG == 1)
                                {
                                    entriesInSilverBullet++;
                                    //bullish entry
                                    var entry = new fvgFutureEntry();
                                    entry.EntryPrice = fvgStructure[2].Low;
                                    entry.StopLoss = Math.Min(fvgStructure[0].Low, fvgStructure[1].Low);
                                    entry.InitialDate = fvgStructure[0].Date.AddHours(-4);
                                    entry.Invalidated = false;
                                    entry.Type = "long";
                                    entry.TakeProfit = entry.EntryPrice + ((entry.EntryPrice - entry.StopLoss) * rr);
                                    buEntries.Add(entry);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"-- Long FVG Found! : {entry.InitialDate}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                if (FVG == 2)
                                {
                                    entriesInSilverBullet++;
                                    //bearish entry
                                    var entry = new fvgFutureEntry();
                                    entry.EntryPrice = fvgStructure[2].High;
                                    entry.StopLoss = Math.Max(fvgStructure[0].High, fvgStructure[1].High);
                                    entry.InitialDate = fvgStructure[0].Date.AddHours(-4);
                                    entry.Invalidated = false;
                                    entry.Type = "short";
                                    entry.TakeProfit = entry.EntryPrice - ((entry.StopLoss - entry.EntryPrice) * rr);
                                    beEntries.Add(entry);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"-- Short FVG Found! FVG: {entry.InitialDate}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                        }
                        //Console.WriteLine("-------------------------");
                    }
                }

                //Console.ReadLine();
            }
            var yty = positionHistories.GroupBy(it => it.hitTargets).ToList();

            Console.WriteLine($"TOTAL PROFIT SO FAR: {totalProfit}");
            Console.WriteLine($"Percent winrate: {1.0 * wins / (wins + losses) * 100}%");
            var gtt = 0;
        }

        private static int IsFairValueGap(List<Quote> quotes)
        {
            //0: No | 1: Bullish | 2: Bearish

            //check for bullish
            if (quotes[0].High < quotes[2].Low
                //&& quotes[1].Open <= quotes[0].High
                //&& quotes[1].Close >= quotes[2].Low
                )
            {
                return 1;
            }

            if (quotes[0].Low > quotes[2].High
                //&& quotes[1].Open >= quotes[0].Low
                //&& quotes[1].Close <= quotes[2].High
                )
            {
                return 2;
            }
            return 0;
        }

        private static BiasType? IsFairValueGap(List<IBinanceKline> quotes)
        {
            //check for bullish
            if (quotes[0].HighPrice < quotes[2].LowPrice
                //&& quotes[1].Open <= quotes[0].High
                //&& quotes[1].Close >= quotes[2].Low
                )
            {
                return BiasType.Bullish;
            }

            if (quotes[0].LowPrice > quotes[2].HighPrice
                //&& quotes[1].Open >= quotes[0].Low
                //&& quotes[1].Close <= quotes[2].High
                )
            {
                return BiasType.Bearish;
            }
            return null;
        }

        private static BiasType? IsMagnetFairValueGap(List<IBinanceKline> quotes)
        {
            //0: No | 1: Bullish | 2: Bearish

            //check for bullish
            if (quotes[0].HighPrice < quotes[2].LowPrice
                && quotes[2].ClosePrice <= quotes[1].HighPrice
                //&& quotes[1].Open <= quotes[0].High
                //&& quotes[1].Close >= quotes[2].Low
                )
            {
                return BiasType.Bullish;
            }

            if (quotes[0].LowPrice > quotes[2].HighPrice
                && quotes[2].ClosePrice >= quotes[1].LowPrice
                //&& quotes[1].Open >= quotes[0].Low
                //&& quotes[1].Close <= quotes[2].High
                )
            {
                return BiasType.Bearish;
            }
            return null;
        }

        private static BiasType? IsBreakawayGap(List<IBinanceKline> quotes)
        {
            //0: No | 1: Bullish | 2: Bearish

            //check for bullish
            if (quotes[0].HighPrice < quotes[2].LowPrice
                && quotes[2].ClosePrice > quotes[1].HighPrice
                //&& quotes[1].Open <= quotes[0].High
                //&& quotes[1].Close >= quotes[2].Low
                )
            {
                return BiasType.Bullish;
            }

            if (quotes[0].LowPrice > quotes[2].HighPrice
                && quotes[2].ClosePrice < quotes[1].LowPrice
                //&& quotes[1].Open >= quotes[0].Low
                //&& quotes[1].Close <= quotes[2].High
                )
            {
                return BiasType.Bearish;
            }
            return null;
        }

        private static void BACKTEST12TheOne_FibProfile(List<IBinanceKline> lineData, List<IBinanceKline> lineDataOneMin)
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();
            var resList = new List<VPRetracementEntry>();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneHour, DateTime.UtcNow.AddDays(-25), DateTime.UtcNow);//"BTCUSDT"
            //lineData = lineData.Skip(Math.Max(0, lineData.Count() - 400)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var positionHistories = new List<positionHistory>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var buEntries = new List<futureEntry>();
            var beEntries = new List<futureEntry>();
            int tresh = 1500;

            totalProfit = 3000;
            for (int d = tresh; d <= lineData.Count; d++)
            {
                Console.WriteLine($"{d} / {lineData.Count}");
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();                

                //TREBUIE MUTATA LOGICA IN AFARA CA SA SE POATA FACE UPDATE PE VALORI LA FIECARE TICK

                if (true)
                {
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    //take all candles for current calculation
                    lineSim = lineData.Take(d).ToList();

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim); //.Take(390).ToList()
                    var zigZagsInit = quotes.GetZigZag(EndType.HighLow, percentChange: 1m);
                    //var t = 0;

                    var pivotsInit = zigZagsInit.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                    //find all Highs and Lows that haven't been liquidated
                    List<ZigZagResult> validHighList = new List<ZigZagResult>();
                    List<ZigZagResult> validLowList = new List<ZigZagResult>();

                    foreach (var pivot in pivotsInit)
                    {
                        var forwardCandles = lineSim.Where(it => it.OpenTime >= pivot.Date);
                        if (pivot.PointType == "H")
                        {
                            if (pivot.RetraceHigh >= forwardCandles.Max(it => it.HighPrice))
                            {
                                validHighList.Add(pivot);
                            }
                        } else
                        {
                            if (pivot.RetraceLow <= forwardCandles.Min(it => it.LowPrice))
                            {
                                validLowList.Add(pivot);
                            }
                        }
                    }

                    //sa fie suma lor mica si sa fie si peak maricel intre ratios
                    //find possible reversals based on VP ratios low volume                
                    if (validHighList.Count > 0 || validLowList.Count > 0)
                    {
                        //integrate bull signals
                        int threshRemover = 50;
                        int pips = 75;
                        int rr = 3;
                        foreach (var pivot in validHighList)
                        {
                            //section
                            var section = lineSim.Where(it => it.OpenTime >= pivot.Date.AddMinutes(-1)).ToList();

                            if (section.Count() > 500)
                            {
                                //find VP of section until LIVE
                                var vp = volumeProfileCalculation(section, write: false);
                                IEnumerable<Quote> vpTimeSeries = ConvertVPToTimeSeries(vp);
                                var vpZigZag = vpTimeSeries.GetZigZag(EndType.Close, percentChange: 5m);
                                var vpPivots = vpZigZag.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                                var aggPivots = vpPivots.Select(it => Tuple.Create(it, (decimal)TimeSpan.FromTicks(it.Date.Ticks).TotalMinutes)).ToList();

                                decimal ret1 = pivot.RetraceHigh.Value;
                                decimal sectionMin = section.Min(it => it.LowPrice);
                                decimal ret0 = sectionMin;
                                decimal diff = ret1 - ret0;
                                decimal ret0236 = ret0 + diff * 0.236m;

                                decimal minSum = 100000000m;
                                decimal minRet0 = 0m;
                                decimal minRet1 = 0;

                                for (; ret0236 > sectionMin + 2; ret0 -= 1)
                                {
                                    ret0236 = ret0 + diff * 0.236m;
                                    decimal ret0382 = ret0 + diff * 0.382m;
                                    decimal ret05 = ret0 + diff * 0.5m;
                                    decimal ret0618 = ret0 + diff * 0.618m;
                                    decimal ret065 = ret0 + diff * 0.65m;
                                    decimal ret0705 = ret0 + diff * 0.705m;
                                    decimal ret0786 = ret0 + diff * 0.786m;
                                    decimal sum = 0;
                                    decimal maxVp = vp.Max(it => it.sum);
                                    int check1 = 0;

                                    var vp0236 = vp.First(it => it.interval.Item1 <= ret0236 && it.interval.Item2 >= ret0236).sum;
                                    var vp0382 = vp.First(it => it.interval.Item1 <= ret0382 && it.interval.Item2 >= ret0382).sum;
                                    var vp05 = vp.First(it => it.interval.Item1 <= ret05 && it.interval.Item2 >= ret05).sum;
                                    var vp0618 = vp.First(it => it.interval.Item1 <= ret0618 && it.interval.Item2 >= ret0618).sum;
                                    var vp065 = vp.First(it => it.interval.Item1 <= ret065 && it.interval.Item2 >= ret065).sum;
                                    var vp0705 = vp.First(it => it.interval.Item1 <= ret0705 && it.interval.Item2 >= ret0705).sum;
                                    var vp0786 = vp.First(it => it.interval.Item1 <= ret0786 && it.interval.Item2 >= ret0786).sum;
                                    var maxVPobj = vp.First(it => it.sum == vp.Max(x => x.sum));

                                    for (int ca = 0; ca < aggPivots.Count - 1; ca++)
                                    {
                                        if (aggPivots[ca].Item2 <= ret0236 && ret0236 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0236 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0236 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret0382 && ret0382 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0382 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0382 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret05 && ret05 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp05 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp05 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret0618 && ret0618 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0618 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0618 - aggPivots[ca + 1].Item2);
                                        }
                                        if (false && aggPivots[ca].Item2 <= ret065 && ret065 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp065 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp065 - aggPivots[ca + 1].Item2);
                                        }
                                        if (false && aggPivots[ca].Item2 <= ret0705 && ret0705 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0705 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0705 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret0786 && ret0786 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0786 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0786 - aggPivots[ca + 1].Item2);
                                        }

                                    }

                                    /*if (vp0236 < maxVp * 0.5m) check1++;
                                    if (vp0382 < maxVp * 0.5m) check1++;
                                    if (vp05 < maxVp * 0.5m) check1++;
                                    if (vp0618 < maxVp * 0.5m) check1++;
                                    if (vp065 < maxVp * 0.5m) check1++;
                                    if (vp0705 < maxVp * 0.5m) check1++;

                                    sum += vp0236;
                                    sum += vp0382;
                                    sum += vp05;
                                    sum += vp0618;
                                    sum += vp065;
                                    sum += vp0705;*/
                                    if (sum < minSum)// && check1 >= 2 && vp0705 <= maxVPobj.interval.Item1 && vp0618 >= maxVPobj.interval.Item2)
                                    {
                                        minSum = sum;
                                        minRet0 = ret0;
                                        minRet1 = ret1;
                                    }
                                }
                                if (minRet0 > 0 && !resList.Exists(it => it.Ret0 == minRet0 && it.Ret1 == minRet1))
                                {
                                    resList.RemoveAll(it => Math.Abs(it.Ret0 - minRet0) <= threshRemover);
                                    resList.Add(new VPRetracementEntry
                                    {
                                        Ret0 = minRet0
                                        ,
                                        Ret1 = minRet1
                                        ,
                                        FoundTime = section.Last().CloseTime
                                        ,
                                        Ret1Time = pivot.Date
                                        ,
                                        EntryPrice = minRet0 + 15
                                        ,
                                        StopLoss = minRet0 + 15 - pips
                                        ,
                                        TargetPrice = minRet0 + 15 + pips * rr
                                        ,
                                        Type = TradeType.Long
                                    }
                                    );
                                    var ggy = 0;
                                }
                            }
                        }

                        //integrate bear signals
                        foreach (var pivot in validLowList)
                        {
                            //section
                            var section = lineSim.Where(it => it.OpenTime >= pivot.Date.AddMinutes(-1)).ToList();

                            if (section.Count() > 500)
                            {
                                //find VP of section until LIVE
                                var vp = volumeProfileCalculation(section, write: false);
                                IEnumerable<Quote> vpTimeSeries = ConvertVPToTimeSeries(vp);
                                var vpZigZag = vpTimeSeries.GetZigZag(EndType.Close, percentChange: 5m);
                                var vpPivots = vpZigZag.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                                var aggPivots = vpPivots.Select(it => Tuple.Create(it, (decimal)TimeSpan.FromTicks(it.Date.Ticks).TotalMinutes)).ToList();
                                decimal ret1 = pivot.RetraceLow.Value;
                                decimal sectionMax = section.Max(it => it.HighPrice);
                                decimal ret0 = sectionMax;
                                decimal diff = ret0 - ret1;
                                decimal ret0236 = ret0 - diff * 0.236m;

                                decimal minSum = 100000000m;
                                decimal minRet0 = 0m;
                                decimal minRet1 = 0m;

                                for (; ret0236 < sectionMax - 2; ret0 += 1)
                                {
                                    ret0236 = ret0 - diff * 0.236m;
                                    decimal ret0382 = ret0 - diff * 0.382m;
                                    decimal ret05 = ret0 - diff * 0.5m;
                                    decimal ret0618 = ret0 - diff * 0.618m;
                                    decimal ret065 = ret0 - diff * 0.65m;
                                    decimal ret0705 = ret0 - diff * 0.705m;
                                    decimal ret0786 = ret0 - diff * 0.786m;
                                    decimal sum = 0;
                                    decimal maxVp = vp.Max(it => it.sum);
                                    int check1 = 0;

                                    var vp0236 = vp.First(it => it.interval.Item1 <= ret0236 && it.interval.Item2 >= ret0236).sum;
                                    var vp0382 = vp.First(it => it.interval.Item1 <= ret0382 && it.interval.Item2 >= ret0382).sum;
                                    var vp05 = vp.First(it => it.interval.Item1 <= ret05 && it.interval.Item2 >= ret05).sum;
                                    var vp0618 = vp.First(it => it.interval.Item1 <= ret0618 && it.interval.Item2 >= ret0618).sum;
                                    var vp065 = vp.First(it => it.interval.Item1 <= ret065 && it.interval.Item2 >= ret065).sum;
                                    var vp0705 = vp.First(it => it.interval.Item1 <= ret0705 && it.interval.Item2 >= ret0705).sum;
                                    var vp0786 = vp.First(it => it.interval.Item1 <= ret0786 && it.interval.Item2 >= ret0786).sum;
                                    var maxVPobj = vp.First(it => it.sum == vp.Max(x => x.sum));

                                    for (int ca = 0; ca < aggPivots.Count - 1; ca++)
                                    {
                                        if (aggPivots[ca].Item2 <= ret0236 && ret0236 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0236 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0236 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret0382 && ret0382 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0382 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0382 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret05 && ret05 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp05 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp05 - aggPivots[ca + 1].Item2);
                                        }
                                        if (aggPivots[ca].Item2 <= ret0618 && ret0618 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0618 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0618 - aggPivots[ca + 1].Item2);
                                        }
                                        if (false && aggPivots[ca].Item2 <= ret065 && ret065 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp065 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp065 - aggPivots[ca + 1].Item2);
                                        }
                                        if (false && aggPivots[ca].Item2 <= ret0705 && ret0705 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0705 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0705 - aggPivots[ca + 1].Item2);
                                        }
                                        if (false && aggPivots[ca].Item2 <= ret0786 && ret0786 <= aggPivots[ca + 1].Item2)
                                        {
                                            if (aggPivots[ca].Item1.PointType == "L") sum += Math.Abs(vp0786 - aggPivots[ca].Item2);
                                            if (aggPivots[ca + 1].Item1.PointType == "L") sum += Math.Abs(vp0786 - aggPivots[ca + 1].Item2);
                                        }
                                    }

                                    /*if (vp0236 < maxVp * 0.5m) check1++;
                                    if (vp0382 < maxVp * 0.5m) check1++;
                                    if (vp05 < maxVp * 0.5m) check1++;
                                    if (vp0618 < maxVp * 0.5m) check1++;
                                    if (vp065 < maxVp * 0.5m) check1++;
                                    if (vp0705 < maxVp * 0.5m) check1++;

                                    sum += vp0236;
                                    sum += vp0382;
                                    sum += vp05;
                                    sum += vp0618;
                                    sum += vp065;
                                    sum += vp0705;*/
                                    if (sum < minSum)// && check1 >= 2 && vp0705 >= maxVPobj.interval.Item2 && vp0618 <= maxVPobj.interval.Item1)
                                    {
                                        minSum = sum;
                                        minRet0 = ret0;
                                        minRet1 = ret1;
                                    }
                                }
                                if (minRet0 > 0 && !resList.Exists(it => it.Ret0 == minRet0 && it.Ret1 == minRet1))
                                {
                                    resList.RemoveAll(it => Math.Abs(it.Ret0 - minRet0) <= threshRemover);
                                    resList.Add(new VPRetracementEntry
                                    {
                                        Ret0 = minRet0
                                        ,
                                        Ret1 = minRet1
                                        ,
                                        FoundTime = section.Last().CloseTime
                                        ,
                                        Ret1Time = pivot.Date
                                        ,
                                        EntryPrice = minRet0 - 15
                                        ,
                                        StopLoss = minRet0 - 15 + pips
                                        ,
                                        TargetPrice = minRet0 - 15 - pips * rr
                                        ,
                                        Type = TradeType.Short
                                    }
                                    );
                                    var ggy = 0;
                                }
                            }
                        }
                    }

                    //INVALIDATE ANY TRADES
                    //Shorts
                    resList.RemoveAll(it => it.Ret1 >= lineSim.Last().LowPrice && it.Type == TradeType.Short);
                    //Longs
                    resList.RemoveAll(it => it.Ret1 <= lineSim.Last().HighPrice && it.Type == TradeType.Long);

                    //CHECK FOR POSITION / PROFIT TAKING / STOPLOSS
                    var lastQ = quotes.Last();
                    if (openPositions.Count > 0)
                    {
                        var openPos = openPositions.First();
                        if (openPos.type == "long")
                        {
                            // CHECK STOPLOSS
                            if (openPos.stopLimitPrice > lastQ.Low)
                            {
                                Console.WriteLine($"Current Profit: {totalProfit}");
                                var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice - 5, openPos.Quantity, 1, 1);
                                totalProfit += cp;

                                positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });

                                if (cp < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                                openPositions.Clear();
                                continue;
                            }
                            else
                            {
                                // CHECK PROFIT TARGETS
                                int cnt = 0;
                                foreach (var child in openPos.ProfitTargets)
                                {
                                    if (child.Alive && lastQ.High >= child.Price)
                                    {
                                        var cp = calculatePnL(openPos.EntryPrice, child.Price, child.Quantity, 1, 1);
                                        totalProfit += cp;
                                        child.Alive = false;
                                        openPos.Quantity -= child.Quantity;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Took PROFIT at {child.Price}, Quantity of {child.Quantity}. WIN of {cp}");
                                        if (true && cnt >= 2)
                                        {
                                            openPos.stopLimitPrice = openPos.ret0;// ProfitTargets[cnt - 2].Price - 25;
                                            Console.WriteLine($"Changed Position STOPLOSS to {openPos.stopLimitPrice}");
                                        }
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    cnt++;
                                }
                            }
                        }

                        if (openPos.type == "short")
                        {
                            // CHECK STOPLOSS
                            if (openPos.stopLimitPrice < lastQ.High)
                            {
                                Console.WriteLine($"Current Profit: {totalProfit}");
                                var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice + 5, openPos.Quantity, 1, -1);
                                totalProfit += cp;

                                positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                if (cp < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS:  {openPos.rsiMom} --- {openPos.rsiRet0}");
                                openPositions.Clear();
                                continue;
                            }
                            else
                            {
                                // CHECK PROFIT TARGETS
                                int cnt = 0;
                                foreach (var child in openPos.ProfitTargets)
                                {
                                    if (child.Alive && lastQ.Low <= child.Price)
                                    {
                                        var cp = calculatePnL(openPos.EntryPrice, child.Price, child.Quantity, 1, -1);
                                        totalProfit += cp;
                                        child.Alive = false;
                                        openPos.Quantity -= child.Quantity;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Took PROFIT at {child.Price}, Quantity of {child.Quantity}. WIN of {cp}");
                                        if (true && cnt >= 2)
                                        {
                                            openPos.stopLimitPrice = openPos.ret0;// ProfitTargets[cnt - 2].Price + 25;
                                            Console.WriteLine($"Changed Position STOPLOSS to {openPos.stopLimitPrice}");
                                        }
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    cnt++;
                                }
                            }
                        }

                        if (openPos.ProfitTargets.Count(it => it.Alive == true) == 0)
                        {
                            positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                            Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                            openPositions.Clear();
                        }
                    }
                    //------

                    if (true)
                    {
                        foreach (var child in resList)
                        {
                            if (child.Type == TradeType.Long && !child.Used)
                            {
                                if (lineSim.Last().LowPrice <= child.EntryPrice)
                                {
                                    Console.WriteLine($"LONG TRADE @ {lineSim.Last().CloseTime} | EntryPrice: {child.EntryPrice} | StopLoss: {child.StopLoss} | TargetPrice: {child.TargetPrice}");
                                    child.Used = true;
                                }
                            }

                            if (child.Type == TradeType.Short && !child.Used)
                            {
                                if (lineSim.Last().HighPrice >= child.EntryPrice)
                                {
                                    Console.WriteLine($"SHORT TRADE @ {lineSim.Last().CloseTime} | EntryPrice: {child.EntryPrice} | StopLoss: {child.StopLoss} | TargetPrice: {child.TargetPrice}");
                                    child.Used = true;
                                }
                            }
                        }
                    }

                    if (true) //switch from true / false - LIVE / TESTING
                    {
                        if (openPositions.Count == 0 || (openPositions.Count > 0 && openPositions.FirstOrDefault().type == "short"))
                        {
                            // check for LONG POSITION
                            var bullEntry = buEntries.LastOrDefault(it => !it.Invalidated);
                            //LONG CHECK
                            //if (bullEntry != null) Console.WriteLine($"Current Bull Entry: {bullEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");
                            if (bullEntry != null
                                && lineSim.Last().OpenPrice <= bullEntry.entryPrice
                                && lineSim.Last().ClosePrice >= bullEntry.entryPrice
                                //&& bias == "bullish"
                                //&& lineSim.Last().ClosePrice < (bullEntry.ret0236 + bullEntry.ret0382) / 2
                                //&& bullEntry.ret0236 - bullEntry.ret0 <= 1000
                                //&& (bullEntry.ret0 % 100 == 0 || bullEntry.ret1 % 100 == 0)                                
                                //&& bullEntry.rsiMom < bullEntry.rsiRet0
                                // && _smi.Last().Smi > _smi.Last().Signal
                                //&& _smi.Last().Smi > -40
                                //&& _smi.First(it => it.Date == bullEntry.InitialDate).Signal <= -20
                                //&& _hurst.Last().HurstExponent > 0.1
                                //&& (lineSim.Last().CloseTime - bullEntry.InitialDate).TotalMinutes <= 600
                                //&& (decimal)_atr.Last().Atr.Value * 1.5m <= (lineSim.Last().HighPrice - lineSim.Last().LowPrice)
                                //&& lineSim.Skip(352).Min(it => it.LowPrice) >= bullEntry.ret0
                                )
                            {
                                // VOLUME PROFILE FIBONACCI SYNC CHECK !!!
                                /*VP.Clear();
                                volumeProfileCalculation(lineDataOneMin.Where(it
                                                        => it.OpenTime >= bullEntry.Ret1Date.AddMinutes(-30) //new DateTime(2023, 5, 25, 1, 24, 0)
                                                        && it.OpenTime <= bullEntry.InitialDate//new DateTime(2023, 5, 29, 0, 24, 0)).ToList()
                                                        ).ToList(), true);*/

                                /*decimal avgVP = VP.Average(it => it.sum);
                                int cnt = 0;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0236 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0382 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret05 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0618 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret065 && it.interval.Item2 <= bullEntry.ret065).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0786 && it.interval.Item2 <= bullEntry.ret0786).sum <= avgVP) cnt++;


                                var ret0Candle = lineSim.First(it => it.CloseTime == bullEntry.InitialDate);*/
                                if (true)//cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) > ((2 * ret0Candle.HighPrice + ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 5, 30, 0, 0, 0))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"LONG SIGNAL! RET0 TIME: {bullEntry.InitialDate} | Top Fib @ {bullEntry.topFib} | Current Profit: {totalProfit}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (openPositions.Count > 0 && openPositions.FirstOrDefault().Quantity > 0)
                                    {
                                        //close previous position
                                        Console.WriteLine($"Sending request to CLOSE previous SHORT position ... Current candle time is: {lineSim.Last().OpenTime}");

                                        //calculate pnl of previous
                                        var cp = calculatePnL(openPositions.First().EntryPrice, bullEntry.entryPrice, openPositions.First().Quantity, 1, openPositions.First().type == "short" ? -1 : 1);
                                        totalProfit += cp;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Closed ongoing position. Took PROFIT at {bullEntry.entryPrice}, Quantity of {openPositions.First().Quantity}. WIN of {cp}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        positionHistories.Add(new positionHistory { posData = openPositions.First(), hitTargets = openPositions.First().ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                        Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPositions.First().rsiMom} --- {openPositions.First().rsiRet0}");
                                        openPositions.Clear();
                                    }

                                    var treshTargets = 25;
                                    var availableProfitTargets = 0;
                                    decimal? stopPrice = bullEntry.ret0236;
                                    if (true && bullEntry.ret1618 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0786; }
                                    if (true && bullEntry.ret1 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0618; }
                                    if (true && bullEntry.ret0786 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0618; }
                                    if (true && bullEntry.ret0618 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0382; }
                                    if (true && bullEntry.ret05 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0236; }
                                    if (true && bullEntry.ret0382 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0; }
                                    //stopPrice = bullEntry.ret0236;

                                    if (availableProfitTargets < 6)
                                    {
                                        var ttt = 0;
                                    }

                                    var targets = new List<ProfitTarget>();
                                    if (true && bullEntry.ret0382 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0382).Value - treshTargets, Quantity = investment / availableProfitTargets }); //REMOVE THESE TPS
                                    if (true && bullEntry.ret05 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret05).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret0618 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0618).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret0786 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0786).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret1 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret1).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret1618 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret1618).Value - treshTargets, Quantity = investment / availableProfitTargets });

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = quotes.Last().Date,
                                        stopLimitPrice = stopPrice - treshTargets,
                                        type = "long",
                                        EntryPrice = lineSim.Last().ClosePrice,
                                        ret0 = bullEntry.ret0,
                                        rsiRet0 = bullEntry.rsiRet0,
                                        rsiMom = bullEntry.rsiMom,
                                        Quantity = investment,
                                        ProfitTargets = targets.ToList()
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened LONG position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }

                        if (openPositions.Count == 0 || (openPositions.Count > 0 && openPositions.FirstOrDefault().type == "long"))
                        {
                            // check for SHORT POSITION
                            var bearEntry = beEntries.LastOrDefault(it => !it.Invalidated);
                            //SHORT CHECK
                            //if (bearEntry != null) Console.WriteLine($"Current Bear Entry: {bearEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");

                            if (bearEntry != null
                                && lineSim.Last().OpenPrice >= bearEntry.entryPrice
                                && lineSim.Last().ClosePrice <= bearEntry.entryPrice
                                //&& bias == "bearish"
                                //&& lineSim.Last().ClosePrice > (bearEntry.ret0236 + bearEntry.ret0382) / 2
                                //&& bearEntry.ret0 - bearEntry.ret0236 <= 1000
                                //&& (bearEntry.ret0 % 100 == 0 || bearEntry.ret1 % 100 == 0)
                                //&& bearEntry.rsiMom >= bearEntry.rsiRet0
                                //&& _smi.Last().Smi < _smi.Last().Signal
                                //&& _smi.Last().Smi < 40
                                //&& _smi.First(it => it.Date == bearEntry.InitialDate).Signal >= 20
                                //&& _hurst.Last().HurstExponent > 0.1
                                //&& (lineSim.Last().CloseTime - bearEntry.InitialDate).TotalMinutes <= 600
                                //&& (decimal)_atr.Last().Atr.Value * 1.5m <= (lineSim.Last().HighPrice - lineSim.Last().LowPrice)
                                //&& lineSim.Skip(352).Max(it => it.HighPrice) <= bearEntry.ret0
                                )
                            {
                                // VOLUME PROFILE FIBONACCI SYNC CHECK !!!
                                /*VP.Clear();
                                volumeProfileCalculation(lineDataOneMin.Where(it
                                                        => it.OpenTime >= bearEntry.Ret1Date.AddMinutes(-30) //new DateTime(2023, 5, 25, 1, 24, 0)
                                                        && it.OpenTime <= bearEntry.InitialDate//new DateTime(2023, 5, 29, 0, 24, 0)).ToList()
                                                        ).ToList(), true);*/

                                var ret0Candle = lineSim.First(it => it.CloseTime == bearEntry.InitialDate);

                                /*decimal avgVP = VP.Average(it => it.sum);
                                int cnt = 0;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0236 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0382 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret05 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0618 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret065 && it.interval.Item2 <= bearEntry.ret065).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0786 && it.interval.Item2 <= bearEntry.ret0786).sum <= avgVP) cnt++;*/

                                if (true)//cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) < ((ret0Candle.HighPrice + 2 * ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 6, 4))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"SHORT SIGNAL! RET0 TIME: {bearEntry.InitialDate} | Top Fib @ {bearEntry.topFib} | Current Profit: {totalProfit}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (openPositions.Count > 0 && openPositions.FirstOrDefault().Quantity > 0)
                                    {
                                        //close previous position
                                        Console.WriteLine($"Sending request to CLOSE previous LONG position ... Current candle time is: {lineSim.Last().OpenTime}");

                                        //calculate pnl of previous
                                        var cp = calculatePnL(openPositions.First().EntryPrice, bearEntry.entryPrice, openPositions.First().Quantity, 1, openPositions.First().type == "short" ? -1 : 1);
                                        totalProfit += cp;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Closed ongoing position. Took PROFIT at {bearEntry.entryPrice}, Quantity of {openPositions.First().Quantity}. WIN of {cp}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        positionHistories.Add(new positionHistory { posData = openPositions.First(), hitTargets = openPositions.First().ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                        Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPositions.First().rsiMom} --- {openPositions.First().rsiRet0}");
                                        openPositions.Clear();
                                    }

                                    var treshTargets = 25;
                                    var availableProfitTargets = 0;
                                    decimal? stopPrice = bearEntry.ret0236;

                                    if (true && bearEntry.ret1618 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0786; }
                                    if (true && bearEntry.ret1 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0618; }
                                    if (true && bearEntry.ret0786 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret05; }
                                    if (true && bearEntry.ret0618 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0382; }
                                    if (true && bearEntry.ret05 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0236; }
                                    if (true && bearEntry.ret0382 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0; }
                                    //stopPrice = bearEntry.ret0236;


                                    if (availableProfitTargets < 6)
                                    {
                                        var ttt = 0;
                                    }

                                    var targets = new List<ProfitTarget>();

                                    if (true && bearEntry.ret0382 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0382 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret05 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret05 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret0618 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0618 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret0786 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0786 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret1 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret1 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret1618 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret1618 + treshTargets).Value, Quantity = investment / availableProfitTargets });

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = quotes.Last().Date,
                                        stopLimitPrice = stopPrice + treshTargets,//bearEntry.ret0236 + 25,
                                        type = "short",
                                        EntryPrice = lineSim.Last().ClosePrice,
                                        ret0 = bearEntry.ret0,
                                        rsiRet0 = bearEntry.rsiRet0,
                                        rsiMom = bearEntry.rsiMom,
                                        Quantity = investment,
                                        ProfitTargets = targets.ToList()
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened SHORT position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }
                        //Console.WriteLine("-------------------------");
                    }
                }

                //Console.ReadLine();
            }
            var yty = positionHistories.GroupBy(it => it.hitTargets).ToList();

            Console.WriteLine($"TOTAL PROFIT SO FAR: {totalProfit}");
            var gtt = 0;
        }

        private static IEnumerable<Quote> ConvertVPToTimeSeries(List<VolumeProfile> vp)
        {
            var result = vp.Select(it => new Quote()
            {
                Close = it.sum,
                High = it.sum,
                Date = new DateTime() + TimeSpan.FromMinutes((int)it.interval.Item1),
                Low = it.sum,
                Open = it.sum,
                Volume = it.interval.Item1
            });
            return result;
        }

        private static void BACKTEST10TheOne(List<IBinanceKline> lineData, List<IBinanceKline> lineDataOneMin)
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneHour, DateTime.UtcNow.AddDays(-25), DateTime.UtcNow);//"BTCUSDT"
            //lineData = lineData.Skip(Math.Max(0, lineData.Count() - 400)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var positionHistories = new List<positionHistory>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var buEntries = new List<futureEntry>();
            var beEntries = new List<futureEntry>();
            int tresh = 400;

            totalProfit = 3000;
            for (int d = 0; d <= lineData.Count - tresh; d++)
            {
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();

                //TREBUIE MUTATA LOGICA IN AFARA CA SA SE POATA FACE UPDATE PE VALORI LA FIECARE TICK

                if (true)
                {
                    //investment = totalProfit.Value / 10000m;
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    //modify stop loss for open position
                    lineSim = lineData.Skip(d).Take(tresh).ToList();

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim); //.Take(390).ToList()

                    //BIAS CHECKING
                    var zigZagsInit = quotes.GetZigZag(EndType.HighLow, percentChange: 0.4m);
                    //var t = 0;

                    var pivotsInit = zigZagsInit.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                    var lastPivots = pivotsInit.Skip(Math.Max(0, pivotsInit.Count() - 10)).ToList();
                    var bias = "na";

                    for (int i = 2; i < lastPivots.Count(); i++)
                    {
                        if (lastPivots[i - 2].PointType == "L"
                            && lastPivots[i - 1].PointType == "H"
                            && lastPivots[i - 0].PointType == "L"
                            && lastPivots[i - 2].ZigZag > quotes.FirstOrDefault(it => it.Date == lastPivots[i].Date).Close)
                        {
                            bias = "bearish";
                        }
                        if (lastPivots[i - 2].PointType == "H"
                            && lastPivots[i - 1].PointType == "L"
                            && lastPivots[i - 0].PointType == "H"
                            && lastPivots[i - 2].ZigZag < quotes.FirstOrDefault(it => it.Date == lastPivots[i].Date).Close)
                        {
                            bias = "bullish";
                        }
                    }

                    //CHECK FOR POSITION / PROFIT TAKING / STOPLOSS
                    var lastQ = quotes.Last();
                    if (openPositions.Count > 0)
                    {
                        var openPos = openPositions.First();
                        if (openPos.type == "long")
                        {
                            // CHECK STOPLOSS
                            if (openPos.stopLimitPrice > lastQ.Low)
                            {
                                Console.WriteLine($"Current Profit: {totalProfit}");
                                var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice - 5, openPos.Quantity, 1, 1);
                                totalProfit += cp;

                                positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });

                                if (cp < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                } else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                                openPositions.Clear();
                                continue;
                            }
                            else
                            {
                                // CHECK PROFIT TARGETS
                                int cnt = 0;
                                foreach (var child in openPos.ProfitTargets)
                                {
                                    if (child.Alive && lastQ.High >= child.Price)
                                    {
                                        var cp = calculatePnL(openPos.EntryPrice, child.Price, child.Quantity, 1, 1);
                                        totalProfit += cp;
                                        child.Alive = false;
                                        openPos.Quantity -= child.Quantity;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Took PROFIT at {child.Price}, Quantity of {child.Quantity}. WIN of {cp}");
                                        if (true && cnt >= 2)
                                        {
                                            openPos.stopLimitPrice = openPos.ret0;// ProfitTargets[cnt - 2].Price - 25;
                                            Console.WriteLine($"Changed Position STOPLOSS to {openPos.stopLimitPrice}");
                                        }
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    cnt++;
                                }
                            }
                        }

                        if (openPos.type == "short")
                        {
                            // CHECK STOPLOSS
                            if (openPos.stopLimitPrice < lastQ.High)
                            {
                                Console.WriteLine($"Current Profit: {totalProfit}");
                                var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice + 5, openPos.Quantity, 1, -1);
                                totalProfit += cp;

                                positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                if (cp < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS:  {openPos.rsiMom} --- {openPos.rsiRet0}");
                                openPositions.Clear();
                                continue;
                            }
                            else
                            {
                                // CHECK PROFIT TARGETS
                                int cnt = 0;
                                foreach (var child in openPos.ProfitTargets)
                                {
                                    if (child.Alive && lastQ.Low <= child.Price)
                                    {
                                        var cp = calculatePnL(openPos.EntryPrice, child.Price, child.Quantity, 1, -1);
                                        totalProfit += cp;
                                        child.Alive = false;
                                        openPos.Quantity -= child.Quantity;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Took PROFIT at {child.Price}, Quantity of {child.Quantity}. WIN of {cp}");
                                        if (true && cnt >= 2)
                                        {
                                            openPos.stopLimitPrice = openPos.ret0;// ProfitTargets[cnt - 2].Price + 25;
                                            Console.WriteLine($"Changed Position STOPLOSS to {openPos.stopLimitPrice}");
                                        }
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    cnt++;
                                }
                            }
                        }

                        if (openPos.ProfitTargets.Count(it => it.Alive == true) == 0)
                        {
                            positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                            Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                            openPositions.Clear();
                        }
                    }
                        //------

                    if (quotes.Count() >= 80)
                    {
                        for (int q = quotes.Count(); q <= quotes.Count(); q++)
                        {
                            bool BullDFlag = false;
                            double rsiBullFlag = 0;
                            bool BearDFlag = false;
                            double rsiBearFlag = 0;
                            int MinPeriod = 4;
                            int Period = 150;

                            //quotes = quotes.Where(it => it.Date < new DateTime(2023, 05, 25, 11, 08, 00)).ToList();
                            var quotes1 = quotes.Take(q);

                            //var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 12, Deviation: 5, BackStep: 2);
                            var zigZags = quotes.GetZigZag(EndType.HighLow, percentChange: 0.4m);
                            //var t = 0;

                            var pivots = zigZags.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                            if (pivots.Count > 2)
                            {

                                var _rsi = quotes1.GetRsi(21);
                                var _momentum = quotes1.GetRoc(21);
                                var _cmo = quotes1.GetCmo(21);

                                for (int i = MinPeriod; i <= Period; i++)
                                {
                                    if (_momentum.Last().Roc > _momentum.Reverse().Skip(i).FirstOrDefault().Roc
                                        && _cmo.Last().Cmo > _cmo.Reverse().Skip(i).FirstOrDefault().Cmo
                                        && _rsi.Last().Rsi > _rsi.Reverse().Skip(i).FirstOrDefault().Rsi)
                                    {
                                        if (quotes1.Last().Close < quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                        {
                                            if (quotes1.Last().Low <= quotes1.Reverse().Take(i).Min(it => it.Low))
                                            {
                                                if (_rsi.Reverse().Take(i).Min(it => it.Rsi) <= 40.0)// && _rsi.Last().Rsi <= 33.0)
                                                {
                                                    BullDFlag = true;
                                                    rsiBullFlag = _rsi.Reverse().Take(i).Min(it => it.Rsi).Value;
                                                }
                                            }
                                        }
                                    }
                                    else if (_momentum.Last().Roc < _momentum.Reverse().Skip(i).FirstOrDefault().Roc
                                             && _cmo.Last().Cmo < _cmo.Reverse().Skip(i).FirstOrDefault().Cmo
                                             && _rsi.Last().Rsi < _rsi.Reverse().Skip(i).FirstOrDefault().Rsi)
                                    {
                                        if (quotes1.Last().Close > quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                        {
                                            if (quotes1.Last().High >= quotes1.Reverse().Take(i).Max(it => it.High))
                                            {
                                                if (_rsi.Reverse().Take(i).Max(it => it.Rsi) >= 60.0)// && _rsi.Last().Rsi >= 67.0)
                                                {
                                                    BearDFlag = true;
                                                    rsiBearFlag = _rsi.Reverse().Take(i).Max(it => it.Rsi).Value;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (BullDFlag)
                                {
                                    //Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    //Console.WriteLine($"BULLISH FLAG - {quotes1.Last().Date} - Posted @ {quotes1.Last().Date}");
                                    //Console.ForegroundColor = ConsoleColor.White;

                                    var highs = pivots.Where(it => it.PointType == "H" && it.Date <= quotes1.Last().Date).Reverse().ToList();

                                    int cnt = 0;
                                    while (cnt + 1 < highs.Count)
                                    {
                                        var theCurrLow = cnt == 0 ? quotes1.Last().Low : pivots[pivots.FindIndex(it => it.Date == highs[cnt].Date) + 1].ZigZag;
                                        var theNextLow = pivots[pivots.FindIndex(it => it.Date == highs[cnt + 1].Date) + 1].ZigZag;
                                        if (highs[cnt].ZigZag > highs[cnt + 1].ZigZag && theNextLow < theCurrLow)
                                        {
                                            break;
                                        }
                                        cnt++;
                                    }
                                    var theHighValue = highs.Take(cnt + 1).Max(it => it.ZigZag);
                                    //Console.WriteLine($"The TOP of the Fibonacci is @ {highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date}");

                                    if (buEntries.LastOrDefault(it => !it.Invalidated) == null || buEntries.Last(it => !it.Invalidated).ret0 > quotes1.Last().Low)
                                    {
                                        foreach (var child in buEntries) child.Invalidated = true; //Invalidate all previous Bull entries
                                        foreach (var child in beEntries) child.Invalidated = true; //Invalidate all previous Bear entries


                                        var bullEntry = new futureEntry(); //Create new Bullish entry
                                        bullEntry.InitialDate = quotes1.Last().Date;
                                        bullEntry.Ret1Date = highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date;
                                        bullEntry.ret0 = quotes1.Last().Low;
                                        bullEntry.ret1 = highs.FirstOrDefault(it => it.ZigZag == theHighValue).ZigZag.Value;
                                        decimal? diff = bullEntry.ret1 - bullEntry.ret0;
                                        bullEntry.ret0236 = bullEntry.ret0 + diff * 0.236m;
                                        bullEntry.ret0382 = bullEntry.ret0 + diff * 0.382m;
                                        bullEntry.ret05 = bullEntry.ret0 + diff * 0.5m;
                                        bullEntry.ret0618 = bullEntry.ret0 + diff * 0.618m;
                                        bullEntry.ret065 = bullEntry.ret0 + diff * 0.65m;
                                        bullEntry.ret0786 = bullEntry.ret0 + diff * 0.786m;
                                        bullEntry.ret1618 = bullEntry.ret0 + diff * 1.618m;
                                        bullEntry.rsiRet0 = (decimal)_rsi.Last().Rsi.Value;
                                        bullEntry.rsiMom = (decimal)rsiBullFlag;
                                        bullEntry.entryPrice = bullEntry.ret0236;// + ((bullEntry.ret0382 - bullEntry.ret0236) / 5); //20% towards the next retracement (0.382)
                                        bullEntry.Invalidated = false;
                                        bullEntry.topFib = highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date;
                                        buEntries.Add(bullEntry);
                                    }
                                }
                                if (BearDFlag)
                                {
                                    //Console.ForegroundColor = ConsoleColor.Red;
                                    //Console.WriteLine($"BEARISH FLAG - {quotes1.Last().Date} - Posted @ {quotes1.Last().Date}");
                                    //Console.ForegroundColor = ConsoleColor.White;

                                    var lows = pivots.Where(it => it.PointType == "L" && it.Date <= quotes1.Last().Date).Reverse().ToList();

                                    int cnt = 0;
                                    while (cnt + 1 < lows.Count)
                                    {
                                        var theCurrHigh = cnt == 0 ? quotes1.Last().High : pivots[pivots.FindIndex(it => it.Date == lows[cnt].Date) + 1].ZigZag;
                                        var theNextHigh = pivots[pivots.FindIndex(it => it.Date == lows[cnt + 1].Date) + 1].ZigZag;
                                        if (lows[cnt].ZigZag < lows[cnt + 1].ZigZag && theNextHigh > theCurrHigh)
                                        {
                                            break;
                                        }
                                        cnt++;
                                    }
                                    var theLowValue = lows.Take(cnt + 1).Min(it => it.ZigZag);
                                    //Console.WriteLine($"The BOTTOM of the Fibonacci is @ {lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date}");

                                    if (beEntries.LastOrDefault(it => !it.Invalidated) == null || beEntries.Last(it => !it.Invalidated).ret0 < quotes1.Last().High)
                                    {
                                        foreach (var child in buEntries) child.Invalidated = true; //Invalidate all previous Bull entries
                                        foreach (var child in beEntries) child.Invalidated = true; //Invalidate all previous Bear entries

                                        var bearEntry = new futureEntry(); //Create new Bearish entry
                                        bearEntry.InitialDate = quotes1.Last().Date;
                                        bearEntry.Ret1Date = lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date;
                                        bearEntry.ret0 = quotes1.Last().High;
                                        bearEntry.ret1 = lows.FirstOrDefault(it => it.ZigZag == theLowValue).ZigZag.Value;
                                        decimal? diff = bearEntry.ret0 - bearEntry.ret1;
                                        bearEntry.ret0236 = bearEntry.ret0 - diff * 0.236m;
                                        bearEntry.ret0382 = bearEntry.ret0 - diff * 0.382m;
                                        bearEntry.ret05 = bearEntry.ret0 - diff * 0.5m;
                                        bearEntry.ret0618 = bearEntry.ret0 - diff * 0.618m;
                                        bearEntry.ret065 = bearEntry.ret0 - diff * 0.65m;
                                        bearEntry.ret0786 = bearEntry.ret0 - diff * 0.786m;
                                        bearEntry.ret1618 = bearEntry.ret0 - diff * 1.618m;
                                        bearEntry.rsiRet0 = (decimal)_rsi.Last().Rsi.Value;
                                        bearEntry.rsiMom = (decimal)rsiBearFlag;
                                        bearEntry.entryPrice = bearEntry.ret0236;// - ((bearEntry.ret0236 - bearEntry.ret0382) / 5); //20% towards the next retracement (0.382)
                                        bearEntry.Invalidated = false;
                                        bearEntry.topFib = lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date;
                                        beEntries.Add(bearEntry);
                                    }
                                }
                            }
                        }
                        var gg = 0;

                        //Invalidate Bear if needed
                        if (beEntries.LastOrDefault(it => !it.Invalidated) != null)
                        {
                            var maxAfterEntry = quotes.Where(it => it.Date >= beEntries.Last(it2 => !it2.Invalidated).InitialDate).Max(it => it.High);
                            if (maxAfterEntry > beEntries.Last(it => !it.Invalidated).ret0)
                            {
                                beEntries.Last(it => !it.Invalidated).Invalidated = true;
                                //Console.WriteLine($"Invalidated latest bearish entry");
                            }
                        }

                        //Invalidate Bull if needed
                        if (buEntries.LastOrDefault(it => !it.Invalidated) != null)
                        {
                            var minAfterEntry = quotes.Where(it => it.Date >= buEntries.Last(it2 => !it2.Invalidated).InitialDate).Min(it => it.Low);
                            if (minAfterEntry < buEntries.Last(it => !it.Invalidated).ret0)
                            {
                                buEntries.Last(it => !it.Invalidated).Invalidated = true;
                                //Console.WriteLine($"Invalidated latest bullish entry");
                            }
                        }

                        //Console.WriteLine("-------------------------------------------------");
                        buEntries.RemoveAll(it => it.Invalidated);
                        beEntries.RemoveAll(it => it.Invalidated);
                        //foreach (var child in buEntries)
                        //{
                        //    Console.WriteLine($"BULLISH POSSIBLE ENTRY @ - {child.InitialDate} - BETWEEN {child.ret0} AND {child.ret1}");
                        //}
                        //foreach (var child in beEntries)
                        //{
                        //    Console.WriteLine($"BEARISH POSSIBLE ENTRY @ - {child.InitialDate} - BETWEEN {child.ret0} AND {child.ret1}");
                        //}
                        //if (buEntries.Count == 0 && beEntries.Count == 0)
                        //{
                        //    Console.WriteLine("CURRENTLY - NO ENTRIES");
                        //}
                    }

                    var _smi = quotes.GetSmi(13, 25, 2, 12);
                    var _hurst = quotes.GetHurst(100);
                    var _atr = quotes.GetAtr(14);
                    var _macd = quotes.GetMacd(12, 26, 9);
                    if (quotes.Last().Date >= new DateTime(2023, 06, 15, 18, 0, 0))
                    {
                        var htt = 0;
                    }

                    foreach (var bu in buEntries)
                    {
                        var section = lineSim.Where(it => it.CloseTime >= bu.InitialDate).ToList();
                        var FVG = FindFirstMagnetFVG(section, BiasType.Bullish);

                        if (FVG.Count > 0 && IsMagnetFairValueGap(FVG) == BiasType.Bullish)
                        {
                            bu.FVGEntryPrice = FVG[2].LowPrice;
                            bu.FVGStopLoss = FVG[0].LowPrice;
                            bu.FVGStopLossTime = FVG[0].OpenTime;
                        }
                    }

                    foreach (var be in beEntries)
                    {
                        var section = lineSim.Where(it => it.CloseTime >= be.InitialDate).ToList();
                        var FVG = FindFirstMagnetFVG(section, BiasType.Bearish);

                        if (FVG.Count > 0 && IsMagnetFairValueGap(FVG) == BiasType.Bearish)
                        {
                            be.FVGEntryPrice = FVG[2].HighPrice;
                            be.FVGStopLoss = FVG[0].HighPrice;
                            be.FVGStopLossTime = FVG[0].OpenTime;
                        }
                    }

                    if (true) //switch from true / false - LIVE / TESTING
                    {
                        if (openPositions.Count == 0 || (openPositions.Count > 0 && openPositions.FirstOrDefault().type == "short"))
                        {
                            // check for LONG POSITION
                            var bullEntry = buEntries.LastOrDefault(it => !it.Invalidated);
                            //LONG CHECK

                            //if (bullEntry != null) Console.WriteLine($"Current Bull Entry: {bullEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");
                            if (bullEntry != null
                                && bullEntry.FVGEntryPrice != null 
                                && lineSim.Last().LowPrice <= bullEntry.FVGEntryPrice
                                && lineSim.Last().HighPrice >= bullEntry.FVGEntryPrice
                                //&& bias == "bullish"
                                //&& lineSim.Last().ClosePrice < (bullEntry.ret0236 + bullEntry.ret0382) / 2
                                //&& bullEntry.ret0236 - bullEntry.ret0 <= 1000
                                //&& (bullEntry.ret0 % 100 == 0 || bullEntry.ret1 % 100 == 0)                                
                                //&& bullEntry.rsiMom < bullEntry.rsiRet0
                                // && _smi.Last().Smi > _smi.Last().Signal
                                //&& _smi.Last().Smi > -40
                                //&& _smi.First(it => it.Date == bullEntry.InitialDate).Signal <= -20
                                //&& _hurst.Last().HurstExponent > 0.1
                                //&& (lineSim.Last().CloseTime - bullEntry.InitialDate).TotalMinutes <= 600
                                //&& (decimal)_atr.Last().Atr.Value * 1.5m <= (lineSim.Last().HighPrice - lineSim.Last().LowPrice)
                                //&& lineSim.Skip(352).Min(it => it.LowPrice) >= bullEntry.ret0
                                )
                            {
                                // VOLUME PROFILE FIBONACCI SYNC CHECK !!!
                                /*VP.Clear();
                                volumeProfileCalculation(lineDataOneMin.Where(it
                                                        => it.OpenTime >= bullEntry.Ret1Date.AddMinutes(-30) //new DateTime(2023, 5, 25, 1, 24, 0)
                                                        && it.OpenTime <= bullEntry.InitialDate//new DateTime(2023, 5, 29, 0, 24, 0)).ToList()
                                                        ).ToList(), true);*/

                                /*decimal avgVP = VP.Average(it => it.sum);
                                int cnt = 0;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0236 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0382 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret05 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0618 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret065 && it.interval.Item2 <= bullEntry.ret065).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0786 && it.interval.Item2 <= bullEntry.ret0786).sum <= avgVP) cnt++;


                                var ret0Candle = lineSim.First(it => it.CloseTime == bullEntry.InitialDate);*/

                                if (true)//cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) > ((2 * ret0Candle.HighPrice + ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 5, 30, 0, 0, 0))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"LONG SIGNAL! RET0 TIME: {bullEntry.InitialDate} | Top Fib @ {bullEntry.topFib} | FVG stopLossTime: {bullEntry.FVGStopLossTime} | Current Profit: {totalProfit}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (openPositions.Count > 0 && openPositions.FirstOrDefault().Quantity > 0)
                                    {
                                        //close previous position
                                        Console.WriteLine($"Sending request to CLOSE previous SHORT position ... Current candle time is: {lineSim.Last().OpenTime}");

                                        //calculate pnl of previous
                                        var cp = calculatePnL(openPositions.First().EntryPrice, bullEntry.entryPrice, openPositions.First().Quantity, 1, openPositions.First().type == "short" ? -1 : 1);
                                        totalProfit += cp;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Closed ongoing position. Took PROFIT at {bullEntry.entryPrice}, Quantity of {openPositions.First().Quantity}. WIN of {cp}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        positionHistories.Add(new positionHistory { posData = openPositions.First(), hitTargets = openPositions.First().ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                        Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPositions.First().rsiMom} --- {openPositions.First().rsiRet0}");
                                        openPositions.Clear();
                                    }

                                    var treshTargets = 25;
                                    var availableProfitTargets = 0;
                                    decimal? stopPrice = bullEntry.ret0236;
                                    if (true && bullEntry.ret1618 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0786; }
                                    if (true && bullEntry.ret1 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0618; }
                                    if (true && bullEntry.ret0786 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0618; }
                                    if (true && bullEntry.ret0618 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0382; }
                                    if (true && bullEntry.ret05 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0236; }
                                    if (true && bullEntry.ret0382 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0; }
                                    //stopPrice = bullEntry.ret0236;

                                    if (availableProfitTargets < 6)
                                    {
                                        var ttt = 0;
                                    }

                                    var targets = new List<ProfitTarget>();
                                    if (true && bullEntry.ret0382 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0382).Value - treshTargets, Quantity = investment / availableProfitTargets }); //REMOVE THESE TPS
                                    if (true && bullEntry.ret05 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret05).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret0618 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0618).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret0786 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0786).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret1 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret1).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret1618 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret1618).Value - treshTargets, Quantity = investment / availableProfitTargets });

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = quotes.Last().Date,
                                        stopLimitPrice = bullEntry.FVGStopLoss.Value,
                                        type = "long",
                                        EntryPrice = bullEntry.FVGEntryPrice.Value,
                                        ret0 = bullEntry.ret0,
                                        rsiRet0 = bullEntry.rsiRet0,
                                        rsiMom = bullEntry.rsiMom,
                                        Quantity = investment,
                                        ProfitTargets = targets.ToList()
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened LONG position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                } else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }

                        if (openPositions.Count == 0 || (openPositions.Count > 0 && openPositions.FirstOrDefault().type == "long"))
                        {
                            // check for SHORT POSITION
                            var bearEntry = beEntries.LastOrDefault(it => !it.Invalidated);
                            //SHORT CHECK
                            //if (bearEntry != null) Console.WriteLine($"Current Bear Entry: {bearEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");

                            if (bearEntry != null
                                && bearEntry.FVGEntryPrice != null
                                && lineSim.Last().HighPrice >= bearEntry.FVGEntryPrice
                                && lineSim.Last().LowPrice <= bearEntry.FVGEntryPrice
                                //&& bias == "bearish"
                                //&& lineSim.Last().ClosePrice > (bearEntry.ret0236 + bearEntry.ret0382) / 2
                                //&& bearEntry.ret0 - bearEntry.ret0236 <= 1000
                                //&& (bearEntry.ret0 % 100 == 0 || bearEntry.ret1 % 100 == 0)
                                //&& bearEntry.rsiMom >= bearEntry.rsiRet0
                                //&& _smi.Last().Smi < _smi.Last().Signal
                                //&& _smi.Last().Smi < 40
                                //&& _smi.First(it => it.Date == bearEntry.InitialDate).Signal >= 20
                                //&& _hurst.Last().HurstExponent > 0.1
                                //&& (lineSim.Last().CloseTime - bearEntry.InitialDate).TotalMinutes <= 600
                                //&& (decimal)_atr.Last().Atr.Value * 1.5m <= (lineSim.Last().HighPrice - lineSim.Last().LowPrice)
                                //&& lineSim.Skip(352).Max(it => it.HighPrice) <= bearEntry.ret0
                                )
                            {
                                // VOLUME PROFILE FIBONACCI SYNC CHECK !!!
                                /*VP.Clear();
                                volumeProfileCalculation(lineDataOneMin.Where(it
                                                        => it.OpenTime >= bearEntry.Ret1Date.AddMinutes(-30) //new DateTime(2023, 5, 25, 1, 24, 0)
                                                        && it.OpenTime <= bearEntry.InitialDate//new DateTime(2023, 5, 29, 0, 24, 0)).ToList()
                                                        ).ToList(), true);*/

                                var ret0Candle = lineSim.First(it => it.CloseTime == bearEntry.InitialDate);

                                /*decimal avgVP = VP.Average(it => it.sum);
                                int cnt = 0;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0236 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0382 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret05 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0618 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret065 && it.interval.Item2 <= bearEntry.ret065).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0786 && it.interval.Item2 <= bearEntry.ret0786).sum <= avgVP) cnt++;*/

                                if (true)//cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) < ((ret0Candle.HighPrice + 2 * ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 6, 4))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"SHORT SIGNAL! RET0 TIME: {bearEntry.InitialDate} | Top Fib @ {bearEntry.topFib} | FVG stopLossTime: {bearEntry.FVGStopLossTime} | Current Profit: {totalProfit}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (openPositions.Count > 0 && openPositions.FirstOrDefault().Quantity > 0)
                                    {
                                        //close previous position
                                        Console.WriteLine($"Sending request to CLOSE previous LONG position ... Current candle time is: {lineSim.Last().OpenTime}");

                                        //calculate pnl of previous
                                        var cp = calculatePnL(openPositions.First().EntryPrice, bearEntry.entryPrice, openPositions.First().Quantity, 1, openPositions.First().type == "short" ? -1 : 1);
                                        totalProfit += cp;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Closed ongoing position. Took PROFIT at {bearEntry.entryPrice}, Quantity of {openPositions.First().Quantity}. WIN of {cp}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        positionHistories.Add(new positionHistory { posData = openPositions.First(), hitTargets = openPositions.First().ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                        Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPositions.First().rsiMom} --- {openPositions.First().rsiRet0}");
                                        openPositions.Clear();
                                    }

                                    var treshTargets = 25;
                                    var availableProfitTargets = 0;
                                    decimal? stopPrice = bearEntry.ret0236;

                                    if (true && bearEntry.ret1618 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0786; }
                                    if (true && bearEntry.ret1 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0618; }
                                    if (true && bearEntry.ret0786 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret05; }
                                    if (true && bearEntry.ret0618 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0382; }
                                    if (true && bearEntry.ret05 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0236; }
                                    if (true && bearEntry.ret0382 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0; }
                                    //stopPrice = bearEntry.ret0236;


                                    if (availableProfitTargets < 6)
                                    {
                                        var ttt = 0;
                                    }

                                    var targets = new List<ProfitTarget>();

                                    if (true && bearEntry.ret0382 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0382 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret05 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret05 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret0618 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0618 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret0786 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0786 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret1 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret1 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret1618 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret1618 + treshTargets).Value, Quantity = investment / availableProfitTargets });

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = quotes.Last().Date,
                                        stopLimitPrice = bearEntry.FVGStopLoss.Value,//bearEntry.ret0236 + 25,
                                        type = "short",
                                        EntryPrice = bearEntry.FVGEntryPrice.Value,
                                        ret0 = bearEntry.ret0,
                                        rsiRet0 = bearEntry.rsiRet0,
                                        rsiMom = bearEntry.rsiMom,
                                        Quantity = investment,
                                        ProfitTargets = targets.ToList()
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened SHORT position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }
                        //Console.WriteLine("-------------------------");
                    }
                }

                //Console.ReadLine();
            }
            var yty = positionHistories.GroupBy(it => it.hitTargets).ToList();

            Console.WriteLine($"TOTAL PROFIT SO FAR: {totalProfit}");
            var gtt = 0;
        }

        private static List<IBinanceKline> FindFirstMagnetFVG(List<IBinanceKline> section, BiasType biasType)
        {
            var candleGroup = section.Take(3).ToList();
            for (int i = 3; i < section.Count; i++)
            {
                var decision = IsMagnetFairValueGap(candleGroup);

                if (decision == biasType)
                {
                    return candleGroup;
                }

                //prepare for the next one
                candleGroup.Add(section[i]);
                candleGroup.RemoveAt(0);
            }
            return new List<IBinanceKline>();
        }

        private static void BACKTEST11TheOneSMC(List<IBinanceKline> lineData4H, List<IBinanceKline> lineData15M)
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneHour, DateTime.UtcNow.AddDays(-25), DateTime.UtcNow);//"BTCUSDT"
            //lineData = lineData.Skip(Math.Max(0, lineData.Count() - 400)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var positionHistories = new List<positionHistory>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var buEntries = new List<futureEntry>();
            var beEntries = new List<futureEntry>();
            int tresh = 400;

            totalProfit = 3000;
            for (int d = 0; d <= lineData4H.Count - tresh; d++)
            {
                var lowPivots = new List<decimal?>();
                var highPivots = new List<decimal?>();

                //TREBUIE MUTATA LOGICA IN AFARA CA SA SE POATA FACE UPDATE PE VALORI LA FIECARE TICK

                if (true)
                {
                    investment = totalProfit.Value / 10000m;
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    //modify stop loss for open position
                    lineSim = lineData4H.Skip(d).ToList(); //.Take(tresh)

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim); //.Take(390).ToList()

                    //CHECK FOR POSITION / PROFIT TAKING / STOPLOSS
                    var lastQ = quotes.Last();
                    if (openPositions.Count > 0)
                    {
                        var openPos = openPositions.First();
                        if (openPos.type == "long")
                        {
                            // CHECK STOPLOSS
                            if (openPos.stopLimitPrice > lastQ.Low)
                            {
                                Console.WriteLine($"Current Profit: {totalProfit}");
                                var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice - 5, openPos.Quantity, 1, 1);
                                totalProfit += cp;

                                positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });

                                if (cp < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                                openPositions.Clear();
                                continue;
                            }
                            else
                            {
                                // CHECK PROFIT TARGETS
                                int cnt = 0;
                                foreach (var child in openPos.ProfitTargets)
                                {
                                    if (child.Alive && lastQ.High >= child.Price)
                                    {
                                        var cp = calculatePnL(openPos.EntryPrice, child.Price, child.Quantity, 1, 1);
                                        totalProfit += cp;
                                        child.Alive = false;
                                        openPos.Quantity -= child.Quantity;                                        

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Took PROFIT at {child.Price}, Quantity of {child.Quantity}. WIN of {cp}");
                                        if (true && cnt >= 2)
                                        {
                                            openPos.stopLimitPrice = openPos.ret0;// ProfitTargets[cnt - 2].Price - 25;
                                            Console.WriteLine($"Changed Position STOPLOSS to {openPos.stopLimitPrice}");
                                        }
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    cnt++;
                                }
                            }
                        }

                        if (openPos.type == "short")
                        {
                            // CHECK STOPLOSS
                            if (openPos.stopLimitPrice < lastQ.High)
                            {
                                Console.WriteLine($"Current Profit: {totalProfit}");
                                var cp = calculatePnL(openPos.EntryPrice, openPos.stopLimitPrice + 5, openPos.Quantity, 1, -1);
                                totalProfit += cp;

                                positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                if (cp < 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Got STOPPED. LOSS of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;   
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"Got STOPPED. WIN of {cp}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THIS LOSS WAS:  {openPos.rsiMom} --- {openPos.rsiRet0}");
                                openPositions.Clear();
                                continue;
                            }
                            else
                            {
                                // CHECK PROFIT TARGETS
                                int cnt = 0;
                                foreach (var child in openPos.ProfitTargets)
                                {
                                    if (child.Alive && lastQ.Low <= child.Price)
                                    {
                                        var cp = calculatePnL(openPos.EntryPrice, child.Price, child.Quantity, 1, -1);
                                        totalProfit += cp;
                                        child.Alive = false;
                                        openPos.Quantity -= child.Quantity;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Took PROFIT at {child.Price}, Quantity of {child.Quantity}. WIN of {cp}");
                                        if (true && cnt >= 2)
                                        {
                                            openPos.stopLimitPrice = openPos.ret0;// ProfitTargets[cnt - 2].Price + 25;
                                            Console.WriteLine($"Changed Position STOPLOSS to {openPos.stopLimitPrice}");
                                        }
                                        Console.ForegroundColor = ConsoleColor.White;
                                    }
                                    cnt++;
                                }
                            }
                        }

                        if (openPos.ProfitTargets.Count(it => it.Alive == true) == 0)
                        {
                            positionHistories.Add(new positionHistory { posData = openPos, hitTargets = openPos.ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                            Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPos.rsiMom} --- {openPos.rsiRet0}");
                            openPositions.Clear();
                        }
                    }
                    //------

                    if (quotes.Count() >= 90)
                    {
                        var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 12, Deviation: 5, BackStep: 2);
                        var pivots = zigZags.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                        BiasType bias = 0; //1 - bullish, 2 - bearish
                        if (pivots.First().PointType == "H" && pivots.Skip(1).First().PointType == "L") bias = BiasType.Bearish;
                        else bias = BiasType.Bullish;

                        var marketStructure4H = GetMarketStructure(lineSim, bias, pivots.First(), pivots.Skip(1).First());
                    }

                    if (false && quotes.Count() >= 80)
                    {
                        for (int q = quotes.Count(); q <= quotes.Count(); q++)
                        {
                            bool BullDFlag = false;
                            double rsiBullFlag = 0;
                            bool BearDFlag = false;
                            double rsiBearFlag = 0;
                            int MinPeriod = 4;
                            int Period = 150;

                            //quotes = quotes.Where(it => it.Date < new DateTime(2023, 05, 25, 11, 08, 00)).ToList();
                            var quotes1 = quotes.Take(q);

                            var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 12, Deviation: 5, BackStep: 2);
                            //var t = 0;

                            var pivots = zigZags.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                            var _rsi = quotes1.GetRsi(21);
                            var _momentum = quotes1.GetRoc(21);
                            var _cmo = quotes1.GetCmo(21);

                            for (int i = MinPeriod; i <= Period; i++)
                            {
                                if (_momentum.Last().Roc > _momentum.Reverse().Skip(i).FirstOrDefault().Roc
                                    && _cmo.Last().Cmo > _cmo.Reverse().Skip(i).FirstOrDefault().Cmo
                                    && _rsi.Last().Rsi > _rsi.Reverse().Skip(i).FirstOrDefault().Rsi)
                                {
                                    if (quotes1.Last().Close < quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                    {
                                        if (quotes1.Last().Low <= quotes1.Reverse().Take(i).Min(it => it.Low))
                                        {
                                            if (_rsi.Reverse().Take(i).Min(it => it.Rsi) <= 40.0)// && _rsi.Last().Rsi <= 33.0)
                                            {
                                                BullDFlag = true;
                                                rsiBullFlag = _rsi.Reverse().Take(i).Min(it => it.Rsi).Value;
                                            }
                                        }
                                    }
                                }
                                else if (_momentum.Last().Roc < _momentum.Reverse().Skip(i).FirstOrDefault().Roc
                                         && _cmo.Last().Cmo < _cmo.Reverse().Skip(i).FirstOrDefault().Cmo
                                         && _rsi.Last().Rsi < _rsi.Reverse().Skip(i).FirstOrDefault().Rsi)
                                {
                                    if (quotes1.Last().Close > quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                    {
                                        if (quotes1.Last().High >= quotes1.Reverse().Take(i).Max(it => it.High))
                                        {
                                            if (_rsi.Reverse().Take(i).Max(it => it.Rsi) >= 60.0)// && _rsi.Last().Rsi >= 67.0)
                                            {
                                                BearDFlag = true;
                                                rsiBearFlag = _rsi.Reverse().Take(i).Max(it => it.Rsi).Value;
                                            }
                                        }
                                    }
                                }
                            }
                            if (BullDFlag)
                            {
                                //Console.ForegroundColor = ConsoleColor.DarkGreen;
                                //Console.WriteLine($"BULLISH FLAG - {quotes1.Last().Date} - Posted @ {quotes1.Last().Date}");
                                //Console.ForegroundColor = ConsoleColor.White;

                                var highs = pivots.Where(it => it.PointType == "H" && it.Date <= quotes1.Last().Date).Reverse().ToList();

                                int cnt = 0;
                                while (cnt + 1 < highs.Count)
                                {
                                    var theCurrLow = cnt == 0 ? quotes1.Last().Low : pivots[pivots.FindIndex(it => it.Date == highs[cnt].Date) + 1].ZigZag;
                                    var theNextLow = pivots[pivots.FindIndex(it => it.Date == highs[cnt + 1].Date) + 1].ZigZag;
                                    if (highs[cnt].ZigZag > highs[cnt + 1].ZigZag && theNextLow < theCurrLow)
                                    {
                                        break;
                                    }
                                    cnt++;
                                }
                                var theHighValue = highs.Take(cnt + 1).Max(it => it.ZigZag);
                                //Console.WriteLine($"The TOP of the Fibonacci is @ {highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date}");

                                if (buEntries.LastOrDefault(it => !it.Invalidated) == null || buEntries.Last(it => !it.Invalidated).ret0 > quotes1.Last().Low)
                                {
                                    foreach (var child in buEntries) child.Invalidated = true; //Invalidate all previous Bull entries
                                    foreach (var child in beEntries) child.Invalidated = true; //Invalidate all previous Bear entries


                                    var bullEntry = new futureEntry(); //Create new Bullish entry
                                    bullEntry.InitialDate = quotes1.Last().Date;
                                    bullEntry.Ret1Date = highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date;
                                    bullEntry.ret0 = quotes1.Last().Low;
                                    bullEntry.ret1 = highs.FirstOrDefault(it => it.ZigZag == theHighValue).ZigZag.Value;
                                    decimal? diff = bullEntry.ret1 - bullEntry.ret0;
                                    bullEntry.ret0236 = bullEntry.ret0 + diff * 0.236m;
                                    bullEntry.ret0382 = bullEntry.ret0 + diff * 0.382m;
                                    bullEntry.ret05 = bullEntry.ret0 + diff * 0.5m;
                                    bullEntry.ret0618 = bullEntry.ret0 + diff * 0.618m;
                                    bullEntry.ret065 = bullEntry.ret0 + diff * 0.65m;
                                    bullEntry.ret0786 = bullEntry.ret0 + diff * 0.786m;
                                    bullEntry.ret1618 = bullEntry.ret0 + diff * 1.618m;
                                    bullEntry.rsiRet0 = (decimal)_rsi.Last().Rsi.Value;
                                    bullEntry.rsiMom = (decimal)rsiBullFlag;
                                    bullEntry.entryPrice = bullEntry.ret0236;// + ((bullEntry.ret0382 - bullEntry.ret0236) / 5); //20% towards the next retracement (0.382)
                                    bullEntry.Invalidated = false;
                                    bullEntry.topFib = highs.FirstOrDefault(it => it.ZigZag == theHighValue).Date;
                                    buEntries.Add(bullEntry);
                                }
                            }
                            if (BearDFlag)
                            {
                                //Console.ForegroundColor = ConsoleColor.Red;
                                //Console.WriteLine($"BEARISH FLAG - {quotes1.Last().Date} - Posted @ {quotes1.Last().Date}");
                                //Console.ForegroundColor = ConsoleColor.White;

                                var lows = pivots.Where(it => it.PointType == "L" && it.Date <= quotes1.Last().Date).Reverse().ToList();

                                int cnt = 0;
                                while (cnt + 1 < lows.Count)
                                {
                                    var theCurrHigh = cnt == 0 ? quotes1.Last().High : pivots[pivots.FindIndex(it => it.Date == lows[cnt].Date) + 1].ZigZag;
                                    var theNextHigh = pivots[pivots.FindIndex(it => it.Date == lows[cnt + 1].Date) + 1].ZigZag;
                                    if (lows[cnt].ZigZag < lows[cnt + 1].ZigZag && theNextHigh > theCurrHigh)
                                    {
                                        break;
                                    }
                                    cnt++;
                                }
                                var theLowValue = lows.Take(cnt + 1).Min(it => it.ZigZag);
                                //Console.WriteLine($"The BOTTOM of the Fibonacci is @ {lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date}");

                                if (beEntries.LastOrDefault(it => !it.Invalidated) == null || beEntries.Last(it => !it.Invalidated).ret0 < quotes1.Last().High)
                                {
                                    foreach (var child in buEntries) child.Invalidated = true; //Invalidate all previous Bull entries
                                    foreach (var child in beEntries) child.Invalidated = true; //Invalidate all previous Bear entries

                                    var bearEntry = new futureEntry(); //Create new Bearish entry
                                    bearEntry.InitialDate = quotes1.Last().Date;
                                    bearEntry.Ret1Date = lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date;
                                    bearEntry.ret0 = quotes1.Last().High;
                                    bearEntry.ret1 = lows.FirstOrDefault(it => it.ZigZag == theLowValue).ZigZag.Value;
                                    decimal? diff = bearEntry.ret0 - bearEntry.ret1;
                                    bearEntry.ret0236 = bearEntry.ret0 - diff * 0.236m;
                                    bearEntry.ret0382 = bearEntry.ret0 - diff * 0.382m;
                                    bearEntry.ret05 = bearEntry.ret0 - diff * 0.5m;
                                    bearEntry.ret0618 = bearEntry.ret0 - diff * 0.618m;
                                    bearEntry.ret065 = bearEntry.ret0 - diff * 0.65m;
                                    bearEntry.ret0786 = bearEntry.ret0 - diff * 0.786m;
                                    bearEntry.ret1618 = bearEntry.ret0 - diff * 1.618m;
                                    bearEntry.rsiRet0 = (decimal)_rsi.Last().Rsi.Value;
                                    bearEntry.rsiMom = (decimal)rsiBearFlag;
                                    bearEntry.entryPrice = bearEntry.ret0236;// - ((bearEntry.ret0236 - bearEntry.ret0382) / 5); //20% towards the next retracement (0.382)
                                    bearEntry.Invalidated = false;
                                    bearEntry.topFib = lows.FirstOrDefault(it => it.ZigZag == theLowValue).Date;
                                    beEntries.Add(bearEntry);
                                }
                            }
                        }
                        var gg = 0;

                        //Invalidate Bear if needed
                        if (beEntries.LastOrDefault(it => !it.Invalidated) != null)
                        {
                            var maxAfterEntry = quotes.Where(it => it.Date >= beEntries.Last(it2 => !it2.Invalidated).InitialDate).Max(it => it.High);
                            if (maxAfterEntry > beEntries.Last(it => !it.Invalidated).ret0)
                            {
                                beEntries.Last(it => !it.Invalidated).Invalidated = true;
                                //Console.WriteLine($"Invalidated latest bearish entry");
                            }
                        }

                        //Invalidate Bull if needed
                        if (buEntries.LastOrDefault(it => !it.Invalidated) != null)
                        {
                            var minAfterEntry = quotes.Where(it => it.Date >= buEntries.Last(it2 => !it2.Invalidated).InitialDate).Min(it => it.Low);
                            if (minAfterEntry < buEntries.Last(it => !it.Invalidated).ret0)
                            {
                                buEntries.Last(it => !it.Invalidated).Invalidated = true;
                                //Console.WriteLine($"Invalidated latest bullish entry");
                            }
                        }

                        //Console.WriteLine("-------------------------------------------------");
                        buEntries.RemoveAll(it => it.Invalidated);
                        beEntries.RemoveAll(it => it.Invalidated);
                        //foreach (var child in buEntries)
                        //{
                        //    Console.WriteLine($"BULLISH POSSIBLE ENTRY @ - {child.InitialDate} - BETWEEN {child.ret0} AND {child.ret1}");
                        //}
                        //foreach (var child in beEntries)
                        //{
                        //    Console.WriteLine($"BEARISH POSSIBLE ENTRY @ - {child.InitialDate} - BETWEEN {child.ret0} AND {child.ret1}");
                        //}
                        //if (buEntries.Count == 0 && beEntries.Count == 0)
                        //{
                        //    Console.WriteLine("CURRENTLY - NO ENTRIES");
                        //}
                    }

                    var _smi = quotes.GetSmi(13, 25, 2, 12);
                    var _hurst = quotes.GetHurst(100);
                    var _atr = quotes.GetAtr(14);
                    var _macd = quotes.GetMacd(12, 26, 9);
                    if (quotes.Last().Date >= new DateTime(2023, 06, 15, 18, 0, 0))
                    {
                        var htt = 0;
                    }
                    if (true) //switch from true / false - LIVE / TESTING
                    {
                        if (openPositions.Count == 0 || (openPositions.Count > 0 && openPositions.FirstOrDefault().type == "short"))
                        {
                            // check for LONG POSITION
                            var bullEntry = buEntries.LastOrDefault(it => !it.Invalidated);
                            //LONG CHECK
                            //if (bullEntry != null) Console.WriteLine($"Current Bull Entry: {bullEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");
                            if (bullEntry != null
                                //&& lineSim.Last().OpenPrice <= bullEntry.entryPrice
                                //&& lineSim.Last().ClosePrice >= bullEntry.entryPrice
                                //&& lineSim.Last().ClosePrice < (bullEntry.ret0236 + bullEntry.ret0382) / 2
                                //&& bullEntry.ret0236 - bullEntry.ret0 <= 1000
                                //&& (bullEntry.ret0 % 100 == 0 || bullEntry.ret1 % 100 == 0)                                
                                //&& bullEntry.rsiMom < bullEntry.rsiRet0
                                // && _smi.Last().Smi > _smi.Last().Signal
                                //&& _smi.Last().Smi > -40
                                //&& _smi.First(it => it.Date == bullEntry.InitialDate).Signal <= -20
                                //&& _hurst.Last().HurstExponent > 0.1
                                //&& (lineSim.Last().CloseTime - bullEntry.InitialDate).TotalMinutes <= 600
                                //&& (decimal)_atr.Last().Atr.Value * 1.5m <= (lineSim.Last().HighPrice - lineSim.Last().LowPrice)
                                //&& lineSim.Skip(352).Min(it => it.LowPrice) >= bullEntry.ret0
                                )
                            {
                                // VOLUME PROFILE FIBONACCI SYNC CHECK !!!
                                /*VP.Clear();
                                volumeProfileCalculation(lineData4H.Where(it
                                                        => it.OpenTime >= bullEntry.Ret1Date.AddMinutes(-30) //new DateTime(2023, 5, 25, 1, 24, 0)
                                                        && it.OpenTime <= bullEntry.InitialDate//new DateTime(2023, 5, 29, 0, 24, 0)).ToList()
                                                        ).ToList(), true);*/

                                decimal avgVP = VP.Average(it => it.sum);
                                int cnt = 0;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0236 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0382 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret05 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0618 && it.interval.Item2 <= bullEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret065 && it.interval.Item2 <= bullEntry.ret065).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bullEntry.ret0786 && it.interval.Item2 <= bullEntry.ret0786).sum <= avgVP) cnt++;


                                var ret0Candle = lineSim.First(it => it.CloseTime == bullEntry.InitialDate);
                                if (cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) > ((2 * ret0Candle.HighPrice + ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 5, 30, 0, 0, 0))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"LONG SIGNAL! RET0 TIME: {bullEntry.InitialDate} | Top Fib @ {bullEntry.topFib} | Current Profit: {totalProfit}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (openPositions.Count > 0 && openPositions.FirstOrDefault().Quantity > 0)
                                    {
                                        //close previous position
                                        Console.WriteLine($"Sending request to CLOSE previous SHORT position ... Current candle time is: {lineSim.Last().OpenTime}");

                                        //calculate pnl of previous
                                        var cp = calculatePnL(openPositions.First().EntryPrice, bullEntry.entryPrice, openPositions.First().Quantity, 1, openPositions.First().type == "short" ? -1 : 1);
                                        totalProfit += cp;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Closed ongoing position. Took PROFIT at {bullEntry.entryPrice}, Quantity of {openPositions.First().Quantity}. WIN of {cp}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        positionHistories.Add(new positionHistory { posData = openPositions.First(), hitTargets = openPositions.First().ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                        Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPositions.First().rsiMom} --- {openPositions.First().rsiRet0}");
                                        openPositions.Clear();
                                    }

                                    var treshTargets = 25;
                                    var availableProfitTargets = 0;
                                    decimal? stopPrice = bullEntry.ret0236;
                                    if (true && bullEntry.ret1618 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0786; }
                                    if (true && bullEntry.ret1 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0618; }
                                    if (true && bullEntry.ret0786 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0618; }
                                    if (true && bullEntry.ret0618 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0382; }
                                    if (true && bullEntry.ret05 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0236; }
                                    if (true && bullEntry.ret0382 - treshTargets > lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bullEntry.ret0; }
                                    //stopPrice = bullEntry.ret0236;

                                    if (availableProfitTargets < 6)
                                    {
                                        var ttt = 0;
                                    }

                                    var targets = new List<ProfitTarget>();
                                    if (true && bullEntry.ret0382 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0382).Value - treshTargets, Quantity = investment / availableProfitTargets }); //REMOVE THESE TPS
                                    if (true && bullEntry.ret05 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret05).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret0618 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0618).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret0786 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret0786).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret1 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret1).Value - treshTargets, Quantity = investment / availableProfitTargets });
                                    if (true && bullEntry.ret1618 - treshTargets > lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bullEntry.ret1618).Value - treshTargets, Quantity = investment / availableProfitTargets });

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = quotes.Last().Date,
                                        stopLimitPrice = stopPrice - treshTargets,
                                        type = "long",
                                        EntryPrice = lineSim.Last().ClosePrice,
                                        ret0 = bullEntry.ret0,
                                        rsiRet0 = bullEntry.rsiRet0,
                                        rsiMom = bullEntry.rsiMom,
                                        Quantity = investment,
                                        ProfitTargets = targets.ToList()
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened LONG position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }

                        if (openPositions.Count == 0 || (openPositions.Count > 0 && openPositions.FirstOrDefault().type == "long"))
                        {
                            // check for SHORT POSITION
                            var bearEntry = beEntries.LastOrDefault(it => !it.Invalidated);
                            //SHORT CHECK
                            //if (bearEntry != null) Console.WriteLine($"Current Bear Entry: {bearEntry.entryPrice} | Previous Kline Open: {lineSim.Last().OpenPrice} | Previous Kline Close: {lineSim.Last().ClosePrice}");

                            if (bearEntry != null
                                //&& lineSim.Last().OpenPrice >= bearEntry.entryPrice
                                //&& lineSim.Last().ClosePrice <= bearEntry.entryPrice
                                //&& lineSim.Last().ClosePrice > (bearEntry.ret0236 + bearEntry.ret0382) / 2
                                //&& bearEntry.ret0 - bearEntry.ret0236 <= 1000
                                //&& (bearEntry.ret0 % 100 == 0 || bearEntry.ret1 % 100 == 0)
                                //&& bearEntry.rsiMom >= bearEntry.rsiRet0
                                //&& _smi.Last().Smi < _smi.Last().Signal
                                //&& _smi.Last().Smi < 40
                                //&& _smi.First(it => it.Date == bearEntry.InitialDate).Signal >= 20
                                //&& _hurst.Last().HurstExponent > 0.1
                                //&& (lineSim.Last().CloseTime - bearEntry.InitialDate).TotalMinutes <= 600
                                //&& (decimal)_atr.Last().Atr.Value * 1.5m <= (lineSim.Last().HighPrice - lineSim.Last().LowPrice)
                                //&& lineSim.Skip(352).Max(it => it.HighPrice) <= bearEntry.ret0
                                )
                            {
                                // VOLUME PROFILE FIBONACCI SYNC CHECK !!!
                                /*VP.Clear();
                                volumeProfileCalculation(lineData4H.Where(it
                                                        => it.OpenTime >= bearEntry.Ret1Date.AddMinutes(-30) //new DateTime(2023, 5, 25, 1, 24, 0)
                                                        && it.OpenTime <= bearEntry.InitialDate//new DateTime(2023, 5, 29, 0, 24, 0)).ToList()
                                                        ).ToList(), true);*/

                                var ret0Candle = lineSim.First(it => it.CloseTime == bearEntry.InitialDate);

                                decimal avgVP = VP.Average(it => it.sum);
                                int cnt = 0;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0236 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0382 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret05 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0618 && it.interval.Item2 <= bearEntry.ret0236).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret065 && it.interval.Item2 <= bearEntry.ret065).sum <= avgVP) cnt++;
                                if (VP.First(it => it.interval.Item1 <= bearEntry.ret0786 && it.interval.Item2 <= bearEntry.ret0786).sum <= avgVP) cnt++;

                                if (cnt >= 4 || Math.Max(ret0Candle.ClosePrice, ret0Candle.OpenPrice) < ((ret0Candle.HighPrice + 2 * ret0Candle.LowPrice) / 3))
                                {

                                    if (lineSim.Last().CloseTime >= new DateTime(2023, 6, 4))
                                    {
                                        var yy = 0;
                                    }
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"SHORT SIGNAL! RET0 TIME: {bearEntry.InitialDate} | Top Fib @ {bearEntry.topFib} | Current Profit: {totalProfit}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (openPositions.Count > 0 && openPositions.FirstOrDefault().Quantity > 0)
                                    {
                                        //close previous position
                                        Console.WriteLine($"Sending request to CLOSE previous LONG position ... Current candle time is: {lineSim.Last().OpenTime}");

                                        //calculate pnl of previous
                                        var cp = calculatePnL(openPositions.First().EntryPrice, bearEntry.entryPrice, openPositions.First().Quantity, 1, openPositions.First().type == "short" ? -1 : 1);
                                        totalProfit += cp;

                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Closed ongoing position. Took PROFIT at {bearEntry.entryPrice}, Quantity of {openPositions.First().Quantity}. WIN of {cp}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        positionHistories.Add(new positionHistory { posData = openPositions.First(), hitTargets = openPositions.First().ProfitTargets.Count(it => it.Alive == false), profit = 0, stoppedLoss = true });
                                        Console.WriteLine($"RSI OF START LINE WAS ---- AND --- RSI OF RET0 AFTER ALL THESE WINNINGS WAS: {openPositions.First().rsiMom} --- {openPositions.First().rsiRet0}");
                                        openPositions.Clear();
                                    }

                                    var treshTargets = 25;
                                    var availableProfitTargets = 0;
                                    decimal? stopPrice = bearEntry.ret0236;

                                    if (true && bearEntry.ret1618 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0786; }
                                    if (true && bearEntry.ret1 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0618; }
                                    if (true && bearEntry.ret0786 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret05; }
                                    if (true && bearEntry.ret0618 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0382; }
                                    if (true && bearEntry.ret05 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0236; }
                                    if (true && bearEntry.ret0382 + treshTargets < lineSim.Last().ClosePrice) { ++availableProfitTargets; stopPrice = bearEntry.ret0; }
                                    //stopPrice = bearEntry.ret0236;


                                    if (availableProfitTargets < 6)
                                    {
                                        var ttt = 0;
                                    }

                                    var targets = new List<ProfitTarget>();

                                    if (true && bearEntry.ret0382 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0382 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret05 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret05 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret0618 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0618 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret0786 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret0786 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret1 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret1 + treshTargets).Value, Quantity = investment / availableProfitTargets });
                                    if (true && bearEntry.ret1618 + treshTargets < lineSim.Last().ClosePrice) targets.Add(new ProfitTarget { Alive = true, Price = Roundto1(bearEntry.ret1618 + treshTargets).Value, Quantity = investment / availableProfitTargets });

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = quotes.Last().Date,
                                        stopLimitPrice = stopPrice + treshTargets,//bearEntry.ret0236 + 25,
                                        type = "short",
                                        EntryPrice = lineSim.Last().ClosePrice,
                                        ret0 = bearEntry.ret0,
                                        rsiRet0 = bearEntry.rsiRet0,
                                        rsiMom = bearEntry.rsiMom,
                                        Quantity = investment,
                                        ProfitTargets = targets.ToList()
                                    });
                                    totalProfit -= openPositions.First().Quantity * openPositions.First().EntryPrice * fee / 100;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine($"Opened SHORT position with the following:\nTime: {openPositions.First().startTime}\nEntry Price: {openPositions.First().EntryPrice}\nStopPrice: {openPositions.First().stopLimitPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else
                                {
                                    Console.WriteLine("--------------------------------- NOT A PIN BAR RET0");
                                }
                            }
                        }
                        //Console.WriteLine("-------------------------");
                    }
                }

                //Console.ReadLine();
            }
            var yty = positionHistories.GroupBy(it => it.hitTargets).ToList();
            Console.WriteLine($"TOTAL PROFIT SO FAR: {totalProfit}");
            var gtt = 0;
        }

        private static (MarketStructure, MarketStructure) GetPreviousPivots(List<MarketStructure> MSL)
        {
            var list = new List<MarketStructure>(MSL);
            var second = list.Last(it => it.CurrEvent == EventType.PivotLow || it.CurrEvent == EventType.PivotHigh);
            list.Remove(second);
            var first = list.Last(it => it.CurrEvent == EventType.PivotLow || it.CurrEvent == EventType.PivotHigh);
            return (first, second);
        }

        private static object GetMarketStructure(List<IBinanceKline> lineSim, BiasType bias, ZigZagResult zigZagResult1, ZigZagResult zigZagResult2)
        {
            var MSL = new List<MarketStructure>();
            var bottomAfterBOSCHOCH = new LatePivot();
            var topAfterBOSCHOCH = new LatePivot();

            if (bias == BiasType.Bullish) {
                var ms = new MarketStructure();
                ms.CurrEvent = EventType.PivotLow;
                ms.Date = zigZagResult1.Date;
                ms.Price = zigZagResult1.ZigZag.Value;
                ms.CurrBias = BiasType.Bullish;
                MSL.Add(ms);
                ms = new MarketStructure();
                ms.CurrEvent = EventType.PivotHigh;
                ms.Date = zigZagResult2.Date;
                ms.Price = zigZagResult2.ZigZag.Value;
                ms.CurrBias = BiasType.Bullish;
                MSL.Add(ms);
            } else
            {
                var ms = new MarketStructure();
                ms.CurrEvent = EventType.PivotHigh;
                ms.Date = zigZagResult1.Date;
                ms.Price = zigZagResult1.ZigZag.Value;
                ms.CurrBias = BiasType.Bearish;
                MSL.Add(ms);
                ms = new MarketStructure();
                ms.CurrEvent = EventType.PivotLow;
                ms.Date = zigZagResult2.Date;
                ms.Price = zigZagResult2.ZigZag.Value;
                ms.CurrBias = BiasType.Bearish;
                MSL.Add(ms);
            }

            foreach (var child in lineSim)
            {
                if (child.OpenTime >= MSL[1].Date)
                {
                    var ff = 0;

                    //var result = GetPreviousPivots(MSL);
                    var lastPivotHigh = MSL.Last(it => it.CurrEvent == EventType.PivotHigh);
                    var lastPivotLow = MSL.Last(it => it.CurrEvent == EventType.PivotLow);

                    if (true || MSL.Last().CurrEvent == EventType.PivotHigh || MSL.Last().CurrEvent == EventType.PivotLow)
                    {
                        if (MSL.Last().CurrBias == BiasType.Bearish)
                        {
                            //verifica aici daca candela curenta se inchide peste PivotHigh sau se inchide sub PivotLow. fa o metoda care gaseste anterioarele 2 pivots.
                            if (MSL.Last().CurrEvent != EventType.BOSDOWN && MSL.Last().CurrEvent != EventType.CHoCHDOWN && Math.Min(child.OpenPrice, child.ClosePrice) < lastPivotLow.Price && Math.Max(child.OpenPrice, child.ClosePrice) > lastPivotLow.Price) //BOS TO THE DOWNSIDE
                            {
                                var range = lineSim.Where(it => it.OpenTime >= lastPivotLow.Date && it.OpenTime <= child.OpenTime).ToList();
                                var topCandleValue = range.Max(it => it.HighPrice);
                                var topCandle = range.FirstOrDefault(it => it.HighPrice == topCandleValue);

                                var ms = new MarketStructure();
                                ms.CurrEvent = EventType.PivotHigh;
                                ms.Date = topCandle.CloseTime;
                                ms.Price = topCandle.HighPrice;
                                ms.CurrBias = BiasType.Bearish;
                                MSL.Add(ms); //Add top pivot when BOSDOWN

                                ms = new MarketStructure();
                                ms.CurrEvent = EventType.BOSDOWN;
                                ms.Date = child.CloseTime;
                                ms.Price = child.LowPrice;
                                ms.CurrBias = BiasType.Bearish;
                                MSL.Add(ms); //Add BOSDOWN
                            }
                            else
                            {
                                if (Math.Min(child.OpenPrice, child.ClosePrice) < lastPivotHigh.Price && Math.Max(child.OpenPrice, child.ClosePrice) > lastPivotHigh.Price) //CHoCH to the UPSIDE
                                {
                                    var ms = new MarketStructure();
                                    if (!(MSL.Last().CurrEvent == EventType.PivotHigh || MSL.Last().CurrEvent == EventType.PivotLow)) //daca ultimul inainte de CHoCH NU este un pivot, atunci pune-l inainte sa pui CHoCH
                                    {
                                        var lastBOSorCHoCHcandle = MSL.Last(it => it.CurrEvent == EventType.BOSDOWN || it.CurrEvent == EventType.CHoCHDOWN);
                                        var range = lineSim.Where(it => it.OpenTime >= lastBOSorCHoCHcandle.Date && it.OpenTime <= child.OpenTime).ToList();
                                        var bottomCandleValue = range.Min(it => it.LowPrice);
                                        var bottomCandle = range.FirstOrDefault(it => it.LowPrice == bottomCandleValue);

                                        ms.CurrEvent = EventType.PivotLow;
                                        ms.Date = bottomCandle.CloseTime;
                                        ms.Price = bottomCandle.LowPrice;
                                        ms.CurrBias = BiasType.Bearish;
                                        MSL.Add(ms); //Add bottom pivot when CHoCH UP and it wasn't added before
                                    }
                                    ms = new MarketStructure();
                                    ms.CurrEvent = EventType.CHoCHUP;
                                    ms.Date = child.CloseTime;
                                    ms.Price = child.ClosePrice;
                                    ms.CurrBias = BiasType.Bullish;
                                    MSL.Add(ms); //Add CHoCH up when MSB from Bearish to Bullish

                                    //----------------
                                    topAfterBOSCHOCH.Candle = child; //ADD THE CANDLE TO THE PIVOT CALCULATION BECAUSE OTHERWISE YOU WOULD GO OVER IT WITHOUT CONSIDERING
                                    topAfterBOSCHOCH.ReverseCandlesAhead = 0;
                                    if (child.OpenPrice > child.ClosePrice) //red candle
                                    {
                                        topAfterBOSCHOCH.ReverseCandlesAhead++;
                                    }
                                }
                            }

                            if (MSL.Last().CurrEvent == EventType.BOSDOWN || MSL.Last().CurrEvent == EventType.CHoCHDOWN)
                            {
                                if (bottomAfterBOSCHOCH.Candle == null || (bottomAfterBOSCHOCH != null && bottomAfterBOSCHOCH.Candle.LowPrice > child.LowPrice))
                                {
                                    bottomAfterBOSCHOCH.Candle = child;
                                    bottomAfterBOSCHOCH.ReverseCandlesAhead = 0;
                                }
                                if (bottomAfterBOSCHOCH.Candle.LowPrice <= child.LowPrice)
                                {
                                    if (child.OpenPrice < child.ClosePrice) //green candle
                                    {
                                        bottomAfterBOSCHOCH.ReverseCandlesAhead++;
                                    }
                                }
                                if (bottomAfterBOSCHOCH.ReverseCandlesAhead >= 3)
                                {
                                    var ms = new MarketStructure();
                                    ms.CurrEvent = EventType.PivotLow;
                                    ms.Date = bottomAfterBOSCHOCH.Candle.CloseTime;
                                    ms.Price = bottomAfterBOSCHOCH.Candle.LowPrice;
                                    ms.CurrBias = BiasType.Bearish;
                                    MSL.Add(ms); //Add bottom pivot when BOS UP

                                    bottomAfterBOSCHOCH = new LatePivot();
                                }
                            }
                        } else
                        {
                            if (MSL.Last().CurrEvent != EventType.BOSUP && MSL.Last().CurrEvent != EventType.CHoCHUP && Math.Min(child.OpenPrice, child.ClosePrice) < lastPivotHigh.Price && Math.Max(child.OpenPrice, child.ClosePrice) > lastPivotHigh.Price) //BOS TO THE UPSIDE
                            {
                                var range = lineSim.Where(it => it.OpenTime >= lastPivotHigh.Date && it.OpenTime <= child.OpenTime).ToList();
                                var bottomCandleValue = range.Min(it => it.LowPrice);
                                var bottomCandle = range.FirstOrDefault(it => it.LowPrice == bottomCandleValue);

                                var ms = new MarketStructure();
                                ms.CurrEvent = EventType.PivotLow;
                                ms.Date = bottomCandle.CloseTime;
                                ms.Price = bottomCandle.LowPrice;
                                ms.CurrBias = BiasType.Bullish;
                                MSL.Add(ms); //Add bottom pivot when BOS UP

                                ms = new MarketStructure();
                                ms.CurrEvent = EventType.BOSUP;
                                ms.Date = child.CloseTime;
                                ms.Price = child.HighPrice;
                                ms.CurrBias = BiasType.Bullish;
                                MSL.Add(ms); //Add BOS UP
                            }
                            else
                            {
                                if (Math.Min(child.OpenPrice, child.ClosePrice) < lastPivotLow.Price && Math.Max(child.OpenPrice, child.ClosePrice) > lastPivotLow.Price) //CHoCH to the UPSIDE
                                {
                                    var ms = new MarketStructure();
                                    if (!(MSL.Last().CurrEvent == EventType.PivotHigh || MSL.Last().CurrEvent == EventType.PivotLow)) //daca ultimul inainte de CHoCH NU este un pivot, atunci pune-l inainte sa pui CHoCH
                                    {
                                        var lastBOSorCHoCHcandle = MSL.Last(it => it.CurrEvent == EventType.BOSUP || it.CurrEvent == EventType.CHoCHUP);
                                        var range = lineSim.Where(it => it.OpenTime >= lastBOSorCHoCHcandle.Date && it.OpenTime <= child.OpenTime).ToList();
                                        var topCandleValue = range.Min(it => it.HighPrice);
                                        var topCandle = range.FirstOrDefault(it => it.HighPrice == topCandleValue);

                                        ms.CurrEvent = EventType.PivotHigh;
                                        ms.Date = topCandle.CloseTime;
                                        ms.Price = topCandle.LowPrice;
                                        ms.CurrBias = BiasType.Bullish;
                                        MSL.Add(ms); //Add top pivot when CHoCH DOWN and it wasn't added before
                                    }
                                    ms = new MarketStructure();
                                    ms.CurrEvent = EventType.CHoCHDOWN;
                                    ms.Date = child.CloseTime;
                                    ms.Price = child.ClosePrice;
                                    ms.CurrBias = BiasType.Bearish;
                                    MSL.Add(ms); //Add CHoCH DOWN when MSB from Bullish to Bearish

                                    //----------------
                                    bottomAfterBOSCHOCH.Candle = child; //ADD THE CANDLE TO THE PIVOT CALCULATION BECAUSE OTHERWISE YOU WOULD GO OVER IT WITHOUT CONSIDERING
                                    bottomAfterBOSCHOCH.ReverseCandlesAhead = 0;
                                    if (child.OpenPrice < child.ClosePrice) //green candle
                                    {
                                        bottomAfterBOSCHOCH.ReverseCandlesAhead++;
                                    }
                                }
                            }

                            if (MSL.Last().CurrEvent == EventType.BOSUP || MSL.Last().CurrEvent == EventType.CHoCHUP)
                            {
                                if (topAfterBOSCHOCH.Candle == null || (topAfterBOSCHOCH != null && topAfterBOSCHOCH.Candle.HighPrice < child.HighPrice))
                                {
                                    topAfterBOSCHOCH.Candle = child;
                                    topAfterBOSCHOCH.ReverseCandlesAhead = 0;
                                }
                                if (topAfterBOSCHOCH.Candle.HighPrice >= child.HighPrice)
                                {
                                    if (child.OpenPrice > child.ClosePrice) //red candle
                                    {
                                        topAfterBOSCHOCH.ReverseCandlesAhead++;
                                    }
                                }
                                if (topAfterBOSCHOCH.ReverseCandlesAhead >= 3)
                                {
                                    var ms = new MarketStructure();
                                    ms.CurrEvent = EventType.PivotHigh;
                                    ms.Date = topAfterBOSCHOCH.Candle.CloseTime;
                                    ms.Price = topAfterBOSCHOCH.Candle.HighPrice;
                                    ms.CurrBias = BiasType.Bullish;
                                    MSL.Add(ms); //Add top missing pivot after bos up/choch up if at least 3 red candles

                                    topAfterBOSCHOCH = new LatePivot();
                                }
                            }
                        }
                    }
                }
            }

            foreach (var child in MSL)
            {
                Console.WriteLine(String.Format("{0} ### {1} ({2}) ### {3}", child.Date, child.CurrEvent, child.CurrBias, child.Price));
            }
            throw new NotImplementedException();
        }

        private static decimal? Roundto1(decimal? ret)
        {
            return Math.Round(ret.Value, 1);
        }

        private static async Task<CryptoExchange.Net.Objects.WebCallResult<Binance.Net.Objects.Models.Futures.BinanceUsdFuturesOrder>> PlaceTakeProfit(BinanceRestClient client, OrderSide orderSide, decimal amount, decimal? price)
        {
            return await client.UsdFuturesApi.Trading.PlaceOrderAsync("BTCBUSD", orderSide
                                                                            , FuturesOrderType.Limit
                                                                            , quantity: amount
                                                                            , price: price
                                                                            , timeInForce: TimeInForce.GoodTillCanceled
                                                                            , reduceOnly: true
                                                                            , workingType: WorkingType.Mark);
        }

        static decimal? stopLossBull = null;
        static decimal? stopLossBear = null;
        static decimal? entryBull = null;
        static decimal? entryBear = null;

        private static decimal percChange(decimal V1, decimal V2)
        {
            var change = ((V2 - V1) / Math.Abs(V1)) * 100;
            return change;
        }
        private static void CalculateZigzags(List<Quote> quotes, int depth, int deviation)
        {

            int Depth = 300;
            var last = 0;
            var direction = 1;
            var zzRLL = new List<ZigZagResult>();
            var zzRHL = new List<ZigZagResult>();
            decimal calcDepth = Depth * 0.01M; // BUSD _Point Size?!
            decimal calcDeviation = 10 * 0.01M; // BUSD _Point Size?!

            var zzL = new decimal[quotes.Count];
            var zzH = new decimal[quotes.Count];
            for (int i = 0; i < quotes.Count() - 1; i++)
            {
                ZigZagResult zzRL = new ZigZagResult(quotes[i].Date);
                ZigZagResult zzRH = new ZigZagResult(quotes[i].Date);
                bool set = false;
                zzL[i] = 0;
                zzH[i] = 0;
                //---
                if (direction > 0)
                {
                    if (quotes[i].High > zzH[last] - deviation)
                    {
                        zzH[last] = 0;
                        zzH[i] = quotes[i].High;
                        if (quotes[i].Low < quotes[last].High - calcDepth)
                        {
                            if (quotes[i].Open < quotes[i].Close) zzH[last] = quotes[last].High; else direction = -1;
                            zzL[i] = quotes[i].Low;
                        }
                        last = i;
                        set = true;
                    }
                    if (quotes[i].Low < zzH[last] - depth && (!set || quotes[i].Open > quotes[i].Close))
                    {
                        zzL[i] = quotes[i].Low;
                        if (quotes[i].High > zzL[i] + depth && quotes[i].Open < quotes[i].Close) zzH[i] = quotes[i].High; else direction = -1;
                        last = i;
                    }
                }
                else
                {
                    if (quotes[i].Low < zzL[last] + deviation)
                    {
                        zzL[last] = 0;
                        zzL[i] = quotes[i].Low;
                        if (quotes[i].High > quotes[last].Low + depth)
                        {
                            if (quotes[i].Open > quotes[i].Close) zzL[last] = quotes[last].Low; else direction = 1;
                            zzH[i] = quotes[i].High;
                        }
                        last = i;
                        set = true;
                    }
                    if (quotes[i].High > zzL[last] + depth && (!set || quotes[i].Open < quotes[i].Close))
                    {
                        zzH[i] = quotes[i].High;
                        if (quotes[i].Low < zzH[i] - depth && quotes[i].Open > quotes[i].Close) zzL[i] = quotes[i].Low; else direction = 1;
                        last = i;
                    }
                }
                zzRL.ZigZag = zzL[i];
                zzRH.ZigZag = zzH[i];

                zzRLL.Add(zzRL);
                zzRHL.Add(zzRH);
            }
            //----
            zzH[quotes.Count() - 1] = 0;
            zzL[quotes.Count() - 1] = 0;
        }

        class ZigZagPattern {
            public ZigZagResult p1 { get; set; }
            public ZigZagResult p2 { get; set; }
            public ZigZagResult p3 { get; set; }
            public ZigZagResult p4 { get; set; }
            public ZigZagResult p5 { get; set; }
        }

        class VolumeProfile
        {
            public Tuple<decimal, decimal> interval { get; set; }
            public decimal sum { get; set; }
            public bool isInterestZone { get; set; }
        }

        class DailyVp
        {
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public Tuple<decimal, decimal> valueArea { get; set; }
            public Tuple<decimal, decimal> poc { get; set; }
        }

        static List<VolumeProfile> VP = new List<VolumeProfile>();
        static List<DailyVp> dailyProfiles = new List<DailyVp>();

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static decimal FindOverlapping(decimal start1, decimal end1, decimal start2, decimal end2)
        {
            return Math.Max(0, Math.Min(end1, end2) - Math.Max(start1, start2));
        }

        static List<VolumeProfile> volumeProfileCalculation(List<IBinanceKline> lineData, bool write = false, decimal ret0 = 0)
        {
            //************************************
            // Target 1 = Point B of the Harmonic, Entry is where you get in, StopLoss in a Bull is entry - (Target1 - Entry)
            //************************************

            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            bool candleStarted = false;
            List<VolumeProfile> VP = new List<VolumeProfile>();

            int step = 50;
            int min = (int)Math.Floor(lineData.Min(it => it.LowPrice));
            if (ret0 != 0) min = (int)Math.Floor(ret0);
            int max = (int)Math.Floor(lineData.Max(it => it.HighPrice)) + step;

            for (int i = min; i < max - step; i += step)
            {
                var x = new VolumeProfile();
                x.interval = new Tuple<decimal, decimal>(i, (decimal)i + step - 0.001M);
                x.sum = 0;
                x.isInterestZone = false;
                VP.Add(x);
            }

            foreach (var child in lineData)
            {
                var volumeToAdd = child.Volume;// / Math.Max((child.HighPrice - child.LowPrice), 1);
                foreach (var dchild in VP)
                {
                    if (child.LowPrice <= dchild.interval.Item2 && dchild.interval.Item1 <= child.HighPrice)
                    {
                        dchild.sum += volumeToAdd;
                    }
                }
            }

            if (!write)
            {
                return VP;
            }
            // ----------------------------------------------------------------------

            //decimal down = 25000;
            //decimal up = 29000;
            //int intcnt = 0;
            //decimal sum = 0;
            decimal avg = 0;

            for (int i = 0; i < step; i++)
            {
                avg += (VP[i].sum / step);
            }
            int stu = 0;

            for (var V = VP.Count - 1; V >= 0; V--)
            {
                var child = VP[V];
                if (stu >= step)
                {
                    avg += VP[stu].sum / step;
                    avg -= VP[stu - step].sum / step;
                }
                if (min <= child.interval.Item2 && child.interval.Item1 <= max)
                {
                    if (child.sum > avg && child.sum > 300)
                    {
                        if (write) Console.Write("### ");
                        child.isInterestZone = true;
                    }
                    else
                    {
                        if (write) Console.Write("--- ");
                        child.isInterestZone = false;
                    }
                    if (write) Console.Write("{0} - {1}:  ", child.interval.Item1, child.interval.Item2);
                    for (int i = 0; i < child.sum / 600; i++)
                    {
                        if (write) Console.Write(".");
                    }
                    if (write) Console.WriteLine();
                }
                stu++;
            }

            foreach (var child in VP)
            {
                var tchild = child.isInterestZone;
                var schild = child.interval.Item2;

            }

            //FIND HVN AND LVN

            List<VolumeNode> VNs = new List<VolumeNode>();
            int range = 20;

            //calc HVN
            foreach (var (value, i) in VP.Select((value, i) => (value, i)))
            {
                if (i >= range && i < VP.Count - range)
                {
                    var okHVN = true;
                    var okLVN = true;

                    for (int j = i - range; j < i; j++)
                    {
                        if (VP[i].sum < VP[j].sum)
                        {
                            okHVN = false; break;
                        }
                    }

                    for (int j = i + 1; j <= i + range; j++)
                    {
                        if (VP[i].sum < VP[j].sum)
                        {
                            okHVN = false; break;
                        }
                    }

                    for (int j = i - range; j < i; j++)
                    {
                        if (VP[i].sum > VP[j].sum)
                        {
                            okLVN = false; break;
                        }
                    }

                    for (int j = i + 1; j <= i + range; j++)
                    {
                        if (VP[i].sum > VP[j].sum)
                        {
                            okLVN = false; break;
                        }
                    }


                    if (okHVN)
                    {
                        VolumeNode vn = new VolumeNode();
                        vn.Interval = VP[i].interval;
                        vn.Type = VolumeNodeType.HVN;
                        VNs.Add(vn);
                    }

                    if (okLVN)
                    {
                        VolumeNode vn = new VolumeNode();
                        vn.Interval = VP[i].interval;
                        vn.Type = VolumeNodeType.LVN;
                        VNs.Add(vn);
                    }
                }
            }

            var yyt = 0;
            //Console.ReadLine();
            return VP;
        }

        static async void volumeProfilePOCCalculation(List<IBinanceKline> lineData)
        {
            //************************************
            // Target 1 = Point B of the Harmonic, Entry is where you get in, StopLoss in a Bull is entry - (Target1 - Entry)
            //************************************

            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-19), DateTime.UtcNow);//"BTCUSDT"
            bool candleStarted = false;

            DateTime startDay = lineData.First().OpenTime.AddDays(1).Date;
            DateTime endDay = lineData.Last().OpenTime.AddDays(-1).Date;

            foreach (var day in EachDay(startDay, endDay))
            {
                var dailyProfile = new DailyVp();
                dailyProfile.start = day;
                dailyProfile.end = day.AddDays(1).AddTicks(-1);

                int step = 20;
                VP = new List<VolumeProfile>();
                for (int i = 0; i < 60000 - step; i += step)
                {
                    var x = new VolumeProfile();
                    x.interval = new Tuple<decimal, decimal>(i, (decimal)i + step - 0.01M);
                    x.sum = 0;
                    x.isInterestZone = false;
                    VP.Add(x);
                }

                //VP.Reverse();

                var dayRange = lineData.Where(it => it.OpenTime >= day && it.OpenTime <= day.AddDays(1).AddTicks(-1)).ToList();
                decimal totalVolume = 0m;

                foreach (var child in dayRange)
                {
                    var volumeToAdd = child.Volume / Math.Max((child.HighPrice - child.LowPrice), 0.01m);
                    totalVolume += child.Volume;
                    foreach (var dchild in VP)
                    {
                        if (child.LowPrice <= dchild.interval.Item2 && dchild.interval.Item1 <= child.HighPrice)
                        {
                            var over = FindOverlapping(child.LowPrice, child.HighPrice, dchild.interval.Item1, dchild.interval.Item2);
                            dchild.sum += (volumeToAdd * over);
                        }
                    }
                }

                // ----------------------------------------------------------------------

                var maxVpBar = VP.OrderByDescending(it => it.sum).First();
                dailyProfile.poc = new Tuple<decimal, decimal>(maxVpBar.interval.Item1, maxVpBar.interval.Item2);

                // CALCULATE VALUE AREA

                decimal volumeVA = maxVpBar.sum;
                int vaNodeUpper = VP.FindIndex(it => it == maxVpBar) + 1;
                int vaNodeLower = VP.FindIndex(it => it == maxVpBar) - 1;
                int upperLimit = VP.Count - 1;
                int lowerLimit = 0;
                decimal sum1 = 0m;
                decimal sum2 = 0m;
                int lowFinal = vaNodeLower + 1;
                int highFinal = vaNodeUpper - 1;
                while (volumeVA < totalVolume * 0.7m)
                {
                    if (vaNodeUpper + 1 > upperLimit)
                        sum1 = 0m;
                    else
                        sum1 = VP[vaNodeUpper].sum + VP[vaNodeUpper + 1].sum;

                    if (vaNodeLower - 1 < lowerLimit)
                        sum2 = 0m;
                    else
                        sum2 = VP[vaNodeLower].sum + VP[vaNodeLower - 1].sum;

                    if (sum1 > sum2)
                    {
                        volumeVA += sum1;
                        vaNodeUpper += 2;
                        highFinal = vaNodeUpper - 1;
                    } else
                    {
                        volumeVA += sum2;
                        vaNodeLower -= 2;
                        lowFinal = vaNodeLower + 1;
                    }
                }

                dailyProfile.valueArea = new Tuple<decimal, decimal>(VP[lowFinal].interval.Item1, VP[highFinal].interval.Item1);

                dailyProfiles.Add(dailyProfile);
            }

            for (int i = 1; i <= dailyProfiles.Count; i++)
            {
                Console.WriteLine("----------");
                Console.WriteLine("{0}:\nPOC of previous day: {1}"
                    , dailyProfiles[i - 1].start.AddDays(1)
                    , dailyProfiles[i - 1].poc);

                var priceStart = lineData.First(it => it.OpenTime > dailyProfiles[i - 1].end).OpenPrice;

                if (priceStart >= dailyProfiles[i - 1].valueArea.Item1 && priceStart <= dailyProfiles[i - 1].valueArea.Item2)
                {
                    Console.WriteLine("Start price {0} in Value Area {1}", priceStart, dailyProfiles[i - 1].valueArea);


                    var pocInter = lineData.FirstOrDefault(it
                        => it.OpenTime >= dailyProfiles[i - 1].end.AddMinutes(1)
                        && FindOverlapping(it.LowPrice, it.HighPrice, dailyProfiles[i - 1].poc.Item1, dailyProfiles[i - 1].poc.Item2) > 0m);

                    var bars = lineData.Where(it =>
                            it.OpenTime >= dailyProfiles[i - 1].end.AddMinutes(1)
                            && it.OpenTime <= pocInter.OpenTime);

                    if (pocInter != null)
                    {
                        Console.WriteLine("TRADE WOULD HIT @ {0} --- FROM {1} to {2}", pocInter.OpenTime, priceStart, dailyProfiles[i - 1].poc);
                        Console.WriteLine("ENTRY: {0} | MAX HIGH: {1} | MIN LOW: {2}", priceStart, bars.Max(it => it.HighPrice), bars.Min(it => it.LowPrice));
                    }
                    else
                    {
                        Console.WriteLine("NO HIT");
                    }
                } else
                {
                    Console.WriteLine("START PRICE NOT IN VALUE AREA");
                }

                /*Console.WriteLine("----------");
                Console.WriteLine("{0}:\nPOC: {1}\nValue Area: {2} - {3}"
                    , dailyProfiles[i].start
                    , dailyProfiles[i].poc
                    , dailyProfiles[i].valueArea.Item1
                    , dailyProfiles[i].valueArea.Item2);

                var priceStart = lineData.First(it => it.OpenTime > dailyProfiles[i].end).Open;
                Console.WriteLine("Price start next day: {0}\n", priceStart);*/
            }

            var gt = 0;
            // ----------------------------------------------------------------------
        }

        class FibRetracement
        {
            public decimal? ret0 { get; set; }
            public decimal? ret0236 { get; set; }
            public decimal? ret0382 { get; set; }
            public decimal? ret05 { get; set; }
            public decimal? ret0618 { get; set; }
            public decimal? ret065 { get; set; }
            public decimal? ret0786 { get; set; }
            public decimal? ret1 { get; set; }
            public decimal? score { get; set; }
        }

        private static decimal FindRet0FromFib1Fib0236(decimal ret1, decimal ret0236)
        {
            return (ret0236 - ret1 * 0.236m) / 0.764m;
        }

        private static void AddNewFibCalculation(List<FibRetracement> fibRets, List<ZigZagResult> highLows, List<Quote> rangeQuotes, decimal ret0, decimal ret1, int direction, decimal tresh)
        {
            var newFib = new FibRetracement();
            newFib.ret1 = ret1;
            newFib.ret0 = ret0;
            var diff = Math.Abs(ret1 - ret0);
            newFib.ret0236 = ret0 + direction * -1 * diff * 0.236m;
            newFib.ret0382 = ret0 + direction * -1 * diff * 0.382m;
            newFib.ret05 = ret0 + direction * -1 * diff * 0.5m;
            newFib.ret0618 = ret0 + direction * -1 * diff * 0.618m;
            newFib.ret065 = ret0 + direction * -1 * diff * 0.65m;
            newFib.ret0786 = ret0 + direction * -1 * diff * 0.786m;

            decimal score = 0;
            foreach (var zz in highLows)
            {
                decimal scrSum = 0, scrLow = 0, scrHigh = 0, scrOpen = 0, scrClose = 0;
                int cnt = 0;
                var child = rangeQuotes.First(it => it.Date == zz.Date);

                scrLow = CalculateScore2(child.Low, child.High - child.Low, newFib, 4, tresh);
                scrHigh = CalculateScore2(child.High, child.High - child.Low, newFib, 4, tresh);
                scrOpen = CalculateScore2(child.Open, child.High - child.Low, newFib, 4, tresh);
                scrClose = CalculateScore2(child.Close, child.High - child.Low, newFib, 4, tresh);
                scrSum = scrLow + scrHigh + scrOpen + scrClose;
                if (scrSum > 0)
                {
                    var fdf = 0;
                }
                score += scrSum;
            }

            //var avg = rangeQuotes.Where(it => it.High - it.Low > 50).Average(it => it.High - it.Low);

            foreach (var child in rangeQuotes)
            {
                decimal scrSum = 0, scrLow = 0, scrHigh = 0, scrOpen = 0, scrClose = 0;
                int cnt = 0;

                //var child = rangeQuotes.First(it => it.Date == zz.Date);
                scrLow = CalculateScore2(child.Low, child.High - child.Low, newFib, 1, tresh);
                scrHigh = CalculateScore2(child.High, child.High - child.Low, newFib, 1, tresh);
                scrOpen = CalculateScore2(child.Open, child.High - child.Low, newFib, 1, tresh);
                scrClose = CalculateScore2(child.Close, child.High - child.Low, newFib, 1, tresh);
                scrSum = scrLow + scrHigh + scrOpen + scrClose;
                if (scrSum > 2)
                {
                    var fdf = 0;
                }
                score += scrSum;

                bool intersects = false;
                if (scrSum == 0)
                {
                    if (child.Low < newFib.ret0236 && child.High > newFib.ret0236) intersects = true;
                    if (child.Low < newFib.ret0382 && child.High > newFib.ret0382) intersects = true;
                    if (child.Low < newFib.ret05 && child.High > newFib.ret05) intersects = true;
                    if (child.Low < newFib.ret0618 && child.High > newFib.ret0618) intersects = true;
                    if (child.Low < newFib.ret065 && child.High > newFib.ret065) intersects = true;
                    if (child.Low < newFib.ret0786 && child.High > newFib.ret0786) intersects = true;
                }

                if (intersects)
                {
                    score--;
                }
            }

            newFib.score = score;// (score + score * (7 - inc));
            fibRets.Add(newFib);
            Console.WriteLine(ret0);
        }

        private static decimal? FinonacciFinder(List<IBinanceKline> lineData, bool quickswitch, decimal oneLevel, int direction, DateTime startTime, DateTime endTime) //31.02
        {
            if (!quickswitch) return 0;
            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            IEnumerable<Quote> quotes = GetQuoteFromKlines(lineData.ToList());

            var rangeQuotes = quotes.Where(it => it.Date >= startTime && it.Date <= endTime).ToList();
            var poi = direction == 1 ? rangeQuotes.Max(it => it.High)
                                     : rangeQuotes.Min(it => it.Low);
            var farLimit = poi + 1400 * direction;//oneLevel + (Math.Abs(oneLevel - poi) * 1.5m * direction);
            //farLimit = 26420m;
            var fibRets = new List<FibRetracement>();
            var tresh = 2m;

            var zigZags = rangeQuotes.GetZigZag(EndType.HighLow, 1m);
            var highLows = zigZags.Where(it => it.PointType == "H" || it.PointType == "L").ToList();
            highLows.Add(new ZigZagResult(rangeQuotes.Last().Date));


            for (decimal zeroLevel = poi; ; zeroLevel += (direction * 1m))
            {
                if (zeroLevel == 23847.8m)
                {
                    var err = 0;
                }
                AddNewFibCalculation(fibRets, highLows, rangeQuotes, zeroLevel, oneLevel, direction, tresh);

                if (direction == 1 && zeroLevel > farLimit) break;
                if (direction == -1 && zeroLevel < farLimit) break;
            }

            var sortedFibs = fibRets.OrderByDescending(it => it.score).ToList();

            Console.WriteLine("\n-----\n");

            var ttt = 0;
            while (sortedFibs.Count > 0) {
                ttt++;
                var ret0 = sortedFibs.First().ret0;
                Console.WriteLine($"{ret0} - {sortedFibs.First().ret1} # Score: {sortedFibs.First().score}");
                sortedFibs.RemoveAll(it => Math.Abs(it.ret0.Value - ret0.Value) <= tresh * 5);
                //sortedFibs.RemoveAt(0);
                if (ttt > 500) break;
            }

            return null;
        }

        private static decimal CalculateScore(decimal point, decimal range, FibRetracement newFib)
        {
            decimal diff = 0;
            decimal score = 0;
            decimal thresh = 5m;
            decimal t2 = 50;

            diff = Math.Abs(point - newFib.ret0.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);
            diff = Math.Abs(point - newFib.ret0236.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);
            diff = Math.Abs(point - newFib.ret0382.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);
            diff = Math.Abs(point - newFib.ret05.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 8);
            diff = Math.Abs(point - newFib.ret0618.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);
            diff = Math.Abs(point - newFib.ret065.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);
            diff = Math.Abs(point - newFib.ret0786.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);
            diff = Math.Abs(point - newFib.ret1.Value); if (diff <= thresh) score += (decimal)Math.Pow((double)(thresh - diff), 4);

            return score;
        }

        private static decimal CalculateScore2(decimal point, decimal range, FibRetracement newFib, decimal factor, decimal thresh)
        {
            decimal diff = 0;
            decimal score = 0;
            //decimal thresh = 3m;
            decimal t2 = 50;

            diff = Math.Abs(point - newFib.ret0.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret0236.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret0382.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret05.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret0618.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret065.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret0786.Value); if (diff <= thresh) score += factor;
            diff = Math.Abs(point - newFib.ret1.Value); if (diff <= thresh) score += factor;

            return score;
        }

        private static bool IsNenStar(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 61.8M)
                || !isInPInt(cPre(BC, AB), 113M, 141M)
                || !isInPInt(cPre(CD, BC), 127.2M, 200M)
                || !isInPInt(cPre(AD, XA), 127.2M, 127.2M)) ret = false;

            return ret;
        }
        private static bool IsCypher(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 61.8M)
                || !isInPInt(cPre(BC, AB), 113M, 141.4M)
                || !isInPInt(cPre(CD, BC), 127.2M, 200M)
                || !isInPInt(cPre(AD, XA), 78.6M, 78.6M)) ret = false;

            return ret;
        }

        private static bool IsShark(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(BC, AB), 113M, 161.8M)
                || !isInPInt(cPre(CD, BC), 161.8M, 224M)
                || !isInPInt(cPre(AD, XA), 88.6M, 113M)) ret = false;

            return ret;
        }
        private static bool IsAntiCrab(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 27.6M, 44.6M)
                || !isInPInt(cPre(BC, AB), 112.8M, 261.8M)
                || !isInPInt(cPre(CD, BC), 161.8M, 261.8M)
                || !isInPInt(cPre(AD, XA), 61.8M, 61.8M)) ret = false;

            return ret;
        }

        private static bool IsAntiButterfly(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 61.8M)
                || !isInPInt(cPre(BC, AB), 112.8M, 261.8M)
                || !isInPInt(cPre(CD, BC), 127.2M, 127.2M)
                || !isInPInt(cPre(AD, XA), 61.8M, 78.6M)) ret = false;

            return ret;
        }
        private static bool IsAntiNenStar(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 50.0M, 78.6M)
                || !isInPInt(cPre(BC, AB), 46.7M, 70.7M)
                || !isInPInt(cPre(CD, BC), 161.8M, 261.8M)
                || !isInPInt(cPre(AD, XA), 78.6M, 78.6M)) ret = false;

            return ret;
        }
        private static bool IsAntiBat(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 61.8M)
                || !isInPInt(cPre(BC, AB), 112.8M, 261.8M)
                || !isInPInt(cPre(CD, BC), 200.0M, 261.8M)
                || !isInPInt(cPre(AD, XA), 112.8M, 112.8M)) ret = false;

            return ret;
        }
        private static bool IsAntiNewCypher(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 50.0M, 78.6M)
                || !isInPInt(cPre(BC, AB), 46.7M, 70.7M)
                || !isInPInt(cPre(CD, BC), 161.8M, 261.8M)
                || !isInPInt(cPre(AD, XA), 127.2M, 127.2M)) ret = false;

            return ret;
        }
        private static bool IsAntiGartley(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 61.8M, 78.6M)
                || !isInPInt(cPre(BC, AB), 112.8M, 261.8M)
                || !isInPInt(cPre(CD, BC), 161.8M, 161.8M)
                || !isInPInt(cPre(AD, XA), 127.2M, 127.2M)) ret = false;

            return ret;
        }
        private static bool IsNavarro200(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 78.6M)
                || !isInPInt(cPre(BC, AB), 88.6M, 112.8M)
                || !isInPInt(cPre(CD, BC), 88.6M, 361.8M)
                || !isInPInt(cPre(AD, XA), 88.6M, 112.8M)) ret = false;

            return ret;
        }
        private static bool IsGartley(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 61.8M, 61.8M)
                || !isInPInt(cPre(BC, AB), 38.2M, 88.6M)
                || !isInPInt(cPre(CD, BC), 113M, 161.8M)
                || !isInPInt(cPre(AD, XA), 78.6M, 78.6M)) ret = false;

            return ret;
        }

        private static bool IsDeepCrab(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 88.6M, 88.6M)
                || !isInPInt(cPre(BC, AB), 38.2M, 88.6M)
                || !isInPInt(cPre(CD, BC), 200M, 361.8M)
                || !isInPInt(cPre(AD, XA), 161.8M, 161.8M)) ret = false;

            return ret;
        }

        private static bool IsCrab(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 61.8M)
                || !isInPInt(cPre(BC, AB), 38.2M, 88.6M)
                || !isInPInt(cPre(CD, BC), 261.8M, 361.8M)
                || !isInPInt(cPre(AD, XA), 161.8M, 161.8M)) ret = false;

            return ret;
        }

        private static bool IsButterfly(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 78.6M, 78.6M)
                || !isInPInt(cPre(BC, AB), 38.2M, 88.6M)
                || !isInPInt(cPre(CD, BC), 161.8M, 224M)
                || !isInPInt(cPre(AD, XA), 127M, 141M)) ret = false;

            return ret;
        }

        private static bool IsAltBat(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 38.2M)
                || !isInPInt(cPre(BC, AB), 38.2M, 88.6M)
                || !isInPInt(cPre(CD, BC), 200M, 361.8M)
                || !isInPInt(cPre(AD, XA), 113M, 113M)) ret = false;

            return ret;
        }

        private static double? cPre(decimal? z1, decimal? z2) => z2 == 0 ? 0 : Mround(z1 * 100 / z2, 1);

        private static bool isInPInt(double? val, decimal low, decimal high, decimal P = 8) => low * (100 - P) / 100 <= (decimal?)val && (decimal?)val <= high * (100 + P) / 100;

        private static bool IsBat(decimal? X, decimal? A, decimal? B, decimal? C, decimal? D)
        {
            //optimize, in the FORs, check if for example B si in the other way than XA

            bool ret = true;
            decimal? XA = Math.Abs((decimal)(X - A));
            decimal? AB = Math.Abs((decimal)(A - B));
            decimal? BC = Math.Abs((decimal)(B - C));
            decimal? CD = Math.Abs((decimal)(C - D));
            decimal? AD = Math.Abs((decimal)(A - D));

            if (!isInPInt(cPre(AB, XA), 38.2M, 50M)
                || !isInPInt(cPre(BC, AB), 38.2M, 88.6M)
                || !isInPInt(cPre(CD, BC), 161.8M, 261.8M)
                || !isInPInt(cPre(AD, XA), 88.6M, 88.6M)) ret = false;

            return ret;
        }

        private static bool equalWithError(decimal A, decimal B, decimal P = 8)
        {
            var change = ((B - A) / Math.Abs(A)) * 100;
            return Math.Abs(change) <= P ? true : false;
        }

        private static double? Mround(decimal? v1, int v2 = 2)
        {
            if (v1 == null) return null;
            return Math.Round((double)v1, v2);
        }

        static decimal BTCUSDTprice = 0;
        static decimal BTCBUSDprice = 0;
        static decimal ETHUSDTprice = 0;

        static async void priceSocket()
        {
            var socketClient = new BinanceSocketClient();

            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            await socketClient.UsdFuturesApi.ExchangeData.SubscribeToMiniTickerUpdatesAsync("BTCBUSD", data =>
            {
                BTCUSDTprice = data.Data.LastPrice;
            });
        }

        public static System.Timers.Timer aTimer = new System.Timers.Timer();

        static async void webhookEvents()
        {
            while (true)
            {
                if (DateTime.UtcNow.Second == 8)
                {
                    //aTimer.Elapsed += new ElapsedEventHandler(OnCheckWebhook);
                    aTimer.Interval = 60000; //change interval for more than 1 minute!!!
                    aTimer.Enabled = true;
                    break;
                }
            }
        }

        static async Task<dynamic> GetResponse(string baseAddress)
        {
            try
            {
                using (var client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                {
                    var customerJsonString = client.DownloadString(baseAddress);
                    var deserialized = JsonConvert.DeserializeObject(custome‌​rJsonString);
                    return deserialized;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<webhookEvent> actionEvents = new List<webhookEvent>();
        public static Stopwatch stopWatch = new Stopwatch();

        private static async void OnCheckWebhook()//object source, ElapsedEventArgs e)
        {
            Console.WriteLine($"Event fired: {DateTime.UtcNow}");

            string token = "ef10573e-e65f-401c-a9f1-801c0a485843";
            string url = String.Format("https://webhook.site/token/{0}/request/latest", token);
            string deleteUrl = "https://webhook.site/token/{0}/request/{1}";

            dynamic deserialized = await GetResponse(url);

            Console.WriteLine("Event #1 finished;");

            var we = new webhookEvent();
            if (deserialized != null)
            {
                we.uuid = deserialized.uuid;
                we.content = deserialized.content;
                we.createdAt = DateTime.Parse(deserialized.created_at.ToString());

                Console.WriteLine("Event #2 finished;");

                double moneyFlow = 0;
                double cloud1 = 0;
                double cloud2 = 0;
                var dotColor = we.content.Split(' ')[0];
                var moneyFlowStr = we.content.Split('\n')[1].Split(' ')[1].Trim();
                var cloud1Str = we.content.Split('\n')[2].Split(' ')[3].Split(',')[0].Trim();
                var cloud2Str = we.content.Split('\n')[2].Split(',')[1].Trim();

                Double.TryParse(moneyFlowStr, out moneyFlow);
                Double.TryParse(cloud1Str, out cloud1);
                Double.TryParse(cloud2Str, out cloud2);

                we.dotColor = dotColor;
                we.moneyFlow = moneyFlow;
                we.cloud1 = cloud1;
                we.cloud2 = cloud2;

                Console.WriteLine("Event #3 finished;");


                actionEvents.Add(we);

                // DELETE

                //string sURL = String.Format(deleteUrl, token, we.uuid);

                //WebRequest request = WebRequest.Create(sURL);
                //request.Method = "DELETE";

                Console.WriteLine("Event #4 finished;");

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine("Event #5 finished;");

            }

            Console.WriteLine($"Event finished: {DateTime.UtcNow}");

            //don't forget to delete it after GETing it!
        }

        static List<decimal?> winTradesRsi = new List<decimal?>();
        static List<decimal?> lossTradesRsi = new List<decimal?>();

        public static bool IsBetween<T>(this T item, T start, T end) where T : IComparable
        {
            return item.CompareTo(start) >= 0 && item.CompareTo(end) <= 0;
        }

        class LagRSIResult
        {
            public DateTime Date { get; set; }

            public decimal LagRSI { get; set; }
        }

        private static decimal NZ(List<decimal> L, int index)
        {
            if (index >= 0) return L[index];
            return 0;
        }

        private static List<LagRSIResult> GetLagRSI(List<Quote> quotes) {
            var alpha = Enumerable.Repeat(0.2m, quotes.Count).ToList();
            var gamma = Enumerable.Repeat(0m, quotes.Count).ToList();
            for (int i = 0; i < quotes.Count; i++) gamma[i] = 1m - alpha[i];

            var L0 = Enumerable.Repeat(0m, quotes.Count).ToList();
            var L1 = Enumerable.Repeat(0m, quotes.Count).ToList();
            var L2 = Enumerable.Repeat(0m, quotes.Count).ToList();
            var L3 = Enumerable.Repeat(0m, quotes.Count).ToList();
            var cu = Enumerable.Repeat(0m, quotes.Count).ToList();
            var cd = Enumerable.Repeat(0m, quotes.Count).ToList();
            var temp = Enumerable.Repeat(0m, quotes.Count).ToList();
            var LaRSI = new List<LagRSIResult>();


            for (int i = 0; i < quotes.Count; i++)
                L0[i] = (1 - gamma[i]) * quotes[i].Close + gamma[i] * NZ(L0, i - 1);

            for (int i = 0; i < quotes.Count; i++)
                L1[i] = gamma[i] * -1 * L0[i] + (i - 1 >= 0 ? L0[i - 1] : 0) + gamma[i] * NZ(L1, i - 1);

            for (int i = 0; i < quotes.Count; i++)
                L2[i] = gamma[i] * -1 * L1[i] + (i - 1 >= 0 ? L1[i - 1] : 0) + gamma[i] * NZ(L2, i - 1);

            for (int i = 0; i < quotes.Count; i++)
                L3[i] = gamma[i] * -1 * L2[i] + (i - 1 >= 0 ? L2[i - 1] : 0) + gamma[i] * NZ(L3, i - 1);


            for (int i = 0; i < quotes.Count; i++)
                cu[i] = (L0[i] > L1[i] ? L0[i] - L1[i] : 0) + (L1[i] > L2[i] ? L1[i] - L2[i] : 0) + (L2[i] > L3[i] ? L2[i] - L3[i] : 0);

            for (int i = 0; i < quotes.Count; i++)
                cd[i] = (L0[i] < L1[i] ? L1[i] - L0[i] : 0) + (L1[i] < L2[i] ? L2[i] - L1[i] : 0) + (L2[i] < L3[i] ? L3[i] - L2[i] : 0);

            for (int i = 0; i < quotes.Count; i++)
                temp[i] = cu[i] + cd[i] == 0 ? -1 : cu[i] + cd[i];

            for (int i = 0; i < quotes.Count; i++) {
                LaRSI.Add(new LagRSIResult());
                LaRSI[i].LagRSI = (temp[i] == -1 ? 0 : cu[i] / temp[i]) * 100m;
                LaRSI[i].Date = quotes[i].Date;
            }
            return LaRSI;
        }

        static void ZigZagCalculator(List<Quote> candles, int Depth = 10, int Deviation = 5, int BackStep = 2)
        {
            var zList = new List<zzResult>();
            foreach (var candle in candles)
            {
                zList.Add(new zzResult
                {
                    buffer = candle
                ,
                    highBuffer = 0
                ,
                    lowBuffer = 0
                ,
                    zigZagBuffer = 0
                ,
                    date = candle.Date
                });
            }

            int[] DirectionBuffer = new int[candles.Count];
            int[] LastHighBarBuffer = new int[candles.Count];
            int[] LastLowBarBuffer = new int[candles.Count];
            double[] ZigZagBuffer = new double[candles.Count];
            int start = 12;

            for (int i = start; i < candles.Count; i++)
            {
                DirectionBuffer[i] = DirectionBuffer[i - 1];
                int ps = i - Depth + 1;
                decimal hb = candles.Skip(ps + 1).Take(Depth).Max(it => it.High);
                decimal lb = candles.Skip(ps + 1).Take(Depth).Max(it => it.Low);

                if (hb == candles[i].High && lb != candles[i].High)
                {
                    DirectionBuffer[i] = 1;
                } else if (lb == candles[i].Low && hb != candles[i].Low)
                {
                    DirectionBuffer[i] = -1;
                }

                LastHighBarBuffer[i] = LastHighBarBuffer[i - 1];
                LastLowBarBuffer[i] = LastLowBarBuffer[i - 1];
                ZigZagBuffer[i] = 0;
                zList[i].zigZagBuffer = 0;

                switch ((int)DirectionBuffer[i])
                {
                    case 1:
                        switch ((int)DirectionBuffer[i - 1])
                        {
                            case 1:
                                if (candles[i].High > candles[LastHighBarBuffer[i]].High)
                                {
                                    ZigZagBuffer[(int)LastHighBarBuffer[i]] = 0;
                                    zList[(int)LastHighBarBuffer[i]].zigZagBuffer = 0;
                                    zList[i].type = "";

                                    ZigZagBuffer[i] = (double)candles[i].High;
                                    zList[i].zigZagBuffer = candles[i].High;
                                    zList[i].type = "H";
                                    LastHighBarBuffer[i] = i;
                                }
                                break;
                            case -1:
                                ZigZagBuffer[i] = (double)candles[i].High;
                                LastHighBarBuffer[i] = i;
                                break;
                        }
                        break;
                    case -1:
                        switch ((int)DirectionBuffer[i - 1])
                        {
                            case -1:
                                if (candles[i].Low < candles[LastLowBarBuffer[i]].Low)
                                {
                                    ZigZagBuffer[LastLowBarBuffer[i]] = 0;
                                    zList[(int)LastLowBarBuffer[i]].zigZagBuffer = 0;
                                    zList[i].type = "";

                                    ZigZagBuffer[i] = (double)candles[i].Low;
                                    zList[i].zigZagBuffer = candles[i].Low;
                                    zList[i].type = "L";
                                    LastLowBarBuffer[i] = i;
                                }
                                break;
                            case 1:
                                ZigZagBuffer[i] = (double)candles[i].Low;
                                LastLowBarBuffer[i] = i;
                                break;
                        }
                        break;
                }
            }

            return;
        }

        static void NewZZ(List<zzResult> zList, List<Quote> candles, int Depth = 10, decimal Deviation = 5, int BackStep = 2)
        {
            foreach (var candle in candles)
            {
                zList.Add(new zzResult
                {
                    buffer = candle
                ,
                    highBuffer = 0
                ,
                    lowBuffer = 0
                ,
                    zigZagBuffer = 0
                ,
                    date = candle.Date
                });
            }

            /*zList.Insert(0, new zzResult
            {
                buffer = candle
            ,
                highBuffer = 0
            ,
                lowBuffer = 0
            ,
                zigZagBuffer = 0
            });*/

            decimal lastHigh = 0;
            decimal lastLow = 0;

            for (var shift = zList.Count - 3; shift >= 0; shift--)
            {
                //--- low
                var val = zList.Skip(shift).Take(Depth).Min(it => it.buffer.Low);
                if (val == lastLow)
                {
                    val = 0.0m;
                }
                else
                {
                    lastLow = val;
                    if (zList[shift].buffer.Low - val > Deviation * 0.1m)
                    {
                        val = 0.0m;
                    }
                    else
                    {
                        for (var back = 1; back <= BackStep; back++)
                        {
                            var res = zList[shift + back].lowBuffer;
                            if (res != 0 && res > val)
                            {
                                zList[shift + back].lowBuffer = 0.0m;
                            }
                        }
                    }
                }
                zList[shift].lowBuffer = val;

                //--- high
                val = zList.Skip(shift).Take(Depth).Max(it => it.buffer.High);
                if (val == lastHigh)
                {
                    val = 0.0m;
                }
                else
                {
                    lastHigh = val;
                    if (val - zList[shift].buffer.High > Deviation * 0.1m)
                    {
                        val = 0.0m;
                    }
                    else
                    {
                        for (var back = 1; back <= BackStep; back++)
                        {
                            var res = zList[shift + back].highBuffer;
                            if (res != 0 && res < val)
                            {
                                zList[shift + back].highBuffer = 0.0m;
                            }
                        }
                    }
                }
                zList[shift].highBuffer = val;
            }

            //--- final cutting
            lastHigh = -1;
            lastLow = -1;
            var lastHighPos = -1;
            var lastLowPos = -1;

            for (var shift = zList.Count - 3; shift >= 0; shift--)
            {
                var curLow = zList[shift].lowBuffer;
                var curHigh = zList[shift].highBuffer;

                if (curLow == 0 && curHigh == 0) continue;

                //---
                if (curHigh != 0)
                {
                    if (lastHigh > 0)
                    {
                        if (lastHigh < curHigh)
                        {
                            zList[lastHighPos].highBuffer = 0;
                        }
                        else
                        {
                            zList[shift].highBuffer = 0;
                        }
                    }
                    //---
                    if (lastHigh < curHigh || lastHigh < 0)
                    {
                        lastHigh = curHigh;
                        lastHighPos = shift;
                    }
                    lastLow = -1;
                }

                //---
                if (curLow != 0)
                {
                    if (lastLow > 0)
                    {
                        if (lastLow > curLow)
                        {
                            zList[lastLowPos].lowBuffer = 0;
                        }
                        else
                        {
                            zList[shift].lowBuffer = 0;
                        }
                    }
                    //---
                    if (curLow < lastLow || lastLow < 0)
                    {
                        lastLow = curLow;
                        lastLowPos = shift;
                    }
                    lastHigh = -1;
                }
            }

            for (var shift = zList.Count - 1; shift >= 0; shift--)
            {
                if (shift >= zList.Count - Depth)
                {
                    zList[shift].lowBuffer = 0.0m;
                }
                else
                {
                    var res = zList[shift].highBuffer;
                    if (res != 0.0m)
                    {
                        zList[shift].highBuffer = res;
                    }
                }

                zList[shift].zigZagBuffer = zList[shift].lowBuffer > 0 ? zList[shift].lowBuffer : zList[shift].highBuffer;
                zList[shift].type = zList[shift].lowBuffer > 0 ? "L" : "H";
            }
        }

        private static decimal getPivotHighResistance(List<Quote> quotes, int leftBars, int rightBars)
        {
            for (int i = quotes.Count() - 1 - rightBars; i >= leftBars; i--)
            {
                bool ok = true;
                for (int j = i - leftBars; j <= i + rightBars; j++)
                {
                    if (quotes[i].High < quotes[j].High)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    return quotes[i].High;
                }
            }
            return 0m;
        }

        private static decimal getPivotLowSupport(List<Quote> quotes, int leftBars, int rightBars)
        {
            for (int i = quotes.Count() - 1 - rightBars; i >= leftBars; i--)
            {
                bool ok = true;
                for (int j = i - leftBars; j <= i + rightBars; j++)
                {
                    if (quotes[i].Low > quotes[j].Low)
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    return quotes[i].Low;
                }
            }
            return 0m;
        }

        private static bool checkLastCandlesEMA(List<IBinanceKline> candles, List<EmaResult> ema, int direction)
        {
            if (direction == 1) //test if candles are above EMA
            {
                for (int i = 0; i < candles.Count(); i++)
                {
                    if (false)//!(candles[i].Low > ema[i].Ema))
                        return false;
                }
            } else //test if candles are below EMA
            {
                for (int i = 0; i < candles.Count(); i++)
                {
                    if (false)//!(candles[i].High < ema[i].Ema))
                        return false;
                }
            }

            return true;
        }

        private static IEnumerable<Quote> GetClosesFromLsma(List<EpmaResult> lsma)
        {

            var qq = lsma.Select(it => new Quote()
            {
                Close = (decimal)(it.Epma != null ? it.Epma : 0),
                Date = it.Date,
                High = 0,
                Low = 0,
                Open = 0,
                Volume = 0
            });
            return qq;
        }

        private static decimal? calculatePnL(decimal? entryPrice, decimal? exitPrice, decimal? quantity, int leverage, int direction) //direction 1: Long, direction -1: Short
        {
            decimal? PnL = (exitPrice - entryPrice) * direction * quantity;
            decimal? fees = quantity * exitPrice * fee / 100;
            return PnL - fee;
        }

        private static decimal? calculatePnLNoFees(decimal? entryPrice, decimal? exitPrice, decimal? quantity, int leverage, int direction) //direction 1: Long, direction -1: Short
        {
            decimal? PnL = (exitPrice - entryPrice) * direction * quantity;
            //decimal? fees = quantity * exitPrice * fee / 100;
            return PnL;
        }

        private static decimal? calculateFees(decimal? exitPrice, decimal? quantity, decimal? feeLevel)
        {
            return quantity * exitPrice * feeLevel / 100;
        }


        private static bool macdCrossesAbove(List<MacdResult> macd)
        {
            //CAND VREI SA FACI SHORT, SA FIE AL 2-lea CROSS ASTFEL INCAT SIGNAL LINE > MACD LINE
            //CAND VREI SA FACI LONG, SA FIE AL 2-lea CROSS ASTFEL INCAT MACD LINE > SIGNAL LINE

            int index = findPreviousCross(macd, macd.Count - 2, 1);
            int index2 = -1;
            if (index == macd.Count - 2)
            {
                index2 = findPreviousCross(macd, index, 1); 
            }

            if (index2 != -1 && macd[index].Macd > macd[index].Signal 
                && macd[index].Macd - macd[index].Signal > 10) return true;
            return false;
        }

        private static bool macdCrossesBelow(List<MacdResult> macd)
        {
            //CAND VREI SA FACI SHORT, SA FIE AL 2-lea CROSS ASTFEL INCAT SIGNAL LINE > MACD LINE
            //CAND VREI SA FACI LONG, SA FIE AL 2-lea CROSS ASTFEL INCAT MACD LINE > SIGNAL LINE

            //OPTIMIZARE CU MACD LINE SLOPE ?? --------------------------------------------------------------------------<<<<<<<<<<<<<<<<<<<<<

            int index = findPreviousCross(macd, macd.Count - 2, -1);
            int index2 = -1;
            if (index == macd.Count - 2)
            {
                index2 = findPreviousCross(macd, index - 1, -1);
            }

            if (index2 != -1 && macd[index].Macd < macd[index].Signal
                && macd[index].Signal - macd[index].Macd > 10) return true;
            return false;
        }

        private static int findPreviousCross(List<MacdResult> macd, int v, int ch)
        {
            for (int i = v; macd[i].Macd * ch > 0 && macd[i].Signal * ch > 0 && i > 0; i--) //poate aici sa ai grija ca ambele, si i si i - 1 sa aiba ambele Macd, Signal > 0 ??
            {
                if ((macd[i].Macd > macd[i].Signal && macd[i - 1].Macd < macd[i - 1].Signal)
                    || (macd[i].Macd < macd[i].Signal && macd[i - 1].Macd > macd[i - 1].Signal))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool stochRsiAboveCross(List<StochRsiResult> stochRsi)
        {
            if (stochRsi[stochRsi.Count - 2].Signal > stochRsi[stochRsi.Count - 2].StochRsi 
                && stochRsi[stochRsi.Count - 3].Signal < stochRsi[stochRsi.Count - 3].StochRsi
                && stochRsi[stochRsi.Count - 2].Signal > 80
                && stochRsi[stochRsi.Count - 3].Signal > 80
                && stochRsi[stochRsi.Count - 2].StochRsi > 80
                && stochRsi[stochRsi.Count - 3].StochRsi > 80) return true;
            if (stochRsi[stochRsi.Count - 2].Signal < stochRsi[stochRsi.Count - 2].StochRsi 
                && stochRsi[stochRsi.Count - 3].Signal > stochRsi[stochRsi.Count - 3].StochRsi
                && stochRsi[stochRsi.Count - 2].Signal > 80
                && stochRsi[stochRsi.Count - 3].Signal > 80
                && stochRsi[stochRsi.Count - 2].StochRsi > 80
                && stochRsi[stochRsi.Count - 3].StochRsi > 80) return true;

            return false;
        }

        private static bool stochRsiBelowCross(List<StochRsiResult> stochRsi)
        {
            if (stochRsi[stochRsi.Count - 2].Signal > stochRsi[stochRsi.Count - 2].StochRsi
                && stochRsi[stochRsi.Count - 3].Signal < stochRsi[stochRsi.Count - 3].StochRsi
                && stochRsi[stochRsi.Count - 2].Signal < 20
                && stochRsi[stochRsi.Count - 3].Signal < 20
                && stochRsi[stochRsi.Count - 2].StochRsi < 20
                && stochRsi[stochRsi.Count - 3].StochRsi < 20) return true;
            if (stochRsi[stochRsi.Count - 2].Signal < stochRsi[stochRsi.Count - 2].StochRsi
                && stochRsi[stochRsi.Count - 3].Signal > stochRsi[stochRsi.Count - 3].StochRsi
                && stochRsi[stochRsi.Count - 2].Signal < 20
                && stochRsi[stochRsi.Count - 3].Signal < 20
                && stochRsi[stochRsi.Count - 2].StochRsi < 20
                && stochRsi[stochRsi.Count - 3].StochRsi < 20) return true;

            return false;
        }

        async static Task<List<IBinanceKline>> GetAllKlinesInterval(string pair, KlineInterval inter, DateTime dateTime, DateTime now)
        {
            var client = new BinanceRestClient(options => {
                // Options can be configured here, for example:
                options.ApiCredentials = new ApiCredentials("1n2SpbTN1V60T41ul5cdiayDmEkdgPIlt97r0IZRCPeMxx4LBwCWsMbii9YIi2M2", "at5urMrhJjA9L2IN2xZZdFI2jn3NuVKm14YNS4VJHFh2QOWkOlcGr37QEJGG29bf");
                options.Environment = BinanceEnvironment.Live;
            });

            var startTime = dateTime;
            List<IBinanceKline> ret = new List<IBinanceKline>();

            while (true)
            {
                var aux = (await client.UsdFuturesApi.ExchangeData.GetKlinesAsync(pair, inter, startTime: startTime, limit: 1500)).Data;
                if (aux.Count() == 0) break;
                ret.AddRange(aux);
                startTime = ret.Last().CloseTime.AddSeconds(0);
            }

            return ret;
        }

        private static double CalculateSlope(Point start, Point arrival)
        {
            return (arrival.Y - start.Y) / (arrival.X - start.X);
        }

        private static double macdNegAvg(List<MacdResult> macd, int timeFrame = 50)
        {
            var period = macd.Skip(Math.Max(0, macd.Count() - timeFrame)).Where(it => it.Macd < 0).ToList();

            if (period.Count < 10) return -100;
            return (double)period.Average(it => it.Histogram);
        }

        private static double macdPosAvg(List<MacdResult> macd, int timeFrame = 50)
        {
            var period = macd.Skip(Math.Max(0, macd.Count() - timeFrame)).Where(it => it.Macd > 0).ToList();

            if (period.Count < 10) return 100;
            return (double)period.Average(it => it.Macd);
        }

        private static IEnumerable<Quote> GetQuoteFromKlines(List<Binance.Net.Interfaces.IBinanceKline> lineData)
        {
            var qq = lineData.Select(it => new Quote()
            {
                Close = it.ClosePrice,
                High = it.HighPrice,
                Date = it.CloseTime,
                Low = it.LowPrice,
                Open = it.OpenPrice,
                Volume = it.Volume
            });
            return qq;
        }

        private static IEnumerable<Quote> GetQuoteFromKlinesStartDay(List<Binance.Net.Interfaces.IBinanceKline> lineData, DateTime startDay)
        {
            var qq = lineData.Where(it => it.OpenTime >= startDay.AddMinutes(-1)).Select(it => new Quote()
            {
                Close = it.ClosePrice,
                High = it.HighPrice,
                Date = it.CloseTime,
                Low = it.LowPrice,
                Open = it.OpenPrice,
                Volume = it.Volume
            });
            return qq;
        }
    }
}
