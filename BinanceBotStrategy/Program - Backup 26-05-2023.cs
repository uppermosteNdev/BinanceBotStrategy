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
//using bsk = Binance.Net.Objects.Spot.MarketData.BinanceSpotKline;
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

namespace BinanceBotStrategy
{
    static class Program
    {
        class futureEntry
        {
            public decimal? entryPrice { get; set; }
            public decimal? ret0 { get; set; }
            public decimal? ret0236 { get; set; }
            public decimal? ret0382 {get; set; }
            public decimal? ret05 { get; set; }
            public decimal? ret0618 { get; set; }
            public decimal? ret0786 { get; set; }
            public decimal? ret1 { get; set; }
            public decimal? ret1618 { get; set; }
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

        class binanceOpenFuture
        {
            public DateTime startTime { get; set; }
            public decimal price { get; set; }
            public decimal targetPrice { get; set; }
            public decimal? stopLimitPrice { get; set; }
            public decimal riskRatio { get; set; }
            public string type { get; set; }
            public long id { get; set; }
        }

        class insaneEntry
        {
            public DateTime time { get; set; }
            public string type { get; set; }
        }


        class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
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
        public static decimal investment = 0.12M;//0.018M; //$300

        async static Task Main(string[] args)
        {
            Thread th = new Thread(priceSocket);
            th.Start();

            Thread vpt = new Thread(volumeProfileCalculation);
            //vpt.Start();

            //Thread who = new Thread(GetHooks);
            //who.Start();

            var client = new BinanceClient(new BinanceClientOptions
            {
                UsdFuturesApiOptions = new BinanceApiClientOptions
                {
                    ApiCredentials = new BinanceApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                    BaseAddress = "https://fapi.binance.com"
                }
            });

            //var ttt = await client.FuturesUsdt.GetIncomeHistoryAsync();

            //var hh = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();

            //result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCUSDT", OrderSide.Buy, OrderType.Market, 0.001M);
            //var closeOpenPosition = client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCUSDT").Result;

            //var resp = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.StopMarket, stopPrice: 39000, quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-6), DateTime.UtcNow);
            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
            //var lineData2 = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-5), DateTime.UtcNow);

            //volumeProfilePOCCalculation(lineData);
            //var lineData = (await client.FuturesUsdt.Market.GetKlinesAsync("BTCUSDT", KlineInterval.OneMinute, limit: 1500)).Data.ToList(); //startTime: DateTime.Now.AddDays(0), 
            //var lineData2 = (await client.FuturesUsdt.Market.GetKlinesAsync("BTCUSDT", KlineInterval.FiveMinutes, endTime: DateTime.Now.AddMinutes(0), limit: 1500)).Data.ToList(); //startTime: DateTime.Now.AddDays(0), 

            //var vpData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneHour, DateTime.UtcNow.AddDays(-21), DateTime.UtcNow);//"BTCUSDT"


            //decimal? profit = backTestStrat1(lineData);
            //decimal? profit = backTestStrat2(lineData);
            //decimal? profit = backTestStrat3(lineData); //EMA50, EMA200, MACD, RSI ||||||||| GOOD!
            //decimal? profit = backTestStrat4(lineData);
            //decimal? profit = backTestStrat5(lineData); //MACD, CMF
            //decimal? profit = backTestStrat6(lineData); //CHANDELIER EXIT + ZLSMA 92% GOA over 6 months
            //decimal? profit = backTestStrat7(lineData);//ADX + RSI 70/30 + EMA 50
            //decimal? profit = backTestStrat8(lineData); //supertrend + EMA 20 + TREND EMA 200 larger timeframe
            //decimal? profit = backTestStrat9(lineData); //3x SuperTrend
            //await liveStrat6(); //CHANDELIER EXIT + ZLSMA
            //await liveStrat7(); //VWAP + EMA + RSI
            //await liveStrat8();
            //await liveStrat8();
            //await volumeProfileCalculation();
            //decimal? profit = backTestStrat10(lineData, vpData, true);
            //await liveStrat10HarmonicPatterns();
            //decimal? profit = backTestStrat11(lineData, lineData2);
            //decimal? profit = backTestStrat12(lineData, lineData2);
            //decimal? profit = backTestStrat13(lineData, lineData2);
            //decimal? profit = backTestStrat14(lineData);
            //decimal? profit = backTestStrat15(lineData, lineData2);
            await liveStrat10TheOne();

            //strat 4 divergence indicator: cand a aparut ultimul candle (count - 1), H Bull a aparut la (count - 6), cu 5 inapoi
            //How far back on the rsi can you check for divergence? I noticed I can find hidden divergence everywhere if I look far back enough so I do not think this strategy is good for beginners 
            //due to the subjectiveness of it.On another note, adding the 50ema helps tremendouslyyyy especially for people who can’t easily tell if the market is currently trending or not.
            //Not taking trades when price is between the 50 and 200 cuts out so many losses.

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

            /*var socketClient = new BinanceSocketClient();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCUSDT", KlineInterval.OneMinute, data =>
            {
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
                if (existingKline == null)
                {
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    existingKline = new Binance.Net.Objects.Spot.MarketData.BinanceSpotKline()
                    {
                        CloseTime = data.Data.Data.CloseTime,
                        OpenTime = data.Data.Data.OpenTime
                    };

                    lineData.Add(existingKline);
                    Console.WriteLine($"Added new kline: {existingKline.OpenTime}-{existingKline.CloseTime}");
                }

                // Update the data of the object
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                if (candleStarted)
                {
                    //testOpenPositions(); //if previous open position has hit a stop limit or a target (candle range)
                    candleStarted = false;

                    // Calculate EMA & Williams Fractal
                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineData);
                    ema20 = quotes.GetEma(20).ToList();
                    ema50 = quotes.GetEma(50).ToList();
                    ema100 = quotes.GetEma(100).ToList();
                    adx = quotes.GetAdx(14).ToList();
                    willf = quotes.GetFractal(2).ToList();

                    //Console.WriteLine($"Bull: Last 3 Willf: {willf[willf.Count - 1].FractalBull}, {willf[willf.Count - 2].FractalBull}, {willf[willf.Count - 3].FractalBull}");
                    //Console.WriteLine($"Bear: Last 3 Willf: {willf[willf.Count - 1].FractalBear}, {willf[willf.Count - 2].FractalBear}, {willf[willf.Count - 3].FractalBear}");

                    // LONG check
                    // See if 2 bars ago we have Fractal
                    if (willf[willf.Count - 4].FractalBull != null)
                    {
                        //see if ema20 > ema50 > ema 100
                        if (ema20[ema20.Count - 5].Ema > ema50[ema50.Count - 5].Ema && ema50[ema50.Count - 5].Ema > ema100[ema100.Count - 5].Ema)
                        {
                            if (ema20[ema20.Count - 5].Ema > willf[willf.Count - 4].FractalBull && willf[willf.Count - 4].FractalBull > ema100[ema100.Count - 5].Ema)
                            {
                                Console.WriteLine($"LONG SIGNAL!  WFractal Bull: {willf[willf.Count - 4].FractalBull} | ADX value: {adx.Last().Adx}");
                                Console.WriteLine("Test buying long ...");
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineData.Last().OpenTime,
                                    price = lineData.Last().Open,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = (ema20[ema20.Count - 5].Ema > willf[willf.Count - 4].FractalBull) && (willf[willf.Count - 4].FractalBull > ema50[ema50.Count - 5].Ema) ? ema50[ema50.Count - 1].Ema : ema100[ema100.Count - 1].Ema,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            } else
                            {
                                Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                            }
                        } else
                        {
                            Console.WriteLine("Not in order for long");
                        }
                    }

                    if (willf[willf.Count - 4].FractalBear != null)
                    {
                        //see if ema20 > ema50 > ema 100
                        if (ema20[ema20.Count - 5].Ema < ema50[ema50.Count - 5].Ema && ema50[ema50.Count - 5].Ema < ema100[ema100.Count - 5].Ema)
                        {
                            if (ema20[ema20.Count - 5].Ema < willf[willf.Count - 4].FractalBear && willf[willf.Count - 4].FractalBear < ema100[ema100.Count - 5].Ema)
                            {
                                Console.WriteLine($"SHORT SIGNAL!  WFractal Bear: {willf[willf.Count - 4].FractalBear} | ADX value: {adx.Last().Adx}");
                                Console.WriteLine("Test buying short ...");
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineData.Last().OpenTime,
                                    price = lineData.Last().Open,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = (ema20[ema20.Count - 5].Ema < willf[willf.Count - 4].FractalBull) && (willf[willf.Count - 4].FractalBull < ema50[ema50.Count - 5].Ema) ? ema50[ema50.Count - 1].Ema : ema100[ema100.Count - 1].Ema,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                            else
                            {
                                Console.WriteLine($"Fractal not between ema100 & ema20 | ADX Value: {adx.Last().Adx}");
                            }
                        } else
                        {
                            Console.WriteLine("Not in order for short");
                        }
                    }
                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close}");
            });*/

            Console.ReadLine();
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

        private static async Task liveStrat8()
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://api.binance.com",
            });

            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);//"BTCUSDT"
            lineData = lineData.Skip(Math.Max(0, lineData.Count() - 1450)).ToList(); //take only last 1450 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();

            var socketClient = new BinanceSocketClient();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCBUSD", KlineInterval.OneMinute, async data =>
            {
                //Console.WriteLine($"BTCUSDT PRICE: # {BTCUSDTprice}");
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
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
                    var lineSim = lineData.ToList();
                    lineData.Add(existingKline);
                    //Console.WriteLine($"count lineSim = {lineSim.Count}, count lineData = {lineData.Count}");
                    lineData.RemoveAt(0); //when adding a new Kline, remove the first in the list so it stays the same length
                    Console.WriteLine($"Added new kline: {existingKline.OpenTime}-{existingKline.CloseTime} | Number of Klines: {lineData.Count}");

                    if (openPositions.Count == 1)
                    {
                        var childOP2 = openPositions.First();
                        Console.WriteLine($"New StopLoss: {childOP2.stopLimitPrice}");
                    }

                    IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 450)).ToList());
                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                    if (quotes.Count() >= 140)// && !(lineSim.Last().OpenTime.Hour >= 0 && lineSim.Last().OpenTime.Hour <= 1 && lineSim.Last().OpenTime.Minute >= 0 && lineSim.Last().OpenTime.Minute <= 5))
                    {

                        //var ema20 = quotesEma.GetEma(20).ToList();
                        var ema50 = quotes.GetEma(50).ToList();
                        var ema200 = quotes.GetEma(200).ToList();
                        var atr3 = quotes.GetAtr(3).ToList();

                        var realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                        Console.WriteLine($"Real open positions First Entry Price: {(realOpenPos.Data != null ? realOpenPos.Data.First().EntryPrice : -1)}");

                        var currClose = lineSim[lineSim.Count - 1].Close;
                        Console.WriteLine($"Current price: {BTCUSDTprice} | realOpenPos: {realOpenPos.Success}");

                        var taskDelay = Task.Delay(11500);
                        await taskDelay;

                        var lastHookGreen = actionEvents.LastOrDefault(it => DateTime.UtcNow <= it.createdAt.AddMinutes(3).AddSeconds(30) && it.dotColor == "Green");
                        var lastHookRed = actionEvents.LastOrDefault(it => DateTime.UtcNow <= it.createdAt.AddMinutes(3).AddSeconds(30) && it.dotColor == "Red");

                        if (lastHookGreen != null)
                        {
                            Console.WriteLine($"GREEN: {lastHookGreen.createdAt.Minute} --- NOW: {DateTime.UtcNow.Minute}");
                        }

                        if (lastHookRed != null)
                        {
                            Console.WriteLine($"  RED: {lastHookRed.createdAt.Minute} --- NOW: {DateTime.UtcNow.Minute}");
                        }

                        //PROCESS CONTENT

                        /*string content = "";
                        string dotColor = "";
                        double moneyFlow = 0, cloud1 = 0, cloud2 = 0;

                        if (lastHook != null)
                        {
                            //content = lastHook.content;
                            dotColor = lastHook.dotColor;
                            moneyFlow = lastHook.moneyFlow;
                            cloud1 = lastHook.cloud1;
                            cloud2 = lastHook.cloud2;
                        }*/

                        //var atrUpper = (double)(lineSim.Last().Close + (atr3.Last().Atr * 2.5));
                        //var atrLower = (double)(lineSim.Last().Close - (atr3.Last().Atr * 2.5));
                        //var atrMult = atr3.Last().Atr * 4M;

                        if (realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0
                            && lastHookGreen != null && lastHookGreen.createdAt.Minute == DateTime.UtcNow.Minute) /// MINUTE == MINUTE
                        {
                            // check for LONG POSITION

                            //LONG CHECK
                            if ((lastHookGreen.cloud1 < 0 || lastHookGreen.cloud2 < 0) &&
                                ema50.Last().Ema > ema200.Last().Ema
                                //&& lineSim.Last().Close >= ema200.Last().Ema
                                //&& lineSim.Last().Open <= (ema50.Last().Ema + 25)
                                )
                            {
                                Console.WriteLine($"LONG SIGNAL!");
                                Console.Write($"Content is: {lastHookGreen.content}");
                                Console.WriteLine($"Sending long ORDER request ... Current price is: {BTCUSDTprice}");

                                var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.Market, quantity: investment);
                                /*var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                                                                , OrderType.TrailingStopMarket
                                                                                , quantity: investment
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark
                                                                                , callbackRate: 0.1M);*/

                                //var result = false;

                                if (result.Success)//false
                                {
                                    realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                                    var position = realOpenPos.Data.First();

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = DateTime.UtcNow,
                                        price = position.EntryPrice,
                                        riskRatio = (decimal)1.5,
                                        stopLimitPrice = position.EntryPrice * 0.99M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                        type = "long",
                                    });

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"LONG OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    //Console.Write($"LONG OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                                    try
                                    {
                                        var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                                                                , OrderType.TrailingStopMarket
                                                                                , quantity: investment
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark
                                                                                , callbackRate: 0.1M);
                                        /*
                                        var tt0 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                            , OrderSide.Sell
                                                                                            , OrderType.Limit
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , price: (decimal?)Math.Round((double)(position.EntryPrice + (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                            , workingType: WorkingType.Mark);

                                        var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                    , OrderSide.Sell
                                                                                    , OrderType.StopMarket
                                                                                    , quantity: investment
                                                                                    , timeInForce: TimeInForce.GoodTillCancel
                                                                                    , reduceOnly: true
                                                                                    , stopPrice: (decimal?)Math.Round((double)(position.EntryPrice - atrMult), 1)   //0.4% in the red
                                                                                    , workingType: WorkingType.Mark);
                                        */
                                    } catch (Exception ex)
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

                        if (realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0
                            && lastHookRed != null && lastHookRed.createdAt.Minute == DateTime.UtcNow.Minute)
                        {
                            // check for SHORT POSITION
                            //Console.WriteLine($"SIGNAL DATA: dir: {dir}, dirPrev: {dirPrev}, shortvs: {shortvs}, shortvs1: {shortvs1}, longvs: {longvs}, longvs1: {longvs1}");
                            if ((lastHookRed.cloud1 > 0 || lastHookRed.cloud2 > 0) &&
                                ema50.Last().Ema < ema200.Last().Ema
                                //&& lineSim.Last().Close <= ema200.Last().Ema
                                //&& lineSim.Last().Open >= ema50.Last().Ema - 25
                                )
                            {
                                Console.WriteLine($"SHORT SIGNAL!");
                                Console.Write($"Content is: {lastHookRed.content}");
                                Console.Write($"Sending short position request ... Current price is: {BTCUSDTprice}");

                                var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.Market, quantity: investment);

                                /*var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                                , OrderType.TrailingStopMarket
                                                                                , quantity: investment
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark
                                                                                , callbackRate: 0.1M);*/

                                if (result.Success)//false
                                {
                                    realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                                    var position = realOpenPos.Data.First();

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = DateTime.UtcNow,
                                        price = position.EntryPrice,
                                        riskRatio = (decimal)1.5,
                                        stopLimitPrice = position.EntryPrice * 1.01M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                        type = "short",
                                    });

                                    try
                                    {
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine($"SHORT OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        //Console.Write($"SHORT OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");

                                        var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                                , OrderType.TrailingStopMarket
                                                                                , quantity: investment
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark
                                                                                , callbackRate: 0.1M);

                                        /*var tt0 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                            , OrderSide.Buy
                                                                                            , OrderType.Limit
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , price: (decimal?)Math.Round((double)(position.EntryPrice - (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                            , workingType: WorkingType.Mark);

                                        var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                            , OrderSide.Buy
                                                                                            , OrderType.StopMarket
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , stopPrice: (decimal?)Math.Round((double)(position.EntryPrice + atrMult), 1)//Math.Round(position.EntryPrice * 1.00133M, 1)   //0.4% in the red
                                                                                            , workingType: WorkingType.Mark);*/
                                    } catch (Exception ex)
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

                if (openPositions.Count == 50)
                {
                    var childOP2 = openPositions.First();
                    if (childOP2.type == "long")
                    {
                        if (BTCUSDTprice > childOP2.price * 1.001M && childOP2.stopLimitPrice < childOP2.price)
                        {
                            childOP2.stopLimitPrice = Math.Round(childOP2.price, 2);
                        }
                        childOP2.stopLimitPrice = (decimal?)Math.Round(Math.Max((double)childOP2.stopLimitPrice, (double)BTCUSDTprice * 0.998), 2);
                    }
                    if (childOP2.type == "short")
                    {
                        if (BTCUSDTprice < childOP2.price * 0.999M && childOP2.stopLimitPrice > childOP2.price)
                        {
                            childOP2.stopLimitPrice = Math.Round(childOP2.price, 2);
                        }
                        childOP2.stopLimitPrice = (decimal?)Math.Round(Math.Min((double)childOP2.stopLimitPrice, (double)BTCUSDTprice * 1.002), 2);
                    }
                    //Console.WriteLine($"New StopLoss: {childOP.stopLimitPrice}");

                    //update stoplimit:
                    if (false)
                    {
                        secondSwitch10 = DateTime.UtcNow.Second;
                        var cancelAllOrders = client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD").Result; //cancel all orders ///// TRY IT WITH TRAILING STOP AUTOMATIC FROM API
                        //dynamic newOrder = null;
                        if (childOP2.type == "long")
                        {
                            var newOrder = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.StopMarket, stopPrice: (decimal?)Mround(childOP2.stopLimitPrice, 1), quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;
                            Console.WriteLine($"long newOrder triggered: success: {newOrder.Success} success false: {newOrder.Error}");
                        }
                        if (childOP2.type == "short")
                        {
                            var newOrder = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.StopMarket, stopPrice: (decimal?)Mround(childOP2.stopLimitPrice, 1), quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;
                            Console.WriteLine($"short newOrder triggered: success: {newOrder.Success} success false: {newOrder.Error}");
                        }
                    }

                    if (false)
                    {
                        secondSwitch30 = DateTime.UtcNow.Second;
                        var pos = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                        if (pos.Quantity == 0)
                        {
                            openPositions.Remove(childOP2);
                        }
                    }
                }

                //CLEAR OPEN POSITIONS AFTER BINANCE POSITIONS CLEARED

                if (openPositions.Count > 0 && DateTime.UtcNow.Second % 10 == 0 && DateTime.UtcNow.Second != secondSwitch30)
                {
                    secondSwitch30 = DateTime.UtcNow.Second;
                    var posToClear = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                    if (posToClear.Quantity == 0 && (posToClear.EntryPrice == 0 || posToClear.EntryPrice == -1))
                    {
                        openPositions.Clear();
                        var closeOpenPosition = await client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD");
                        if (closeOpenPosition.Success)
                        {
                            Console.WriteLine($"CLEARED OPEN POSITION | Clear Orders Message: {closeOpenPosition.Data}");
                        } else
                        {
                            Console.WriteLine($"{closeOpenPosition.Error}");
                        }
                    }
                }

                //var childOP = openPositions.FirstOrDefault();

                // Update the data of the object
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                //testRealOpenPositionsTrailingStopLoss(openPositions, BTCUSDTprice, client);

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

            var client = new BinanceClient(new BinanceClientOptions
            {
                UsdFuturesApiOptions = new BinanceApiClientOptions {
                    ApiCredentials = new BinanceApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                    BaseAddress = "https://fapi.binance.com"
                }
            });

            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.FifteenMinutes, DateTime.UtcNow.AddDays(-20), DateTime.UtcNow);//"BTCUSDT"
            lineData = lineData.Skip(Math.Max(0, lineData.Count() - 1000)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();

            var bullEntry = new futureEntry();
            var bearEntry = new futureEntry();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCBUSD", KlineInterval.OneMinute, async data =>
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

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                    if (quotes.Count() >= 80)
                    {
                        for (int q = 330; q <= quotes.Count(); q++)
                        {
                            bool BullDFlag = false;
                            bool BearDFlag = false;
                            int MinPeriod = 4;
                            int Period = 120;

                            //quotes = quotes.Where(it => it.Date < new DateTime(2023, 05, 25, 11, 08, 00)).ToList();
                            var quotes1 = quotes.Take(q);

                            var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 10, Deviation: 5, BackStep: 2);
                            //var t = 0;

                            var pivots = zigZags.Where(it => it.PointType == "H" || it.PointType == "L").ToList();

                            var _rsi = quotes1.Use(CandlePart.OHLC4).GetRsi(21);
                            var _momentum = quotes1.GetRoc(20);

                            for (int i = MinPeriod; i <= Period; i++)
                            {
                                if (_momentum.Last().Roc > _momentum.Reverse().Skip(i).FirstOrDefault().Roc)
                                {
                                    if (quotes1.Last().Close < quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                    {
                                        if (quotes1.Last().Low <= quotes1.Reverse().Take(i).Min(it => it.Low))
                                        {
                                            if (_rsi.Reverse().Take(i).Min(it => it.Rsi) <= 30.0 && _rsi.Last().Rsi <= 30.0)
                                            {
                                                BullDFlag = true;
                                            }
                                        }
                                    }
                                }
                                else if (_momentum.Last().Roc < _momentum.Reverse().Skip(i).FirstOrDefault().Roc)
                                {
                                    if (quotes1.Last().Close > quotes1.Reverse().Skip(i).FirstOrDefault().Close)
                                    {
                                        if (quotes1.Last().High >= quotes1.Reverse().Take(i).Max(it => it.High))
                                        {
                                            if (_rsi.Reverse().Take(i).Max(it => it.Rsi) >= 70.0 && _rsi.Last().Rsi >= 70.0)
                                            {
                                                BearDFlag = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (BullDFlag)
                            {
                                Console.WriteLine($"BULLISH FLAG - {quotes1.Last().Date}");

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

                                if (bullEntry.ret0 == null || bullEntry.ret0 > quotes1.Last().Low)
                                {
                                    bullEntry.ret0 = quotes1.Last().Low;
                                    bullEntry.ret1 = highs.FirstOrDefault(it => it.ZigZag == theHighValue).ZigZag.Value;
                                    decimal? diff = bullEntry.ret1 - bullEntry.ret0;
                                    bullEntry.ret0236 = bullEntry.ret0 + diff * 0.236m;
                                    bullEntry.ret0382 = bullEntry.ret0 + diff * 0.382m;
                                    bullEntry.ret05 = bullEntry.ret0 + diff * 0.5m;
                                    bullEntry.ret0618 = bullEntry.ret0 + diff * 0.618m;
                                    bullEntry.ret0786 = bullEntry.ret0 + diff * 0.786m;
                                    bullEntry.ret1618 = bullEntry.ret0 + diff * 1.618m;
                                    bullEntry.entryPrice = bullEntry.ret0236;
                                }
                            }
                            if (BearDFlag)
                            {
                                Console.WriteLine($"BEARISH FLAG - {quotes1.Last().Date}");

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

                                if (bearEntry.ret0 == null || bearEntry.ret0 < quotes1.Last().High)
                                {
                                    bearEntry.ret0 = quotes1.Last().High;
                                    bearEntry.ret1 = lows.FirstOrDefault(it => it.ZigZag == theLowValue).ZigZag.Value;
                                    decimal? diff = bearEntry.ret0 - bearEntry.ret1;
                                    bearEntry.ret0236 = bearEntry.ret0 - diff * 0.236m;
                                    bearEntry.ret0382 = bearEntry.ret0 - diff * 0.382m;
                                    bearEntry.ret05 = bearEntry.ret0 - diff * 0.5m;
                                    bearEntry.ret0618 = bearEntry.ret0 - diff * 0.618m;
                                    bearEntry.ret0786 = bearEntry.ret0 - diff * 0.786m;
                                    bearEntry.ret1618 = bearEntry.ret0 - diff * 1.618m;
                                    bearEntry.entryPrice = bearEntry.ret0236;
                                }
                            }
                        }
                        var gg = 0;
                    }
                    if (true)
                    {
                        var realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                        if (realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0) /// MINUTE == MINUTE
                        {
                            // check for LONG POSITION

                            //LONG CHECK
                            if (quotes.Last().Open < bullEntry.entryPrice
                                && quotes.Last().Close > bullEntry.entryPrice)
                            {
                                Console.WriteLine($"LONG SIGNAL!");
                                Console.WriteLine($"Sending long ORDER request ... Current price is: {BTCUSDTprice}");

                                var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.Market, quantity: investment);

                                if (result.Success)//false
                                {
                                    //var tradeInfo = new futureEntry(bullEntry);
                                    realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
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
                                        var SL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                                                                , OrderType.StopMarket
                                                                                , quantity: investment
                                                                                , stopPrice: bullEntry.ret0 - 15
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark);
                                        var TP1 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, bullEntry.ret0382);
                                        var TP2 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, bullEntry.ret05);
                                        var TP3 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, bullEntry.ret0618);
                                        var TP4 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, bullEntry.ret0786);
                                        var TP5 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, bullEntry.ret1);
                                        var TP6 = await PlaceTakeProfit(client, OrderSide.Sell, investment / 6, bullEntry.ret1618);

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

                        if (realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0)
                        {
                            // check for SHORT POSITION
                            //Console.WriteLine($"SIGNAL DATA: dir: {dir}, dirPrev: {dirPrev}, shortvs: {shortvs}, shortvs1: {shortvs1}, longvs: {longvs}, longvs1: {longvs1}");
                            if (quotes.Last().Open > bearEntry.entryPrice
                                && quotes.Last().Close < bearEntry.entryPrice)
                            {
                                Console.WriteLine($"SHORT SIGNAL!");
                                Console.Write($"Sending short position request ... Current price is: {BTCUSDTprice}");

                                var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.Market, quantity: investment);

                                if (result.Success)//false
                                {
                                    realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
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
                                        var SL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                                , OrderType.StopMarket
                                                                                , quantity: investment
                                                                                , stopPrice: bullEntry.ret0 + 15
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark);
                                        var TP1 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, bullEntry.ret0382);
                                        var TP2 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, bullEntry.ret05);
                                        var TP3 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, bullEntry.ret0618);
                                        var TP4 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, bullEntry.ret0786);
                                        var TP5 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, bullEntry.ret1);
                                        var TP6 = await PlaceTakeProfit(client, OrderSide.Buy, investment / 6, bullEntry.ret1618);
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

                if (openPositions.Count == 50)
                {
                    var childOP2 = openPositions.First();
                    if (childOP2.type == "long")
                    {
                        if (BTCUSDTprice > childOP2.price * 1.001M && childOP2.stopLimitPrice < childOP2.price)
                        {
                            childOP2.stopLimitPrice = Math.Round(childOP2.price, 2);
                        }
                        childOP2.stopLimitPrice = (decimal?)Math.Round(Math.Max((double)childOP2.stopLimitPrice, (double)BTCUSDTprice * 0.998), 2);
                    }
                    if (childOP2.type == "short")
                    {
                        if (BTCUSDTprice < childOP2.price * 0.999M && childOP2.stopLimitPrice > childOP2.price)
                        {
                            childOP2.stopLimitPrice = Math.Round(childOP2.price, 2);
                        }
                        childOP2.stopLimitPrice = (decimal?)Math.Round(Math.Min((double)childOP2.stopLimitPrice, (double)BTCUSDTprice * 1.002), 2);
                    }
                    //Console.WriteLine($"New StopLoss: {childOP.stopLimitPrice}");

                    //update stoplimit:
                    if (false)
                    {
                        secondSwitch10 = DateTime.UtcNow.Second;
                        var cancelAllOrders = client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD").Result; //cancel all orders ///// TRY IT WITH TRAILING STOP AUTOMATIC FROM API
                        //dynamic newOrder = null;
                        if (childOP2.type == "long")
                        {
                            var newOrder = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.StopMarket, stopPrice: (decimal?)Mround(childOP2.stopLimitPrice, 1), quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;
                            Console.WriteLine($"long newOrder triggered: success: {newOrder.Success} success false: {newOrder.Error}");
                        }
                        if (childOP2.type == "short")
                        {
                            var newOrder = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.StopMarket, stopPrice: (decimal?)Mround(childOP2.stopLimitPrice, 1), quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;
                            Console.WriteLine($"short newOrder triggered: success: {newOrder.Success} success false: {newOrder.Error}");
                        }
                    }

                    if (false)
                    {
                        secondSwitch30 = DateTime.UtcNow.Second;
                        var pos = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                        if (pos.Quantity == 0)
                        {
                            openPositions.Remove(childOP2);
                        }
                    }
                }

                //CLEAR OPEN POSITIONS AFTER BINANCE POSITIONS CLEARED

                if (false && openPositions.Count > 0 && DateTime.UtcNow.Second % 10 == 0 && DateTime.UtcNow.Second != secondSwitch30)
                {
                    secondSwitch30 = DateTime.UtcNow.Second;
                    var posToClear = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                    if (posToClear.Quantity == 0 && (posToClear.EntryPrice == 0 || posToClear.EntryPrice == -1))
                    {
                        openPositions.Clear();
                        var closeOpenPosition = await client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD");
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
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                //Console.WriteLine($"High: {existingKline.High} | Low: {existingKline.Low} | Volume: {existingKline.BaseVolume}");

                //testRealOpenPositionsTrailingStopLoss(openPositions, BTCUSDTprice, client);

                if (candleStarted)
                {
                    candleStarted = false;

                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
        }

        private static async Task<CryptoExchange.Net.Objects.WebCallResult<BinanceFuturesPlacedOrder>> PlaceTakeProfit(BinanceClient client, OrderSide orderSide, decimal amount, decimal? price)
        {
            return await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", orderSide
                                                                            , OrderType.Limit
                                                                            , quantity: amount
                                                                            , price: price
                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                            , reduceOnly: true
                                                                            , workingType: WorkingType.Mark);
        }

        private static async Task liveStrat9_1()
        {
            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://api.binance.com",
            });

            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow);//"BTCUSDT"
            lineData = lineData.Skip(Math.Max(0, lineData.Count() - 200)).ToList(); //take only last 400 for optimization purposes
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();
            var lineSim = new List<IBinanceKline>();

            var socketClient = new BinanceSocketClient();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCBUSD", KlineInterval.OneMinute, async data =>
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
                }

                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                if (quotes.Count() >= 80)
                {
                    int length = 20; //Bollinger Bands length
                    decimal mult = 2.0m; //Bollinger Bands multFactor
                    int lengthKC = 20; //KC length
                    decimal multKC = 1.5m; //KC multFactor
                    var quotesList = quotes.ToList();

                    // ----------------

                    var BB = quotes.Use(CandlePart.Close).GetBollingerBands(length, (double)multKC);
                    var KC = quotes.GetKeltner(lengthKC, (double)multKC);

                    var bbKC = BB.Zip(KC, (x, y) => new Tuple<BollingerBandsResult, KeltnerResult>(x, y)).ToList();


                    var sqzOn = bbKC.Select(it => (it.Item1.LowerBand > it.Item2.LowerBand) && (it.Item1.UpperBand < it.Item2.UpperBand)).ToList();
                    var sqzOff = bbKC.Select(it => (it.Item1.LowerBand < it.Item2.LowerBand) && (it.Item1.UpperBand > it.Item2.UpperBand)).ToList();
                    var noSqz = sqzOn.Zip(sqzOff, (x, y) => new Tuple<bool, bool>(x, y)).ToList().Select(it => (it.Item1 == false) && (it.Item2 == false)).ToList();

                    var val = new List<decimal>();
                    for (int i = 0; i < quotesList.Count; i++)
                    {
                        if (i < lengthKC - 1)
                        {
                            val.Add(0);
                            continue;
                        }
                        var highest = quotes.Skip(i - lengthKC + 1).Take(lengthKC).Max(it => it.High);
                        var lowest = quotes.Skip(i - lengthKC + 1).Take(lengthKC).Min(it => it.Low);
                        var sma = quotes.Skip(i - lengthKC + 1).Take(lengthKC).GetSma(lengthKC).Last().Sma;

                        if (sma != null)
                        {
                            decimal item = quotesList[i].Close - (((highest + lowest) / 2 + (decimal)sma.Value) / 2);
                            val.Add(item);
                        }
                        else
                        {
                            val.Add(0);
                        }
                    }

                    var linreg = new List<double>();
                    double[] inputs = Enumerable.Range(0, 20)
                            .Select(i => (double)i)
                            .ToArray();
                    OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

                    for (int i = 0; i < val.Count; i++)
                    {
                        if (i < lengthKC - 1)
                        {
                            linreg.Add(0);
                        }
                        else
                        {
                            var skip = i - lengthKC + 1;
                            var take = lengthKC;
                            var outputs = val.Skip(skip).Take(take).Select(it => (double)it).ToArray();
                            SimpleLinearRegression regression = ols.Learn(inputs, outputs);

                            linreg.Add(regression.Intercept + regression.Slope * (lengthKC - 1));
                        }
                    }

                    //var realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");

                    //find all entries with stop/target
                    var last = quotesList.Count - 1;
                    var sqzCalc = noSqz[last] ? 1 : sqzOn[last] ? 2 : 3;
                    int orangeDots = 0;
                    for (int j = last - 6; j < last; j++)
                    {
                        if (sqzOn[j]) orangeDots++;
                    }


                    //check Sell
                    if (linreg[last] < linreg[last - 1] && sqzOn[last] == false && orangeDots > 3)// && quotesList[i].Close < lP) //sell
                    {
                        //Console.WriteLine($"SELL SIGNAL @ TIME: {quotesList[i].Date} / Candle Low: {quotesList[i].Low}");
                    }

                    //check Buy
                    if (linreg[last] > linreg[last - 1] && sqzOn[last] == false && orangeDots > 3)// && quotesList[i].Close > hP) //sell
                    {
                        //Console.WriteLine($"BUY SIGNAL @ TIME: {quotesList[i].Date} / Candle High: {quotesList[i].High}");
                    }
                    var ggg = 0;
                }
                if (false)
                {
                    var realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");


                    if (realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0) /// MINUTE == MINUTE
                    {
                        // check for LONG POSITION

                        //LONG CHECK
                        if (false)
                        {
                            Console.WriteLine($"LONG SIGNAL!");
                            Console.WriteLine($"Sending long ORDER request ... Current price is: {BTCUSDTprice}");

                            var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.Market, quantity: investment);
                            /*var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                                                            , OrderType.TrailingStopMarket
                                                                            , quantity: investment
                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                            , reduceOnly: true
                                                                            , workingType: WorkingType.Mark
                                                                            , callbackRate: 0.1M);*/

                            //var result = false;

                            if (result.Success)//false
                            {
                                realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                                var position = realOpenPos.Data.First();

                                openPositions.Add(new binanceOpenFuture
                                {
                                    startTime = DateTime.UtcNow,
                                    price = position.EntryPrice,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = position.EntryPrice * 0.99M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                    type = "long",
                                });

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine($"LONG OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                Console.ForegroundColor = ConsoleColor.White;
                                //Console.Write($"LONG OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                                try
                                {
                                    var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                                                            , OrderType.TrailingStopMarket
                                                                            , quantity: investment
                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                            , reduceOnly: true
                                                                            , workingType: WorkingType.Mark
                                                                            , callbackRate: 0.1M);
                                    /*
                                    var tt0 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                        , OrderSide.Sell
                                                                                        , OrderType.Limit
                                                                                        , quantity: investment
                                                                                        , timeInForce: TimeInForce.GoodTillCancel
                                                                                        , reduceOnly: true
                                                                                        , price: (decimal?)Math.Round((double)(position.EntryPrice + (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                        , workingType: WorkingType.Mark);

                                    var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                , OrderSide.Sell
                                                                                , OrderType.StopMarket
                                                                                , quantity: investment
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , stopPrice: (decimal?)Math.Round((double)(position.EntryPrice - atrMult), 1)   //0.4% in the red
                                                                                , workingType: WorkingType.Mark);
                                    */
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

                    if (realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0)
                    {
                        // check for SHORT POSITION
                        //Console.WriteLine($"SIGNAL DATA: dir: {dir}, dirPrev: {dirPrev}, shortvs: {shortvs}, shortvs1: {shortvs1}, longvs: {longvs}, longvs1: {longvs1}");
                        if (false)
                        {
                            Console.WriteLine($"SHORT SIGNAL!");
                            Console.Write($"Sending short position request ... Current price is: {BTCUSDTprice}");

                            var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.Market, quantity: investment);

                            /*var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                            , OrderType.TrailingStopMarket
                                                                            , quantity: investment
                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                            , reduceOnly: true
                                                                            , workingType: WorkingType.Mark
                                                                            , callbackRate: 0.1M);*/

                            if (result.Success)//false
                            {
                                realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                                var position = realOpenPos.Data.First();

                                openPositions.Add(new binanceOpenFuture
                                {
                                    startTime = DateTime.UtcNow,
                                    price = position.EntryPrice,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = position.EntryPrice * 1.01M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                    type = "short",
                                });

                                try
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"SHORT OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    //Console.Write($"SHORT OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");

                                    var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                            , OrderType.TrailingStopMarket
                                                                            , quantity: investment
                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                            , reduceOnly: true
                                                                            , workingType: WorkingType.Mark
                                                                            , callbackRate: 0.1M);

                                    /*var tt0 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                        , OrderSide.Buy
                                                                                        , OrderType.Limit
                                                                                        , quantity: investment
                                                                                        , timeInForce: TimeInForce.GoodTillCancel
                                                                                        , reduceOnly: true
                                                                                        , price: (decimal?)Math.Round((double)(position.EntryPrice - (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                        , workingType: WorkingType.Mark);

                                    var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                        , OrderSide.Buy
                                                                                        , OrderType.StopMarket
                                                                                        , quantity: investment
                                                                                        , timeInForce: TimeInForce.GoodTillCancel
                                                                                        , reduceOnly: true
                                                                                        , stopPrice: (decimal?)Math.Round((double)(position.EntryPrice + atrMult), 1)//Math.Round(position.EntryPrice * 1.00133M, 1)   //0.4% in the red
                                                                                        , workingType: WorkingType.Mark);*/
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

                if (openPositions.Count == 50)
                {
                    var childOP2 = openPositions.First();
                    if (childOP2.type == "long")
                    {
                        if (BTCUSDTprice > childOP2.price * 1.001M && childOP2.stopLimitPrice < childOP2.price)
                        {
                            childOP2.stopLimitPrice = Math.Round(childOP2.price, 2);
                        }
                        childOP2.stopLimitPrice = (decimal?)Math.Round(Math.Max((double)childOP2.stopLimitPrice, (double)BTCUSDTprice * 0.998), 2);
                    }
                    if (childOP2.type == "short")
                    {
                        if (BTCUSDTprice < childOP2.price * 0.999M && childOP2.stopLimitPrice > childOP2.price)
                        {
                            childOP2.stopLimitPrice = Math.Round(childOP2.price, 2);
                        }
                        childOP2.stopLimitPrice = (decimal?)Math.Round(Math.Min((double)childOP2.stopLimitPrice, (double)BTCUSDTprice * 1.002), 2);
                    }
                    //Console.WriteLine($"New StopLoss: {childOP.stopLimitPrice}");

                    //update stoplimit:
                    if (false)
                    {
                        secondSwitch10 = DateTime.UtcNow.Second;
                        var cancelAllOrders = client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD").Result; //cancel all orders ///// TRY IT WITH TRAILING STOP AUTOMATIC FROM API
                        //dynamic newOrder = null;
                        if (childOP2.type == "long")
                        {
                            var newOrder = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.StopMarket, stopPrice: (decimal?)Mround(childOP2.stopLimitPrice, 1), quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;
                            Console.WriteLine($"long newOrder triggered: success: {newOrder.Success} success false: {newOrder.Error}");
                        }
                        if (childOP2.type == "short")
                        {
                            var newOrder = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.StopMarket, stopPrice: (decimal?)Mround(childOP2.stopLimitPrice, 1), quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true, workingType: WorkingType.Mark).Result;
                            Console.WriteLine($"short newOrder triggered: success: {newOrder.Success} success false: {newOrder.Error}");
                        }
                    }

                    if (false)
                    {
                        secondSwitch30 = DateTime.UtcNow.Second;
                        var pos = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                        if (pos.Quantity == 0)
                        {
                            openPositions.Remove(childOP2);
                        }
                    }
                }

                //CLEAR OPEN POSITIONS AFTER BINANCE POSITIONS CLEARED

                if (false && openPositions.Count > 0 && DateTime.UtcNow.Second % 10 == 0 && DateTime.UtcNow.Second != secondSwitch30)
                {
                    secondSwitch30 = DateTime.UtcNow.Second;
                    var posToClear = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                    if (posToClear.Quantity == 0 && (posToClear.EntryPrice == 0 || posToClear.EntryPrice == -1))
                    {
                        openPositions.Clear();
                        var closeOpenPosition = await client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD");
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
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                //Console.WriteLine($"High: {existingKline.High} | Low: {existingKline.Low} | Volume: {existingKline.BaseVolume}");

                //testRealOpenPositionsTrailingStopLoss(openPositions, BTCUSDTprice, client);

                if (candleStarted)
                {
                    candleStarted = false;

                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
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

        static async void volumeProfilePOCCalculation(List<IBinanceKline> lineData)
        {
            //************************************
            // Target 1 = Point B of the Harmonic, Entry is where you get in, StopLoss in a Bull is entry - (Target1 - Entry)
            //************************************

            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://api.binance.com",
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
                    var volumeToAdd = child.BaseVolume / Math.Max((child.High - child.Low), 0.01m);
                    totalVolume += child.BaseVolume;
                    foreach (var dchild in VP)
                    {
                        if (child.Low <= dchild.interval.Item2 && dchild.interval.Item1 <= child.High)
                        {
                            var over = FindOverlapping(child.Low, child.High, dchild.interval.Item1, dchild.interval.Item2);
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

                var priceStart = lineData.First(it => it.OpenTime > dailyProfiles[i - 1].end).Open;

                if (priceStart >= dailyProfiles[i - 1].valueArea.Item1 && priceStart <= dailyProfiles[i - 1].valueArea.Item2)
                {
                    Console.WriteLine("Start price {0} in Value Area {1}", priceStart, dailyProfiles[i - 1].valueArea);


                    var pocInter = lineData.FirstOrDefault(it
                        => it.OpenTime >= dailyProfiles[i - 1].end.AddMinutes(1)
                        && FindOverlapping(it.Low, it.High, dailyProfiles[i - 1].poc.Item1, dailyProfiles[i - 1].poc.Item2) > 0m);

                    var bars = lineData.Where(it =>
                            it.OpenTime >= dailyProfiles[i - 1].end.AddMinutes(1)
                            && it.OpenTime <= pocInter.OpenTime);

                    if (pocInter != null)
                    {
                        Console.WriteLine("TRADE WOULD HIT @ {0} --- FROM {1} to {2}", pocInter.OpenTime, priceStart, dailyProfiles[i - 1].poc);
                        Console.WriteLine("ENTRY: {0} | MAX HIGH: {1} | MIN LOW: {2}", priceStart, bars.Max(it => it.High), bars.Min(it => it.Low));
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

        static async void volumeProfileCalculation()
        {
            //************************************
            // Target 1 = Point B of the Harmonic, Entry is where you get in, StopLoss in a Bull is entry - (Target1 - Entry)
            //************************************

            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://api.binance.com",
            });

            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-20), DateTime.UtcNow);//"BTCUSDT"
            bool candleStarted = false;

            int step = 20;
            for (int i = 0; i < 60000 - step; i += step)
            {
                var x = new VolumeProfile();
                x.interval = new Tuple<decimal, decimal>(i, (decimal)i + step - 0.01M);
                x.sum = 0;
                x.isInterestZone = false;
                VP.Add(x);
            }

            foreach (var child in lineData)
            {
                var volumeToAdd = child.BaseVolume / Math.Max((child.High - child.Low), 1);
                foreach (var dchild in VP)
                {
                    if (child.Low <= dchild.interval.Item2 && dchild.interval.Item1 <= child.High)
                    {
                        dchild.sum += volumeToAdd;
                    }
                }
            }

            // ----------------------------------------------------------------------

            decimal down = 15000;
            decimal up = 21500;
            int intcnt = 0;
            decimal sum = 0;
            decimal avg = 0;

            for (int i = 0; i < 20; i++)
            {
                avg += (VP[i].sum / 20);
            }
            int stu = 0;

            for (var V = VP.Count - 1; V >= 0; V--)
            {
                var child = VP[V];
                if (stu >= 20)
                {
                    avg += VP[stu].sum / 20;
                    avg -= VP[stu - 20].sum / 20;
                }
                if (down <= child.interval.Item2 && child.interval.Item1 <= up)
                {
                    if (child.sum > avg && child.sum > 300)
                    {
                        Console.Write("### ");
                        child.isInterestZone = true;
                    }
                    else
                    {
                        Console.Write("--- ");
                        child.isInterestZone = false;
                    }
                    Console.Write("{0} - {1}:  ", child.interval.Item1, child.interval.Item2);
                    for (int i = 0; i < child.sum / 100; i++)
                    {
                        Console.Write("#");
                    }
                    Console.WriteLine();
                }
                stu++;
            }

            // ----------------------------------------------------------------------

            // ### DEBUGGING - print results

            var socketClient = new BinanceSocketClient();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCBUSD", KlineInterval.FiveMinutes, async data => //SCHIMBAT
            {
                //Console.WriteLine($"BTCUSDT PRICE: # {BTCUSDTprice}");
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
                List<insaneEntry> inEntries = new List<insaneEntry>();
                if (existingKline == null)
                {
                    //A NEW CANDLE HAS STARTED
                    candleStarted = true;

                    existingKline = new bsk()
                    {
                        CloseTime = data.Data.Data.CloseTime,
                        OpenTime = data.Data.Data.OpenTime
                    };
                    lineData.Add(existingKline);
                }

                // Update the data of the object
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                if (candleStarted)
                {
                    var volumeToAdd = existingKline.BaseVolume / Math.Max((existingKline.High - existingKline.Low), 1);
                    foreach (var dchild in VP)
                    {
                        if (existingKline.Low <= dchild.interval.Item2 && dchild.interval.Item1 <= existingKline.High)
                        {
                            dchild.sum += volumeToAdd;
                        }
                    }

                    down = 15000;
                    up = 21500;
                    intcnt = 0;
                    sum = 0;
                    avg = 0;

                    for (int i = 0; i < 20; i++)
                    {
                        avg += (VP[i].sum / 20);
                    }
                    stu = 0;

                    for (var V = VP.Count - 1; V >= 0; V--)
                    {
                        var child = VP[V];
                        if (stu >= 20)
                        {
                            avg += VP[stu].sum / 20;
                            avg -= VP[stu - 20].sum / 20;
                        }
                        if (down <= child.interval.Item2 && child.interval.Item1 <= up)
                        {
                            if (child.sum > avg && child.sum > 300) {
                                Console.Write("### ");
                                child.isInterestZone = true;
                            } else {
                                Console.Write("--- ");
                                child.isInterestZone = false;
                            }
                            Console.Write("{0} - {1}:  ", child.interval.Item1, child.interval.Item2);
                            for (int i = 0; i < child.sum / 100; i++)
                            {
                                Console.Write("#");
                            }
                            Console.WriteLine();
                        }
                        stu++;
                    }
                    candleStarted = false;
                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
        }

        private static async Task liveStrat10HarmonicPatterns()
        {
            //************************************
            // Target 1 = Point B of the Harmonic, Entry is where you get in, StopLoss in a Bull is entry - (Target1 - Entry)
            //************************************

            //https://webhook.site/#!/1e7596ab-3a87-44c5-b0cc-b4fc26f52607/99a5b41e-0028-4d6b-870e-052b2ec77e3b/1

            //webhookEvents();
            //stopWatch.Start();

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://api.binance.com",
            });

            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.ThirtyMinutes, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);//"BTCUSDT"
            //var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.OneMinute, DateTime.UtcNow.AddDays(-6), DateTime.UtcNow);//"BTCUSDT"
            var lineData = await GetAllKlinesInterval("BTCBUSD", KlineInterval.ThirtyMinutes, DateTime.UtcNow.AddDays(-32), DateTime.UtcNow);//"BTCUSDT"
            //lineData = lineData.Skip(Math.Max(0, lineData.Count() - 5450)).ToList(); //take only last 1450 for optimization purposes
            lineData = lineData.Skip(Math.Max(0, lineData.Count() - 650)).ToList(); //take only last 1450 for optimization purposes //SCHIMBAT
            bool candleStarted = false;

            var openPositions = new List<binanceOpenFuture>();

            var socketClient = new BinanceSocketClient();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCBUSD", KlineInterval.ThirtyMinutes, async data => //SCHIMBAT
            {
                //Console.WriteLine($"BTCUSDT PRICE: # {BTCUSDTprice}");
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
                List<insaneEntry> inEntries = new List<insaneEntry>();
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
                    var lineSim = lineData.ToList();
                    lineData.Add(existingKline);
                    //Console.WriteLine($"count lineSim = {lineSim.Count}, count lineData = {lineData.Count}");
                    lineData.RemoveAt(0); //when adding a new Kline, remove the first in the list so it stays the same length
                    Console.WriteLine($"Added new kline: {existingKline.OpenTime}-{existingKline.CloseTime} | Number of Klines: {lineData.Count}");

                    if (openPositions.Count == 1)
                    {
                        var childOP2 = openPositions.First();
                        Console.WriteLine($"New StopLoss: {childOP2.stopLimitPrice}");
                    }

                    //lineSim = lineSim.Where(it => it.CloseTime < Convert.ToDateTime("11 Aug 2022 14:44")).ToList();
                    //IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 550)).ToList()); //SCHIMBAT
                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 650)).ToList()); //USING LINE DATA INSTEAD OF LINESIM BECAUSE I NEED THE LIVE HIGH/LOW

                    var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 6, Deviation: 5, BackStep: 2);
                    //zigZags = zigZags.Skip(Math.Max(0, zigZags.Count() - 250)).ToList(); //15minutes
                    //zigZags = zigZags.Skip(Math.Max(0, zigZags.Count() - 120)).ToList();
                    //CalculateZigzags(quotes.ToList(), 12, 5);
                    //var zigZags = quotes.GetZigZag(EndType.HighLow, percentChange: 0.5M).ToList();

                    //var currClose = lineSim[lineSim.Count - 2].Close;

                    //List<decimal?> orderList = new List<decimal?>();

                    //PATTERN CHECKER

                    //var ptQuotes = lineSim.Skip(Math.Max(0, lineSim.Count() - 30)).ToList();
                    //zigZags = zigZags.Where(it => it.Date < Convert.ToDateTime("17 Aug 2022 08:00")).ToList();
                    var zzTemp = zigZags.Where(it => it.PointType == "H" || it.PointType == "L");
                    //var zz = zzTemp.Skip(Math.Max(0, zzTemp.Count() - 120)).ToList(); //15 minutes
                    var zz = zzTemp.Skip(Math.Max(0, zzTemp.Count() - 60)).ToList();

                    Console.WriteLine("First ZigZag in testing: {0}", zz.FirstOrDefault() != null ? zz.First().Date : DateTime.UtcNow);

                    bool bullishBat = false;
                    bool bearishBat = false;
                    bool bullishAltBat = false;
                    bool bearishAltBat = false;
                    bool bullishButterfly = false;
                    bool bearishButterfly = false;
                    bool bullishCrab = false;
                    bool bearishCrab = false;
                    bool bullishDeepCrab = false;
                    bool bearishDeepCrab = false;
                    bool bullishGartley = false;
                    bool bearishGartley = false;
                    bool bullishShark = false;
                    bool bearishShark = false;
                    bool bullishCypher = false;
                    bool bearishCypher = false;
                    bool bullishNenStar = false;
                    bool bearishNenStar = false;
                    bool bullishNavarro200 = false;
                    bool bearishNavarro200 = false;
                    bool bullishAntiCrab = false;
                    bool bearishAntiCrab = false;
                    bool bullishAntiButterfly = false;
                    bool bearishAntiButterfly = false;
                    bool bullishAntiNenStar = false;
                    bool bearishAntiNenStar = false;
                    bool bullishAntiBat = false;
                    bool bearishAntiBat = false;
                    bool bullishAntiNewCypher = false;
                    bool bearishAntiNewCypher = false;
                    bool bullishAntiGartley = false;
                    bool bearishAntiGartley = false;
                    int countBull = 0;
                    int countBear = 0;
                    decimal? goodP1 = 0;
                    decimal? goodP2 = 0;
                    decimal? goodP3 = 0;
                    decimal? goodP4 = 0;
                    decimal? goodP5 = 0;
                    int md = 11; //max depth
                    int hFound = 0;

                    List<ZigZagPattern> genPat = new List<ZigZagPattern>();

                    for (int i1 = 0; i1 < zz.Count; i1++)
                    {
                        if ((DateTime.UtcNow - zz[i1].Date).TotalHours > 240) continue;
                        for (int i2 = i1 + 1; i2 < Math.Min(i1 + md, zz.Count); i2++)
                        {
                            if ((DateTime.UtcNow - zz[i2].Date).TotalHours > 240) continue;
                            if (zz[i1].PointType == zz[i2].PointType) continue;// || i2 > i1 + 30

                            for (int i3 = i2 + 1; i3 < Math.Min(i2 + md, zz.Count); i3++)
                            {
                                if ((DateTime.UtcNow - zz[i3].Date).TotalHours > 240) continue;
                                if (zz[i2].PointType == zz[i3].PointType) continue;

                                for (int i4 = i3 + 1; i4 < Math.Min(i3 + md, zz.Count); i4++)
                                {
                                    if ((DateTime.UtcNow - zz[i4].Date).TotalHours > 240) continue;
                                    if (zz[i3].PointType == zz[i4].PointType) continue;

                                    for (int i5 = zz.Count - 1; i5 < zz.Count; i5++)
                                    {
                                        int bull = 0;
                                        int bear = 0;

                                        if (zz[i4].PointType == zz[i5].PointType || i5 - i4 > md) continue;

                                        if (zz[i1].PointType == "L"
                                            && zz[i1].ZigZag < zz[i2].ZigZag
                                            && zz[i2].ZigZag > zz[i3].ZigZag
                                            && zz[i3].ZigZag < zz[i4].ZigZag
                                            && zz[i4].ZigZag > zz[i5].ZigZag) //
                                        {
                                            bullishBat = IsBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAltBat = IsAltBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishButterfly = IsButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishCrab = IsCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishDeepCrab = IsDeepCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishGartley = IsGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishShark = IsShark(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishCypher = IsCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishNenStar = IsNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiCrab = IsAntiCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiButterfly = IsAntiButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiNenStar = IsAntiNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiBat = IsAntiBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiNewCypher = IsAntiNewCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiGartley = IsAntiGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);                                            
                                        }

                                        if (zz[i1].PointType == "H"
                                            && zz[i1].ZigZag > zz[i2].ZigZag
                                            && zz[i2].ZigZag < zz[i3].ZigZag
                                            && zz[i3].ZigZag > zz[i4].ZigZag
                                            && zz[i4].ZigZag < zz[i5].ZigZag) //
                                        {
                                            bearishBat = IsBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishAltBat = IsAltBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishButterfly = IsButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishCrab = IsCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishDeepCrab = IsDeepCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishGartley = IsGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishShark = IsShark(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishCypher = IsCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishNenStar = IsNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishNavarro200 = IsNavarro200(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiCrab = IsAntiCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiButterfly = IsAntiButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiNenStar = IsAntiNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiBat = IsAntiBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiNewCypher = IsAntiNewCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiGartley = IsAntiGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low); 
                                        }

                                        //##############################
                                        //Bat Harmonic Chart Pattern

                                        //##############################
                                        //ALT Bat Harmonic Chart Pattern

                                        //##############################
                                        //Butterfly Harmonic Chart Pattern

                                        //##############################
                                        //Crab Harmonic Chart Pattern

                                        //##############################
                                        //Deep Crab Harmonic Chart Pattern

                                        //##############################
                                        //Gartley Harmonic Chart Pattern

                                        //##############################
                                        //Shark Harmonic Chart Pattern

                                        //##############################
                                        //Cypher Harmonic Chart Pattern

                                        bool confirmation = false;
                                        //int bull = 0;
                                        //int bear = 0;
                                        bool log = true;

                                        if (bullishBat)
                                        {
                                            if (log) Console.Write("BULLISH BAT!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishBat)
                                        {
                                            if (log) Console.Write("BEARISH BAT!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAltBat)
                                        {
                                            if (log) Console.Write("BULLISH ALT BAT!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAltBat)
                                        {
                                            if (log) Console.Write("BEARISH ALT BAT!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishButterfly)
                                        {
                                            if (log) Console.Write("BULLISH BUTTERFLY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishButterfly)
                                        {
                                            if (log) Console.Write("BEARISH BUTTERFLY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishCrab)
                                        {
                                            if (log) Console.Write("BULLISH CRAB!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishCrab)
                                        {
                                            if (log) Console.Write("BEARISH CRAB!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishDeepCrab)
                                        {
                                            if (log) Console.Write("BULLISH DEEP CRAB!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishDeepCrab)
                                        {
                                            if (log) Console.Write("BEARISH DEEP CRAB!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishGartley)
                                        {
                                            if (log) Console.Write("BULLISH GARTLEY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishGartley)
                                        {
                                            if (log) Console.Write("BEARISH GARTLEY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishShark)
                                        {
                                            if (log) Console.Write("BULLISH SHARK!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishShark)
                                        {
                                            if (log) Console.Write("BEARISH SHARK!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishCypher)
                                        {
                                            if (log) Console.Write("BULLISH CYPHER!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishCypher)
                                        {
                                            if (log) Console.Write("BEARISH CYPHER!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishNavarro200)
                                        {
                                            if (log) Console.Write("BULLISH NAVARRO 200!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishNavarro200)
                                        {
                                            if (log) Console.Write("BEARISH NAVARRO 200!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiCrab)
                                        {
                                            if (log) Console.Write("BULLISH ANTI CRAB!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiCrab)
                                        {
                                            if (log) Console.Write("BEARISH ANTI CRAB!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiBat)
                                        {
                                            if (log) Console.Write("BULLISH ANTI BAT!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiBat)
                                        {
                                            if (log) Console.Write("BEARISH ANTI BAT!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiNewCypher)
                                        {
                                            if (log) Console.Write("BULLISH ANTI NEW CYPHER!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiNewCypher)
                                        {
                                            if (log) Console.Write("BEARISH ANTI NEW CYPHER!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiGartley)
                                        {
                                            if (log) Console.Write("BULLISH ANTI GARTLEY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiGartley)
                                        {
                                            if (log) Console.Write("BEARISH ANTI GARTLEY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiButterfly)
                                        {
                                            if (log) Console.Write("BULLISH ANTI BUTTERFLY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiButterfly)
                                        {
                                            if (log) Console.Write("BEARISH ANTI BUTTERFLY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiNenStar)
                                        {
                                            if (log) Console.Write("BULLISH ANTI NEN STAR!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiNenStar)
                                        {
                                            if (log) Console.Write("BEARISH ANTI NEN STAR!");
                                            confirmation = true; ++bear;
                                        }

                                        if (confirmation && log)
                                        {
                                            genPat.Add(new ZigZagPattern
                                            {
                                                p1 = zz[i1],
                                                p2 = zz[i2],
                                                p3 = zz[i3],
                                                p4 = zz[i4],
                                                p5 = zz[i5]
                                            });
                                            Console.WriteLine(String.Format(" ~~~ {0}, {1}, {2}, {3}, {4}"
                                                , zz[i1].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i2].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i3].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i4].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i5].Date.ToString("dd/MM hh:mm tt")));
                                            //i1 = i2 = i3 = i4 = i5 = zz.Count;
                                            //break;
                                        }

                                        if (bull > 0 && bear == 0)
                                        {
                                            //Console.WriteLine("INSANE BUY POSITION");
                                            countBull++;
                                        }
                                        if (bear > 0 && bull == 0)
                                        {
                                            countBear++;
                                            //Console.WriteLine("INSANE SELL POSITION");
                                        }

                                        bullishBat = false;
                                        bearishBat = false;
                                        bullishAltBat = false;
                                        bearishAltBat = false;
                                        bullishButterfly = false;
                                        bearishButterfly = false;
                                        bullishCrab = false;
                                        bearishCrab = false;
                                        bullishDeepCrab = false;
                                        bearishDeepCrab = false;
                                        bullishGartley = false;
                                        bearishGartley = false;
                                        bullishShark = false;
                                        bearishShark = false;
                                        bullishCypher = false;
                                        bearishCypher = false;
                                        bullishNenStar = false;
                                        bearishNenStar = false;
                                        bullishNavarro200 = false;
                                        bearishNavarro200 = false;
                                        bearishAntiCrab = false;
                                        bearishAntiButterfly = false;
                                        bearishAntiNenStar = false;
                                        bearishAntiBat = false;
                                        bearishAntiNewCypher = false;
                                        bearishAntiGartley = false;
                                    }
                                }

                            }
                        }
                    }

                    if (genPat.Count > 0)
                    {
                        if (genPat.First().p5.ZigZag > genPat.First().p4.ZigZag)
                        {
                            genPat = genPat.OrderByDescending(it => it.p3.ZigZag).ToList();
                        } else
                        {
                            genPat = genPat.OrderBy(it => it.p3.ZigZag).ToList();
                        }
                    }
                    //genPat = genPat.OrderByDescending(it => it.p3.Date).ToList();
                    ZigZagPattern zzp = genPat.FirstOrDefault();
                    if (zzp != null)
                    {
                        goodP1 = zzp.p1.ZigZag; goodP2 = zzp.p2.ZigZag; goodP3 = zzp.p3.ZigZag; goodP4 = zzp.p4.ZigZag; goodP5 = zzp.p5.ZigZag;
                    }
                    Console.WriteLine("Harmonic points: {0}, {1}, {2}, {3}, {4}", goodP1, goodP2, goodP3, goodP4, goodP5);

                    if (countBull > 2 && countBear == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(String.Format("INSANE BUY POSITION {0} | countBull: {1} | countBear: {2}", zz[zz.Count - 1].Date.ToString("dd/MM hh:mm tt"), countBull, countBear));
                        inEntries.Add(new insaneEntry { time = zz[zz.Count - 1].Date, type = "buy" }); //SCHIMBAT
                        Console.ForegroundColor = ConsoleColor.White;
                        hFound = countBull;
                    }

                    if (countBear > 2 && countBull == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(String.Format("INSANE SELL POSITION {0} | countBull: {1} | countBear: {2}", zz[zz.Count - 1].Date.ToString("dd/MM hh:mm tt"), countBull, countBear));
                        inEntries.Add(new insaneEntry { time = zz[zz.Count - 1].Date, type = "sell" });//zz[zz.Count - 1].Date,  SCHIMBAT
                        Console.ForegroundColor = ConsoleColor.White;
                        hFound = countBear;
                    }

                    Console.WriteLine(String.Format("FIVE PREVIOUS H/L:\n {0} {1} \n {2} {3} \n {4} {5} \n {6} {7} \n {8} {9}"
                        , zz[zz.Count - 5].PointType, zz[zz.Count - 5].Date.ToString("dd/MM hh:mm tt")
                        , zz[zz.Count - 4].PointType, zz[zz.Count - 4].Date.ToString("dd/MM hh:mm tt")
                        , zz[zz.Count - 3].PointType, zz[zz.Count - 3].Date.ToString("dd/MM hh:mm tt")
                        , zz[zz.Count - 2].PointType, zz[zz.Count - 2].Date.ToString("dd/MM hh:mm tt")
                        , zz[zz.Count - 1].PointType, zz[zz.Count - 1].Date.ToString("dd/MM hh:mm tt")
                        ));
                    //var ema20 = quotesEma.GetEma(20).ToList();

                    var realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                    Console.WriteLine($"Real open positions First Entry Price: {(realOpenPos.Data != null ? realOpenPos.Data.First().EntryPrice : -1)}");

                    //var currClose = lineSim[lineSim.Count - 1].Close;
                    Console.WriteLine($"Current price: {BTCUSDTprice} | realOpenPos: {realOpenPos.Success}");

                    if (realOpenPos.Data != null && inEntries.Count > 0)
                    {
                        Console.WriteLine(String.Format("inEntries.Count = {0}\ninEntries.First().type = {1}\nrealOpenPos.Success = {2}\nrealOpenPos.Fata.FirstOrDefault().Quantity = {3}\nopenPositions.Count = {4}"
                            , inEntries.Count
                            , inEntries.First().type
                            , realOpenPos.Success
                            , realOpenPos.Data.FirstOrDefault().Quantity
                            , openPositions.Count));
                    }
                    if (realOpenPos.Data != null && inEntries.Count > 0 && inEntries.First().type == "buy" && realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0) /// MINUTE == MINUTE
                    {
                        Console.WriteLine("### TIME BeETWEEN HARMONIC PATTERN AND ENTRY | Minutes: {0}", (lineSim.Last().OpenTime - inEntries.First().time).Minutes);
                        if (false)//(lineSim.Last().OpenTime - inEntries.First().time).Minutes > 30)
                        {
                            //Console.WriteLine("### TOO FAR BACK! | Minutes: {0}", (lineSim.Last().OpenTime - inEntries.First().time).Minutes);
                        }
                        else
                        {
                            //goodP3 -= (goodP3 - goodP5) * 0.90M; //Take 80% of Target 1
                            // check for LONG POSITION
                            //LONG CHECK
                            var pChange = percChange(zz[zz.Count - 1].ZigZag.Value, BTCUSDTprice); //SCHIMBAT
                            Console.WriteLine("FOUND BUY ENTRY FROM PATTERN - INITIATING BUY | inEntries datetime {0} | lineData datettime {1} | pChange {2}%", inEntries.First().time, lineSim.Last().OpenTime, Math.Round(pChange, 2));
                            //Console.WriteLine(String.Format("GoodP3: {0} | GoodP5: {1} | BTCUSDTprice: {2}", goodP3, goodP5, BTCUSDTprice));
                            //Console.WriteLine("Max allowed entry line: ", goodP5 + ((goodP3 - goodP5) * 2 / 3));


                            //TRENDOSCOPE TARGETS
                            var topPoint = Math.Max(goodP2.Value, goodP4.Value);
                            var entry = (goodP5 + ((topPoint - goodP5) * 0.1M)).Value;
                            var target1 = goodP3.Value;
                            var stopLoss = Math.Min((goodP5 * 0.999M).Value, (entry - (target1 - entry) - 20));
                            Console.WriteLine("##########\nSTOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}\n##########", stopLoss, entry, target1);
                            if ((BTCUSDTprice < goodP5 + ((goodP3 - goodP5) * 2 / 3)) && (target1 - BTCUSDTprice) > 50)//pChange > 0 && pChange < 0.4M && //inEntries.First().time.AddMilliseconds(1) == lineSim.Last().OpenTime)
                            {
                                Console.WriteLine($"LONG SIGNAL!");
                                Console.WriteLine($"Sending long ORDER request ... Current price is: {BTCUSDTprice}");

                                var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.Market, quantity: investment);

                                if (result.Success)//false
                                {
                                    realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                                    var position = realOpenPos.Data.First();

                                    //DINAPOLI TARGETS
                                    //var stopLoss = Math.Min((double)(position.EntryPrice - (goodP3 - position.EntryPrice)), (double)(goodP5 - 10M));
                                    //-----

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = DateTime.UtcNow,
                                        price = position.EntryPrice,
                                        riskRatio = (decimal)1.5,
                                        stopLimitPrice = position.EntryPrice * 0.99M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                        type = "long",
                                    });

                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine($"LONG OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    //Console.Write($"LONG OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                                    try
                                    {
                                        //var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell
                                        //                                        , OrderType.TrailingStopMarket
                                        //                                        , quantity: investment
                                        //                                        , timeInForce: TimeInForce.GoodTillCancel
                                        //                                        , reduceOnly: true
                                        //                                        , workingType: WorkingType.Mark
                                        //                                        , price: goodP3//(decimal?)Math.Round((double)(position.EntryPrice * 1.002M), 1)
                                        //                                        , callbackRate: 0.1M);

                                        var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                , OrderSide.Sell
                                                , OrderType.StopMarket
                                                , quantity: investment
                                                , timeInForce: TimeInForce.GoodTillCancel
                                                , reduceOnly: true
                                                , stopPrice: (decimal?)Math.Round(stopLoss, 1)//(decimal?)Math.Round((double)(position.EntryPrice * 0.998M), 1)   //0.4% in the red
                                                , workingType: WorkingType.Mark);

                                        var tkp = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                            , OrderSide.Sell
                                                                                            , OrderType.Limit
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , price: Math.Round(target1 - 20, 1)//(decimal?)Math.Round((double)(position.EntryPrice + (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                            , workingType: WorkingType.Mark);

                                        /*
                                        var tt0 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                            , OrderSide.Sell
                                                                                            , OrderType.Limit
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , price: (decimal?)Math.Round((double)(position.EntryPrice + (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                            , workingType: WorkingType.Mark);

                                        var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                    , OrderSide.Sell
                                                                                    , OrderType.StopMarket
                                                                                    , quantity: investment
                                                                                    , timeInForce: TimeInForce.GoodTillCancel
                                                                                    , reduceOnly: true
                                                                                    , stopPrice: (decimal?)Math.Round((double)(position.EntryPrice - atrMult), 1)   //0.4% in the red
                                                                                    , workingType: WorkingType.Mark);
                                        */
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
                    }

                    if (realOpenPos.Data != null && inEntries.Count > 0 && inEntries.First().type == "sell" && realOpenPos.Success == true && realOpenPos.Data.FirstOrDefault().Quantity == 0 && openPositions.Count == 0)
                    {
                        Console.WriteLine("### TIME BeETWEEN HARMONIC PATTERN AND ENTRY | Minutes: {0}", (lineSim.Last().OpenTime - inEntries.First().time).Minutes);
                        if (false)//(lineSim.Last().OpenTime - inEntries.First().time).Minutes > 30)
                        {
                            //Console.WriteLine("### TOO FAR BACK! | Minutes: {0}", (lineSim.Last().OpenTime - inEntries.First().time).Minutes);
                        }
                        else
                        {
                            //goodP3 += (goodP5 - goodP3) * 0.8M; //20% higher Target 1 so it hits faster
                            // check for SHORT POSITION

                            //Console.WriteLine($"SIGNAL DATA: dir: {dir}, dirPrev: {dirPrev}, shortvs: {shortvs}, shortvs1: {shortvs1}, longvs: {longvs}, longvs1: {longvs1}");
                            var pChange = percChange(zz[zz.Count - 1].ZigZag.Value, BTCUSDTprice); //SCHIMBAT
                            Console.WriteLine("FOUND SELL ENTRY FROM PATTERN - INITIATING SELL | inEntries datetime {0} | lineData datettime {1} | pChange {2}%", inEntries.First().time, lineSim.Last().OpenTime, pChange);
                            Console.WriteLine(String.Format("GoodP3: {0} | GoodP5: {1} | BTCUSDTprice: {2}", goodP3, goodP5, BTCUSDTprice));
                            Console.WriteLine("Max allowed entry line: ", goodP5 - ((goodP5 - goodP3) * 2 / 3));

                            //TRENDOSCOPE TARGETS
                            var lowPoint = Math.Min(goodP2.Value, goodP4.Value);
                            var entry = (goodP5 - ((goodP5 - lowPoint) * 0.1M)).Value;
                            var target1 = goodP3.Value;
                            var stopLoss = Math.Max((goodP5 * 1.001M).Value, entry + (entry - target1) + 20);
                            Console.WriteLine("##########\nSTOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}\n##########", stopLoss, entry, target1);
                            if ((BTCUSDTprice > goodP5 - ((goodP5 - goodP3) * 2 / 3)) && (BTCUSDTprice - target1) > 50)//pChange < 0 && pChange > -0.4M && //inEntries.First().time.AddMilliseconds(1) == lineSim.Last().OpenTime)
                            {
                                Console.WriteLine($"SHORT SIGNAL!");
                                Console.Write($"Sending short position request ... Current price is: {BTCUSDTprice}");

                                var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.Market, quantity: investment);

                                /*var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                                                                , OrderType.TrailingStopMarket
                                                                                , quantity: investment
                                                                                , timeInForce: TimeInForce.GoodTillCancel
                                                                                , reduceOnly: true
                                                                                , workingType: WorkingType.Mark
                                                                                , callbackRate: 0.1M);*/

                                if (result.Success)//false
                                {
                                    realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD");
                                    var position = realOpenPos.Data.First();

                                    //DINAPOLI TARGETS
                                    //var stopLoss = Math.Max((double)(position.EntryPrice + (position.EntryPrice - goodP3)), (double)(goodP5 + 10M));
                                    //-----

                                    openPositions.Add(new binanceOpenFuture
                                    {
                                        startTime = DateTime.UtcNow,
                                        price = position.EntryPrice,
                                        riskRatio = (decimal)1.5,
                                        stopLimitPrice = position.EntryPrice * 1.01M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                        type = "short",
                                    });

                                    try
                                    {
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine($"SHORT OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        //Console.Write($"SHORT OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");

                                        //var TSL = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy
                                        //                                        , OrderType.TrailingStopMarket
                                        //                                        , quantity: investment
                                        //                                        , timeInForce: TimeInForce.GoodTillCancel
                                        //                                        , reduceOnly: true
                                        //                                        , workingType: WorkingType.Mark
                                        //                                        , price: goodP3//(decimal?)Math.Round((double)(position.EntryPrice * 0.998M), 1)
                                        //                                        , callbackRate: 0.1M);

                                        var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                            , OrderSide.Buy
                                                                                            , OrderType.StopMarket
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , stopPrice: (decimal?)Math.Round(stopLoss, 1)//(decimal?)Math.Round((double)(position.EntryPrice * 1.002M), 1)//Math.Round(position.EntryPrice * 1.00133M, 1)   //0.4% in the red
                                                                                            , workingType: WorkingType.Mark);

                                        var tkp = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                            , OrderSide.Buy
                                                                                            , OrderType.Limit
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , price: Math.Round(target1 + 20, 1)//(decimal?)Math.Round((double)(position.EntryPrice + (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                            , workingType: WorkingType.Mark);

                                        /*var tt0 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Take Profit
                                                                                            , OrderSide.Buy
                                                                                            , OrderType.Limit
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , price: (decimal?)Math.Round((double)(position.EntryPrice - (atrMult * 1.5M)), 1) //0.6% in the green
                                                                                            , workingType: WorkingType.Mark);

                                        var tt1 = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD" //Stop Loss
                                                                                            , OrderSide.Buy
                                                                                            , OrderType.StopMarket
                                                                                            , quantity: investment
                                                                                            , timeInForce: TimeInForce.GoodTillCancel
                                                                                            , reduceOnly: true
                                                                                            , stopPrice: (decimal?)Math.Round((double)(position.EntryPrice + atrMult), 1)//Math.Round(position.EntryPrice * 1.00133M, 1)   //0.4% in the red
                                                                                            , workingType: WorkingType.Mark);*/
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
                    }
                    Console.WriteLine("-------------------------");
                }

                //OUT OF EXISTING KLINE = NULL
                //CLEAR OPEN POSITIONS AFTER BINANCE POSITIONS CLEARED

                //openPositions.Add(new binanceOpenFuture());
                if (openPositions.Count > 0 && DateTime.UtcNow.Second % 10 == 0 && DateTime.UtcNow.Second != secondSwitch30)
                {
                    await Task.Run(async () => //Task.Run automatically unwraps nested Task types!
                    {
                        //Console.WriteLine("Start TWO SECONDS");
                        await Task.Delay(2000);
                        //Console.WriteLine("Done TWO SECONDS");
                    });
                    secondSwitch30 = DateTime.UtcNow.Second;
                    var posToClear = new BinancePositionDetailsUsdt();
                    var ordersToClear = 0;
                    bool success = true;
                    try
                    {
                        posToClear = client.FuturesUsdt.GetPositionInformationAsync("BTCBUSD").Result.Data.FirstOrDefault();
                        ordersToClear = client.FuturesUsdt.Order.GetOpenOrdersAsync("BTCBUSD").Result.Data.Count();
                    } catch (Exception ex)
                    {
                        success = false;
                        Console.WriteLine(ex);
                    }
                    if (success)
                    {
                        //Console.WriteLine("Trying to clear orders ...");
                        if (openPositions.Count > 0 && (posToClear == null || posToClear.Quantity == 0))
                        {
                            openPositions.Clear();
                        }
                        if (posToClear.Quantity == 0 && ordersToClear > 0)//(posToClear.EntryPrice == 0 || posToClear.EntryPrice == -1))
                        {
                            openPositions.Clear();
                            var closeOpenPosition = await client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCBUSD");
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
                }

                //var childOP = openPositions.FirstOrDefault();

                // Update the data of the object
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                //testRealOpenPositionsTrailingStopLoss(openPositions, BTCUSDTprice, client);

                if (candleStarted)
                {
                    candleStarted = false;

                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
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

            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://fapi.binance.com",
            });

            /*var listenKey = await client.FuturesUsdt.UserStream.StartUserStreamAsync(); //60 minutes

            if (listenKey.Success) {
                await socketClient.FuturesUsdt.SubscribeToUserDataUpdatesAsync(
                    listenKey.Data
                    , null
                    , null
                    , null
                    , orderData =>
                    {
                        Console.WriteLine(orderData.Data);
                    },
                    null//listenKey = await client.FuturesUsdt.UserStream.StartUserStreamAsync()
                    );
            }*/
            await socketClient.FuturesUsdt.SubscribeToSymbolMiniTickerUpdatesAsync("BTCBUSD", async data =>
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

        async private static Task liveStrat6()
        {
            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://api.binance.com",
            });

            var lineData = await GetAllKlinesInterval("BTCUSDT", KlineInterval.OneMinute, DateTime.Now.AddDays(-1), DateTime.Now);
            lineData = lineData.Skip(Math.Max(0, lineData.Count() - 300)).ToList(); //take only last 300 for optimization purposes
            bool candleStarted = false;

            int dir = 1;
            int? dirPrev = null;

            decimal? shortvs = null;
            decimal? longvs = null;
            decimal? shortvs1 = null;
            decimal? longvs1 = null;
            bool longswitch = false;
            bool shortswitch = false;
            var openPositions = new List<binanceOpenFuture>();

            var socketClient = new BinanceSocketClient();
            await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync("BTCUSDT", KlineInterval.OneMinute, async data =>
            {
                var existingKline = lineData.SingleOrDefault(k => k.OpenTime == data.Data.Data.OpenTime);
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
                    var lineSim = lineData.ToList();
                    lineData.Add(existingKline);
                    Console.WriteLine($"count lineSim = {lineSim.Count}, count lineData = {lineData.Count}");
                    lineData.RemoveAt(0); //when adding a new Kline, remove the first in the list so it stays the same length
                    Console.WriteLine($"Added new kline: {existingKline.OpenTime}-{existingKline.CloseTime} | Number of Klines: {lineData.Count}");

                    if (openPositions.Count == 1)
                    {
                        var childOP = openPositions.First();
                        if (childOP.type == "long")
                        {
                            childOP.stopLimitPrice = (decimal?)Math.Max((double)childOP.stopLimitPrice, (double)lineSim[lineSim.Count - 1].Low);
                        }
                        if (childOP.type == "short")
                        {
                            childOP.stopLimitPrice = (decimal?)Math.Min((double)childOP.stopLimitPrice, (double)lineSim[lineSim.Count - 1].High);
                        }
                        Console.WriteLine($"New StopLoss: {childOP.stopLimitPrice}");
                    }

                    //TEST OLD CANDLE TO SEE IF WE CAN GO IN
                    //CHANDELIER EXIT + ZLSMA                   

                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                    var mult = (decimal?)1.85;
                    var atr = quotes.GetAtr(2).ToList();

                    //ZLSMA CALCULATION
                    var lsma = quotes.GetEpma(32).ToList();
                    IEnumerable<Quote> quotes2 = GetClosesFromLsma(lsma);
                    var lsma2 = quotes2.GetEpma(32).ToList();
                    var zlsma = lsma[lsma.Count - 2].Epma + (lsma[lsma.Count - 2].Epma - lsma2[lsma2.Count - 2].Epma);

                    Console.WriteLine($"# Previous ATR: {atr[atr.Count - 1].Atr} | Previous Close/High/Low: {lineSim[lineSim.Count - 1].Close} {lineSim[lineSim.Count - 1].High} {lineSim[lineSim.Count - 1].Low}");

                    //verifica daca candela previous are semnal de buy/sell, intra la open-ul candle-ului curent

                    var short_stop = 0;// Math.Min(lineSim[lineSim.Count - 1].Low, lineSim[lineSim.Count - 2].Low) + (mult * atr[atr.Count - 1].Atr);
                    var long_stop = 0;// Math.Max(lineSim[lineSim.Count - 1].High, lineSim[lineSim.Count - 2].High) - (mult * atr[atr.Count - 1].Atr);

                    shortvs = (decimal?)(shortvs1 == null ? (double)short_stop : ((double)lineSim[lineSim.Count - 1].Close > (double)shortvs1 ? (double)short_stop : (double)Math.Min((double)short_stop, (double)shortvs1)));
                    longvs = (decimal?)(longvs1 == null ? (double)long_stop : ((double)lineSim[lineSim.Count - 1].Close < (double)longvs1 ? (double)long_stop : (double)Math.Max((double)long_stop, (double)longvs1)));


                    if (shortvs1 != null && longvs1 != null)
                    {
                        longswitch = lineSim[lineSim.Count - 1].Close >= shortvs1 && lineSim[lineSim.Count - 2].Close < shortvs1 ? true : false;
                        shortswitch = lineSim[lineSim.Count - 1].Close <= longvs1 && lineSim[lineSim.Count - 2].Close > longvs1 ? true : false;
                    }

                    if (shortvs1 != null && longvs1 != null)
                    {
                        if (dirPrev == null)
                        {
                            dir = 0;
                        }
                        else
                        {
                            if (dirPrev <= 0 && longswitch)
                            {
                                dir = 1;
                            }
                            else
                            {
                                if (dirPrev >= 0 && shortswitch)
                                {
                                    dir = -1;
                                }
                                else
                                {
                                    dir = (int)dirPrev;
                                }
                            }
                        }
                    }

                    //check if previous candle is entry point

                    var realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCUSDT");

                    //modify stoploss every candle close
                    //Console.WriteLine($"Position count: {realOpenPos.Data.Count()}");

                    if (realOpenPos.Success == true && realOpenPos.Data.First().Quantity == 0)
                    {
                        // check for LONG POSITION
                        //Console.WriteLine($"SIGNAL DATA: dir: {dir}, dirPrev: {dirPrev}, shortvs: {shortvs}, shortvs1: {shortvs1}, longvs: {longvs}, longvs1: {longvs1}");
                        if (dir > 0 && dirPrev <= 0 && shortvs1 != null && longvs1 != null
                        &&
                        true)//lineData[lineData.Count - 2].Close > zlsma) // && macdhH < 0
                        {
                            Console.WriteLine($"LONG SIGNAL!");
                            Console.WriteLine("Sending long ORDER request ...");

                            var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCUSDT", OrderSide.Buy, OrderType.Limit, price: BTCUSDTprice, quantity: 0.001M);

                            if (result.Success)
                            {
                                realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCUSDT");
                                var position = realOpenPos.Data.First();

                                openPositions.Add(new binanceOpenFuture
                                {
                                    startTime = position.UpdateTime,
                                    price = position.EntryPrice,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = position.EntryPrice * 0.999M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                    type = "long",
                                });

                                Console.WriteLine($"LONG OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                //Console.Write($"LONG OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                            }
                            else
                            {
                                Console.WriteLine($"Place order: {result.Error}");
                            }
                        }
                    }

                    if (realOpenPos.Success == true && realOpenPos.Data.First().Quantity == 0)
                    {
                        // check for SHORT POSITION
                        //Console.WriteLine($"SIGNAL DATA: dir: {dir}, dirPrev: {dirPrev}, shortvs: {shortvs}, shortvs1: {shortvs1}, longvs: {longvs}, longvs1: {longvs1}");
                        if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                        &&
                        true)//lineData[lineData.Count - 2].Close < zlsma) // && macdhH < 0
                        {
                            Console.WriteLine($"SHORT SIGNAL!");
                            Console.Write("Sending short position request ... ");

                            var result = await client.FuturesUsdt.Order.PlaceOrderAsync("BTCUSDT", OrderSide.Sell, OrderType.Market, 0.001M);

                            if (result.Success)
                            {
                                realOpenPos = await client.FuturesUsdt.GetPositionInformationAsync("BTCUSDT");
                                var position = realOpenPos.Data.First();

                                openPositions.Add(new binanceOpenFuture
                                {
                                    startTime = position.UpdateTime,
                                    price = position.EntryPrice,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = position.EntryPrice * 1.001M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                    type = "short",
                                });

                                Console.WriteLine($"SHORT OPEN SUCCESS - Timestamp: {position.UpdateTime} | Price: {position.EntryPrice}");
                                //Console.Write($"SHORT OPEN SUCCESS - Timestamp: {op.startTime} | Order ID: <NA> | Price: {op.price}");
                            } else
                            {
                                Console.WriteLine($"Place order: {result.Error}");
                            }
                        }
                    }

                    dirPrev = dir;
                    shortvs1 = shortvs;
                    longvs1 = longvs;
                }

                // Update the data of the object
                existingKline.High = data.Data.Data.High;
                existingKline.Low = data.Data.Data.Low;
                existingKline.Close = data.Data.Data.Close;
                existingKline.Open = data.Data.Data.Open;
                existingKline.TradeCount = data.Data.Data.TradeCount;
                existingKline.BaseVolume = data.Data.Data.BaseVolume;

                testRealOpenPositionsTrailingStopLoss(openPositions, BTCUSDTprice, client);

                if (candleStarted)
                {
                    candleStarted = false;

                }

                //Console.WriteLine($"Kline updated. Last price: {lineData.OrderByDescending(l => l.OpenTime).First().Close} | Date: {lineData.OrderByDescending(l => l.OpenTime).First().TradeCount}");
            });

            Console.ReadLine();
        }

        static List<decimal?> winTradesRsi = new List<decimal?>();
        static List<decimal?> lossTradesRsi = new List<decimal?>();

        private static decimal? backTestStrat6(List<IBinanceKline> lineData)
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(1450));
            //lineSim.AddRange(lineData);

            IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();
            var ema20 = quotesEma.GetEma(20).ToList();
            var ema100 = quotesEma.GetEma(100).ToList();
            var ema140 = quotesEma.GetEma(140).ToList();

            List<openFuture> openPositions = new List<openFuture>();
            int dir = 1;
            int? dirPrev = null;

            decimal? shortvs = null;
            decimal? longvs = null;
            decimal? shortvs1 = null;
            decimal? longvs1 = null;
            bool longswitch = false;
            bool shortswitch = false;


            for (int i = 1450; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                if (lineSim.Last().OpenTime == DateTime.Parse("12/01/2021 17:28:00 PM"))
                {
                    var hh = 0;
                }
                //Console.WriteLine($"Added {i}");
                //lineSim.RemoveAt(0); //USE WHEN NOT DOING FROM START DAY

                //var sk = openPositions.Count == 1;
                //int skip = testOpenPositions(openPositions, lineSim);
                /*if (sk && openPositions.Count == 0)
                {
                    continue;
                }*/

                /*for (int j = 0; j < skip && i < lineData.Count - 1; j++)
                {
                    lineSim.Add(lineData[++i]);
                    lineSim.RemoveAt(0);
                }*/

                testOpenPositionsTrailingStopLoss(openPositions, lineSim);

                //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 300)).ToList());
                IEnumerable<Quote> quotes = GetQuoteFromKlinesStartDay(lineSim, lineSim.Last().OpenTime.Date);

                if (quotes.Count() < 140) continue;

                //var mult = (decimal?)1.85;
                //var atr = quotes.GetAtr(2).ToList();
                var rsi = quotes.GetRsi(14).ToList();
                //var klt = quotes.GetKeltner(10, 0.85M, 10).ToList();
                var vwap = quotes.GetVwap(lineSim.Last().OpenTime.Date).ToList();

                //SLOPE CALCULATION

                Point p1 = new Point() { X = (i - 1) * 100, Y = (double)ema20[i - 1].Ema };
                Point p2 = new Point() { X = (i) * 100, Y = (double)ema20[i].Ema };

                var csp1p2 = CalculateSlope(p1, p2);

                //ZLSMA CALCULATION
                //var lsma = quotes.GetEpma(32).ToList();
                //IEnumerable<Quote> quotes2 = GetClosesFromLsma(lsma);
                //var lsma2 = quotes2.GetEpma(32).ToList();
                //var zlsma = lsma[lsma.Count - 2].Epma + (lsma[lsma.Count - 2].Epma - lsma2[lsma2.Count - 2].Epma);

                //testOpenPositionsZlsma(openPositions, lineSim, zlsma);


                /*var chandeLong = quotes.GetChandelier(2, (decimal)1.85, ChandelierType.Long).ToList();
                var chandeShort = quotes.GetChandelier(2, (decimal)1.85, ChandelierType.Short).ToList();

                var longStopPrev = (decimal?)(lineSim[lineSim.Count - 3].Close > chandeLong[chandeLong.Count - 3].ChandelierExit
                                    ? (decimal?)Math.Max((double)chandeLong[chandeLong.Count - 3].ChandelierExit, (double)chandeLong[chandeLong.Count - 4].ChandelierExit)
                                    : chandeLong[chandeLong.Count - 3].ChandelierExit);

                var shortStopPrev = (decimal?)(lineSim[lineSim.Count - 3].Close > chandeShort[chandeShort.Count - 3].ChandelierExit
                                    ? (decimal?)Math.Max((double)chandeShort[chandeShort.Count - 3].ChandelierExit, (double)chandeShort[chandeShort.Count - 4].ChandelierExit)
                                    : chandeShort[chandeShort.Count - 3].ChandelierExit);*/
                //-------------------------

                /*var short_stop = Math.Min(lineSim[lineSim.Count - 2].Low, lineSim[lineSim.Count - 3].Low) + (mult * atr[atr.Count - 2].Atr);
                var long_stop = Math.Max(lineSim[lineSim.Count - 2].High, lineSim[lineSim.Count - 3].High) - (mult * atr[atr.Count - 2].Atr);

                shortvs = (decimal?)(shortvs1 == null ? (double)short_stop : ((double)lineSim[lineSim.Count - 2].Close > (double)shortvs1 ? (double)short_stop : (double)Math.Min((double)short_stop, (double)shortvs1)));
                longvs = (decimal?)(longvs1 == null ? (double)long_stop : ((double)lineSim[lineSim.Count - 2].Close < (double)longvs1 ? (double)long_stop : (double)Math.Max((double)long_stop, (double)longvs1)));


                if (shortvs1 != null && longvs1 != null) {
                    longswitch = lineSim[lineSim.Count - 2].Close >= shortvs1 && lineSim[lineSim.Count - 3].Close < shortvs1 ? true : false;
                    shortswitch = lineSim[lineSim.Count - 2].Close <= longvs1 && lineSim[lineSim.Count - 3].Close > longvs1 ? true : false;
                }

                if (shortvs1 != null && longvs1 != null)
                {
                    if (dirPrev == null)
                    {
                        dir = 0;
                    }
                    else
                    {
                        if (dirPrev <= 0 && longswitch)
                        {
                            dir = 1;
                        }
                        else
                        {
                            if (dirPrev >= 0 && shortswitch)
                            {
                                dir = -1;
                            }
                            else
                            {
                                dir = (int)dirPrev;
                            }
                        }
                    }
                }

                //var pc = dir > 0 ? longvs : shortvs;

                //-------------------------

                //verifica daca candela previous are semnal, intra la open-ul candle-ului curent

                /*var longStop1 = Math.Max(lineSim[lineSim.Count - 3].High, lineSim[lineSim.Count - 2].High) - (atr[atr.Count - 2].Atr * mult);
                var longStopPrev1 = Math.Max(lineSim[lineSim.Count - 4].High, lineSim[lineSim.Count - 3].High) - (atr[atr.Count - 3].Atr * mult);
                var longStopPrev2 = Math.Max(lineSim[lineSim.Count - 5].High, lineSim[lineSim.Count - 4].High) - (atr[atr.Count - 4].Atr * mult);
                longStopPrev1 = (decimal?)(lineSim[lineSim.Count - 3].Close > longStopPrev1 ? Math.Max((double)longStopPrev1, (double)longStopPrev2) : (double)longStopPrev1);

                var shortStop1 = Math.Min(lineSim[lineSim.Count - 3].Low, lineSim[lineSim.Count - 2].Low) + (atr[atr.Count - 2].Atr * mult);
                var shortStopPrev1 = Math.Min(lineSim[lineSim.Count - 4].Low, lineSim[lineSim.Count - 3].Low) + (atr[atr.Count - 3].Atr * mult);
                var shortStopPrev2 = Math.Min(lineSim[lineSim.Count - 5].Low, lineSim[lineSim.Count - 4].Low) + (atr[atr.Count - 4].Atr * mult);
                shortStopPrev1 = (decimal?)(lineSim[lineSim.Count - 3].Close < shortStopPrev1 ? Math.Min((double)shortStopPrev1, (double)shortStopPrev2) : (double)shortStopPrev1);*/

                //var longStop2 = Math.Max(lineSim[lineSim.Count - 4].High, lineSim[lineSim.Count - 3].High) - (atr[atr.Count - 3].Atr * mult);
                //var longStopPrev2 = Math.Max(lineSim[lineSim.Count - 5].High, lineSim[lineSim.Count - 4].High) - (atr[atr.Count - 4].Atr * mult);

                //var shortStop2 = Math.Min(lineSim[lineSim.Count - 4].Low, lineSim[lineSim.Count - 3].Low) + (atr[atr.Count - 3].Atr * mult);
                //var shortStopPrev2 = Math.Min(lineSim[lineSim.Count - 5].Low, lineSim[lineSim.Count - 4].Low) + (atr[atr.Count - 4].Atr * mult);

                /*int dir = 1;
                dir = lineSim[lineSim.Count - 2].Close > chandeShort[chandeShort.Count - 2].ChandelierExit 
                    ? 1 : (lineSim[lineSim.Count - 2].Close < chandeLong[chandeLong.Count - 2].ChandelierExit ? -1 : dir);
                int dirPrev = 1;
                dirPrev = lineSim[lineSim.Count - 3].Close > chandeShort[chandeShort.Count - 3].ChandelierExit
                    ? 1 : (lineSim[lineSim.Count - 3].Close < chandeLong[chandeLong.Count - 3].ChandelierExit ? -1 : dirPrev);*/

                /*ASTA ERA BUN
                 * dir = lineSim[lineSim.Count - 2].Close > shortStopPrev1
                    ? 1 : (lineSim[lineSim.Count - 2].Close < longStopPrev1 ? -1 : dir);*/

                /*dir = lineSim[lineSim.Count - 2].Close > shortStopPrev
                    ? 1 : (lineSim[lineSim.Count - 2].Close < longStopPrev ? -1 : dir);*/

                /*dirPrev = lineSim[lineSim.Count - 4].Close > shortStopPrev2
                    ? 1 : (lineSim[lineSim.Count - 4].Close < longStopPrev2 ? -1 : dirPrev);*/

                ///---! dir[1] e dir cu toate alea care calculeaza dir-ul -1

                //INTRI CAND INCEPE UN OPEN PE UN CANDLE NOU SI VERIFICI TOTI PARAMETRII DE LA CLOSE-UL CANDLE-ULUI DE-ABIA TERMINAT
                // LONG check
                // See if EMA50 is on a bigger uptrend than EMA200
                // check if difference is at least 0.003%                

                /*if (dir > 0 && dirPrev <= 0 && shortvs1 != null && longvs1 != null
                    &&
                    lineSim[lineSim.Count - 2].Close > zlsma) // && macdhH < 0*/
                var currClose = (double)lineSim[lineSim.Count - 2].Close;

                if ((vwap[vwap.Count - 2].Vwap * 0.997 > currClose //rsi[rsi.Count - 2].Rsi < 30
                                                                   //&& currClose < ema20[i - 1].Ema
                    && ema20[i - 1].Ema < ema100[i - 1].Ema
                    && ema100[i - 1].Ema < ema140[i - 1].Ema
                    && csp1p2 >= 0) || rsi[rsi.Count - 2].Rsi < 20)
                {
                    //Console.WriteLine($"DateTime: {lineSim[lineSim.Count - 2].OpenTime} | Close: {lineSim[lineSim.Count - 2].Close} | VWAP: {vwap[vwap.Count - 2].Vwap} | RSI: {rsi[rsi.Count - 2].Rsi}");
                    //Console.WriteLine($"Slope EMA 20: {csp1p2}");
                    if (openPositions.Count == 1 && false)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 2].Close, (decimal?)investment, 1, -1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += PnL;
                            winTradesRsi.Add(child.rsi);
                        } else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += PnL;
                            lossTradesRsi.Add(child.rsi);
                        }

                        openPositions.RemoveAt(0);
                    }

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"LONG SIGNAL!");
                        if (Math.Round((double)vwap[vwap.Count - 2].Vwap, 4) == 42763.5366)
                        {
                            var hgh = 0;
                        }
                        Console.WriteLine($"Test buying long ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = lineSim[lineSim.Count - 1].Open * 0.99M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                            type = "long",
                        });
                        var op = openPositions.Last();
                        op.targetPrice = 100000;
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                    &&
                    lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                if ((vwap[vwap.Count - 2].Vwap * 1.003 < currClose //rsi[rsi.Count - 2].Rsi > 70
                                                                   //&& currClose > ema20[i - 1].Ema
                    && ema20[i - 1].Ema > ema100[i - 1].Ema
                    && ema100[i - 1].Ema > ema140[i - 1].Ema
                    && csp1p2 <= 0) || rsi[rsi.Count - 2].Rsi > 80)
                {
                    //Console.WriteLine($"DateTime: {lineSim[lineSim.Count - 2].OpenTime} | Close: {lineSim[lineSim.Count - 2].Close} | VWAP: {vwap[vwap.Count - 2].Vwap} | RSI: {rsi[rsi.Count - 2].Rsi}");
                    //Console.WriteLine($"Slope EMA 20: {csp1p2}");
                    if (openPositions.Count == 1 && false)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 2].Close, (decimal?)investment, 1, 1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += PnL;
                            winTradesRsi.Add(child.rsi);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += PnL;
                            lossTradesRsi.Add(child.rsi);
                        }

                        openPositions.RemoveAt(0);
                    }

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"SHORT SIGNAL!");
                        Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = lineSim[lineSim.Count - 1].Open * 1.01M, // 0.2% //+ atr[atr.Count - 1].Atr * (decimal)1.5,
                            type = "short",
                        });
                        var op = openPositions.Last();
                        op.targetPrice = 100000;
                    }
                }
                dirPrev = dir;
                shortvs1 = shortvs;
                longvs1 = longvs;
            }
            return null;
        }

        private static decimal? backTestStrat7(List<IBinanceKline> lineData)
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(1450));
            //lineSim.AddRange(lineData);

            IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();
            var ema50 = quotesEma.GetEma(50).ToList();
            var ema100 = quotesEma.GetEma(100).ToList();
            var ema140 = quotesEma.GetEma(140).ToList();
            var adx = quotesEma.GetAdx(20).ToList();
            var rsi = quotesEma.GetRsi(20).ToList();

            List<openFuture> openPositions = new List<openFuture>();
            int dir = 1;
            int? dirPrev = null;

            decimal? shortvs = null;
            decimal? longvs = null;
            decimal? shortvs1 = null;
            decimal? longvs1 = null;
            bool longswitch = false;
            bool shortswitch = false;


            for (int i = 1450; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                if (lineSim.Last().OpenTime == DateTime.Parse("12/01/2021 17:28:00 PM"))
                {
                    var hh = 0;
                }

                testOpenPositionsTrailingStopLoss(openPositions, lineSim);

                //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 300)).ToList());
                //IEnumerable<Quote> quotes = GetQuoteFromKlinesStartDay(lineSim, lineSim.Last().OpenTime.Date);

                //if (quotes.Count() < 140) continue;

                //var mult = (decimal?)1.85;
                //var vwap = quotes.GetVwap(lineSim.Last().OpenTime.Date).ToList();

                //SLOPE CALCULATION

                //Point p1 = new Point() { X = (i - 1) * 100, Y = (double)ema20[i - 1].Ema };
                //Point p2 = new Point() { X = (i) * 100, Y = (double)ema20[i].Ema };

                //var csp1p2 = CalculateSlope(p1, p2);

                var currClose = lineSim[lineSim.Count - 2].Close;

                //LONG CHECK
                if (rsi[i - 1].Rsi >= 70
                    //&& ema50[i - 1].Ema < currClose
                    && adx[i - 1].Adx >= 25)
                {
                    if (openPositions.Count == 1 && false)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 2].Close, (decimal?)investment, 1, -1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += PnL;
                            winTradesRsi.Add(child.rsi);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += PnL;
                            lossTradesRsi.Add(child.rsi);
                        }

                        openPositions.RemoveAt(0);
                    }

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"LONG SIGNAL!");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = lineSim[lineSim.Count - 1].Open * 0.99M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                            type = "long",
                        });
                        var op = openPositions.Last();
                        op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                    &&
                    lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                if (rsi[i - 1].Rsi <= 30
                    //&& ema50[i - 1].Ema > currClose
                    && adx[i - 1].Adx >= 25)
                {
                    //Console.WriteLine($"DateTime: {lineSim[lineSim.Count - 2].OpenTime} | Close: {lineSim[lineSim.Count - 2].Close} | VWAP: {vwap[vwap.Count - 2].Vwap} | RSI: {rsi[rsi.Count - 2].Rsi}");
                    //Console.WriteLine($"Slope EMA 20: {csp1p2}");
                    if (openPositions.Count == 1 && false)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 2].Close, (decimal?)investment, 1, 1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += PnL;
                            winTradesRsi.Add(child.rsi);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += PnL;
                            lossTradesRsi.Add(child.rsi);
                        }

                        openPositions.RemoveAt(0);
                    }

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"SHORT SIGNAL!");
                        //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = lineSim[lineSim.Count - 1].Open * 1.01M, // 0.2% //+ atr[atr.Count - 1].Atr * (decimal)1.5,
                            type = "short",
                        });
                        var op = openPositions.Last();
                        op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                    }
                }
                dirPrev = dir;
                shortvs1 = shortvs;
                longvs1 = longvs;
            }
            return null;
        }

        private static decimal? backTestStrat8(List<IBinanceKline> lineData)
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(1450));
            //lineSim.AddRange(lineData);

            //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();


            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 1450; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                if (lineSim.Last().OpenTime == DateTime.Parse("12/01/2021 17:28:00 PM"))
                {
                    var hh = 0;
                }

                testOpenPositionsTrailingStopLoss(openPositions, lineSim);

                //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 300)).ToList());
                //IEnumerable<Quote> quotes = GetQuoteFromKlinesStartDay(lineSim, lineSim.Last().OpenTime.Date);

                IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 450)).ToList());
                var ema20 = quotesEma.GetEma(20).ToList();
                var ema200 = quotesEma.GetEma(200).ToList();
                var superTrend = quotesEma.GetSuperTrend(10, 3).ToList();

                //if (quotes.Count() < 140) continue;

                //var mult = (decimal?)1.85;
                //var vwap = quotes.GetVwap(lineSim.Last().OpenTime.Date).ToList();

                //SLOPE CALCULATION

                Point p1 = new Point() { X = (i - 2) * 100, Y = (double)ema20[ema20.Count - 2].Ema };
                Point p2 = new Point() { X = (i - 1) * 100, Y = (double)ema20[ema20.Count - 1].Ema };

                var csp1p2 = CalculateSlope(p1, p2);

                //var currClose = lineSim[lineSim.Count - 2].Close;

                //LONG CHECK
                if (superTrend[superTrend.Count - 2].UpperBand == null && superTrend[superTrend.Count - 3].LowerBand == null
                    //&& checkLastCandlesEMA(lineSim.Skip(Math.Max(0, lineSim.Count() - 4)).Take(2).ToList(), ema20.Skip(Math.Max(0, ema20.Count() - 4)).Take(2).ToList(), 1) 
                    //&& lineSim[lineSim.Count - 2].Low < ema20[ema20.Count - 2].Ema && lineSim[lineSim.Count - 2].High > ema20[ema20.Count - 2].Ema
                    //&& lineSim.Last().Open > ema200[ema200.Count - 2].Ema
                    //&& Math.Abs(csp1p2) > 0.1
                    ) //anteriorul sa fie intersectat cu EMA 20, inca 5 anterioare anteriorului sa fie > EMA20
                {
                    if (openPositions.Count == 1 && false)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 2].Close, (decimal?)investment, 1, -1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += PnL;
                            winTradesRsi.Add(child.rsi);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += PnL;
                            lossTradesRsi.Add(child.rsi);
                        }

                        openPositions.RemoveAt(0);
                    }

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"LONG SIGNAL!");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = lineSim[lineSim.Count - 1].Open * 0.99M, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                            type = "long",
                        });
                        var op = openPositions.Last();
                        op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                    &&
                    lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                if (superTrend[superTrend.Count - 2].LowerBand == null && superTrend[superTrend.Count - 3].UpperBand == null
                    //&& checkLastCandlesEMA(lineSim.Skip(Math.Max(0, lineSim.Count() - 4)).Take(2).ToList(), ema20.Skip(Math.Max(0, ema20.Count() - 4)).Take(2).ToList(), -1)
                    //&& lineSim[lineSim.Count - 2].Low < ema20[ema20.Count - 2].Ema && lineSim[lineSim.Count - 2].High > ema20[ema20.Count - 2].Ema
                    //&& lineSim.Last().Open < ema200[ema200.Count - 2].Ema //anteriorul sa fie intersectat cu EMA 20, inca 5 anterioare anteriorului sa fie > EMA20
                    //&& Math.Abs(csp1p2) > 0.1
                    )
                {
                    //Console.WriteLine($"DateTime: {lineSim[lineSim.Count - 2].OpenTime} | Close: {lineSim[lineSim.Count - 2].Close} | VWAP: {vwap[vwap.Count - 2].Vwap} | RSI: {rsi[rsi.Count - 2].Rsi}");
                    //Console.WriteLine($"Slope EMA 20: {csp1p2}");
                    if (openPositions.Count == 1 && false)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 2].Close, (decimal?)investment, 1, 1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += PnL;
                            winTradesRsi.Add(child.rsi);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += PnL;
                            lossTradesRsi.Add(child.rsi);
                        }

                        openPositions.RemoveAt(0);
                    }

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"SHORT SIGNAL!");
                        //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = lineSim[lineSim.Count - 1].Open * 1.01M, // 0.2% //+ atr[atr.Count - 1].Atr * (decimal)1.5,
                            type = "short",
                        });
                        var op = openPositions.Last();
                        op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                    }
                }
            }
            return null;
        }

        private static decimal? backTestStrat9(List<IBinanceKline> lineData) //31.02
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(1450));
            //lineSim.AddRange(lineData);

            //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();

            var sd = swingDetector(lineSim, 40);

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 1450; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                if (lineSim.Last().OpenTime == DateTime.Parse("12/01/2021 17:28:00 PM"))
                {
                    var hh = 0;
                }

                if (openPositions.Count == 1)
                {
                    var childOP = openPositions.First();
                    if (childOP.type == "long")
                    {
                        if (lineSim.Last().Low > childOP.price && childOP.stopLimitPrice < childOP.price)
                        {
                            childOP.stopLimitPrice = childOP.price;
                        }
                        childOP.stopLimitPrice = (decimal?)Math.Max((double)childOP.stopLimitPrice, (double)lineSim.Last().Low * 0.998);
                    }
                    if (childOP.type == "short")
                    {
                        if (lineSim.Last().High < childOP.price && childOP.stopLimitPrice > childOP.price)
                        {
                            childOP.stopLimitPrice = childOP.price;
                        }
                        childOP.stopLimitPrice = (decimal?)Math.Min((double)childOP.stopLimitPrice, (double)lineSim.Last().High * 1.002);
                    }
                    //Console.WriteLine($"New StopLoss: {childOP.stopLimitPrice}");
                }

                testOpenPositionsTrailingStopLoss(openPositions, lineSim);
                //testOpenPositions(openPositions, lineSim);

                //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 300)).ToList());
                //IEnumerable<Quote> quotes = GetQuoteFromKlinesStartDay(lineSim, lineSim.Last().OpenTime.Date);

                IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 450)).ToList());
                //var ema20 = quotesEma.GetEma(20).ToList();
                var ema50 = quotesEma.GetEma(50).ToList();
                var superTrend1 = quotesEma.GetSuperTrend(12, 3).ToList();
                var superTrend2 = quotesEma.GetSuperTrend(11, 2).ToList();
                var superTrend3 = quotesEma.GetSuperTrend(10, 1).ToList();
                var rsi = quotesEma.GetRsi().ToList();
                var ema200 = quotesEma.GetEma(200).ToList();
                //var stdDev = quotesEma.GetStdDevChannels

                //if (quotes.Count() < 140) continue;

                //var mult = (decimal?)1.85;
                //var vwap = quotes.GetVwap(lineSim.Last().OpenTime.Date).ToList();

                //SLOPE CALCULATION

                Point p1 = new Point() { X = (i - 2) * 100, Y = (double)ema50[ema50.Count - 2].Ema };
                Point p2 = new Point() { X = (i - 1) * 100, Y = (double)ema50[ema50.Count - 1].Ema };

                var csp1p2 = CalculateSlope(p1, p2);

                var currClose = lineSim[lineSim.Count - 2].Close;

                var st1 = superTrend1[superTrend1.Count - 2];
                var st2 = superTrend2[superTrend2.Count - 2];
                var st3 = superTrend3[superTrend3.Count - 2];

                List<decimal?> orderList = new List<decimal?>();

                //LONG CHECK
                if (st1.UpperBand == null && st2.UpperBand == null && st3.UpperBand == null
                    //&& (superTrend1[superTrend1.Count - 3].LowerBand == null || superTrend2[superTrend2.Count - 3].LowerBand == null || superTrend3[superTrend3.Count - 3].LowerBand == null)
                    //&& ema200[ema200.Count - 2].Ema > currClose
                    && csp1p2 >= 0.03
                    && rsi[rsi.Count - 2].Rsi <= 65
                    )
                {
                    orderList.Add(st1.LowerBand);
                    orderList.Add(st2.LowerBand);
                    orderList.Add(st3.LowerBand);

                    //orderList.Sort();
                    //if (!(orderList[0] * 1.001M > orderList[1] && orderList[1] * 1.001M > orderList[2])) continue;

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"LONG SIGNAL!");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = st2.LowerBand, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                            targetPrice = (decimal)((double)(lineSim[lineSim.Count - 1].Open) + ((double)lineSim[lineSim.Count - 1].Open - (double)st2.LowerBand) * 10),
                            type = "long",
                            slDiff = lineSim[lineSim.Count - 1].Open - st2.LowerBand
                        });
                        //Console.WriteLine($"OPENED POSITION LONG # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                    &&
                    lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                if (st1.LowerBand == null && st2.LowerBand == null && st3.LowerBand == null
                    //&& (superTrend1[superTrend1.Count - 3].UpperBand == null || superTrend2[superTrend2.Count - 3].UpperBand == null || superTrend3[superTrend3.Count - 3].UpperBand == null)
                    //&& ema200[ema200.Count - 2].Ema  < currClose
                    && csp1p2 <= -0.03
                    && rsi[rsi.Count - 2].Rsi >= 35
                    )
                {
                    orderList.Add(st1.UpperBand);
                    orderList.Add(st2.UpperBand);
                    orderList.Add(st3.UpperBand);

                    //orderList.Sort();
                    //if (!(orderList[0] * 1.001M > orderList[1] && orderList[1] * 1.001M > orderList[2])) continue;

                    if (openPositions.Count == 0)
                    {
                        Console.WriteLine($"SHORT SIGNAL!");
                        //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = (decimal)1.5,
                            stopLimitPrice = st2.UpperBand, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                            targetPrice = (decimal)((double)(lineSim[lineSim.Count - 1].Open) - ((double)st2.UpperBand - (double)lineSim[lineSim.Count - 1].Open) * 10),
                            type = "short",
                            slDiff = st2.UpperBand - lineSim[lineSim.Count - 1].Open
                        });
                        //Console.WriteLine($"OPENED POSITION SHORT # OPEN: {openPositions.First().price}, S/L: {openPositions.First().stopLimitPrice}, TARGET PRICE: {openPositions.First().targetPrice}, SL DIFF: {openPositions.First().slDiff}");
                        //Console.WriteLine($"OPENED POSITION SHORT # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                    }
                }
            }
            return null;
        }

        private static decimal? backTestStrat12(List<IBinanceKline> lineData, List<IBinanceKline> exitsData) //31.02
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(200));

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 200; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                if (lineSim.Last().OpenTime == DateTime.Parse("12/01/2021 17:28:00 PM"))
                {
                    var hh = 0;
                }

                //testOpenPositions(openPositions, lineSim);

                //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 300)).ToList());
                //IEnumerable<Quote> quotes = GetQuoteFromKlinesStartDay(lineSim, lineSim.Last().OpenTime.Date);

                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim.ToList());
                //var ema20 = quotesEma.GetEma(20).ToList();
                var lagRSI = GetLagRSI(quotes.ToList());
                var atr = quotes.GetAtr();
                decimal factor = (decimal)atr.Last().Atr * 0.2m;

                //LONG CHECK
                if (lagRSI[lagRSI.Count - 2].LagRSI > 20
                    && lagRSI[lagRSI.Count - 3].LagRSI <= 20)
                {

                    if (openPositions.Count == 0)
                    {
                        Console.Write($"LONG SIGNAL! -------------------------------------------------------------------------------------- CURRENT PROFIT: ");
                        Console.WriteLine(totalProfit.Value);

                        for (int P = 1; P <= 10; P++)
                        {
                            openPositions.Add(new openFuture
                            {
                                startTime = lineSim.Last().OpenTime,
                                price = lineSim[lineSim.Count - 1].Open,
                                riskRatio = (decimal)P,
                                stopLimitPrice = lineSim.Last().Open - factor,//lineSim[lineSim.Count - 1].Open * 0.998m, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                targetPrice = lineSim.Last().Open + (factor * ((decimal)P)),//lineSim[lineSim.Count - 1].Open * 1.004m,
                                slTpFactor = factor,
                                liquidated = false,
                                investment = investment / 20m,
                                type = "long",
                            });
                        }
                        //Console.WriteLine($"OPENED POSITION LONG # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                    &&
                    lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                if (lagRSI[lagRSI.Count - 2].LagRSI < 80
                    && lagRSI[lagRSI.Count - 3].LagRSI >= 80)
                {

                    if (openPositions.Count == 0)
                    {
                        Console.Write($"SHORT SIGNAL! -------------------------------------------------------------------------------------- CURRENT PROFIT: ");
                        Console.WriteLine(totalProfit.Value);
                        //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");
                        for (int P = 1; P <= 10; P++)
                        {
                            openPositions.Add(new openFuture
                            {
                                startTime = lineSim.Last().OpenTime,
                                price = lineSim[lineSim.Count - 1].Open,
                                riskRatio = (decimal)1.5,
                                stopLimitPrice = lineSim.Last().Open + factor,//lineSim[lineSim.Count - 1].Open * 1.002m, //- atr[atr.Count - 1].Atr * (decimal)1.5,
                                targetPrice = lineSim.Last().Open - (factor * ((decimal)P)),//lineSim[lineSim.Count - 1].Open * 0.996m,
                                slTpFactor = factor,
                                liquidated = false,
                                investment = investment / 20m,
                                type = "short",
                            });
                        }
                        //Console.WriteLine($"OPENED POSITION SHORT # OPEN: {openPositions.First().price}, S/L: {openPositions.First().stopLimitPrice}, TARGET PRICE: {openPositions.First().targetPrice}, SL DIFF: {openPositions.First().slDiff}");
                        //Console.WriteLine($"OPENED POSITION SHORT # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                    }
                }
                if (openPositions.Count > 0)
                {
                    var currentExits = exitsData.Where(it => it.OpenTime >= lineData[i].OpenTime && it.OpenTime <= lineData[i].CloseTime).ToList();
                    foreach (var childExit in currentExits)
                    {
                        checkOpenPositionsTargetStop(openPositions, childExit);
                        foreach (var childOP in openPositions) //DO THIS TO SET STOPLOSS TO BREAK EVEN
                                                               //TRY TO GET PROFITS AT FACTOR 1x, 2x, 3x, 4x (25% at each) !!!!!!!
                        {
                            if (childOP.type == "long")
                            {
                                if (childExit.High >= childOP.targetPrice - childOP.slTpFactor)
                                {
                                    int dex = openPositions.FindIndex(it => it == childOP);
                                    for (int op = dex; op < openPositions.Count; op++)
                                    {
                                        openPositions[op].stopLimitPrice = childOP.targetPrice - 2 * childOP.slTpFactor;
                                    }
                                    //Console.WriteLine("MODIFIED STOPLOSS TO BREAK EVEN");
                                }
                            }
                            if (childOP.type == "short")
                            {
                                if (childExit.Low <= childOP.targetPrice + childOP.slTpFactor)
                                {
                                    int dex = openPositions.FindIndex(it => it == childOP);
                                    for (int op = dex; op < openPositions.Count; op++)
                                    {
                                        openPositions[op].stopLimitPrice = childOP.targetPrice + 2 * childOP.slTpFactor;
                                    }
                                    //Console.WriteLine("MODIFIED STOPLOSS TO BREAK EVEN");
                                }
                            }
                            //Console.WriteLine($"New StopLoss: {childOP.stopLimitPrice}");
                        }
                    }
                }
            }
            return null;
        }

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

        private static decimal? backTestStrat11(List<IBinanceKline> lineData2, List<IBinanceKline> lineData) //31.02
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            //lineSim.AddRange(lineData.Take(1));
            //lineSim.AddRange(lineData);

            //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();

            //var sd = swingDetector(lineSim, 40);

            List<openFuture> openPositions = new List<openFuture>();

            IEnumerable<Quote> quotes = GetQuoteFromKlines(lineData.ToList());

            var lagRSI = GetLagRSI(quotes.ToList());

            var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 6, Deviation: 5, BackStep: 2);

            var pivots = zigZags.Where(it => it.PointType == "L" || it.PointType == "H").ToList();

            for (int i = 0; i < lineData.Count; i++)
            {

                if (openPositions.Count == 0)// && lineData[i].OpenTime == lineData[i].OpenTime.Date)
                {

                    var dpDex = dailyProfiles.FindIndex(it => it.start <= lineData[i].OpenTime.AddDays(-1) && it.end >= lineData[i].OpenTime.AddDays(-1));
                    if (dpDex == -1) continue;

                    //check if start of day
                    bool dayStart = lineData[i].OpenTime == lineData[i].OpenTime.Date;
                    

                    var dailyRange = lineData.Where(it
                        => it.OpenTime >= dailyProfiles[dpDex].start
                        && it.OpenTime <= dailyProfiles[dpDex].end);

                    var dailyMax = dailyRange.Max(it => it.High);
                    var dailyMin = dailyRange.Min(it => it.Low);

                    if ((dailyMax - dailyMin) * 0.4m < dailyProfiles[dpDex].valueArea.Item2 - dailyProfiles[dpDex].valueArea.Item1) continue;

                    var priceStart = lineData[i].Open;//lineData.First(it => it.OpenTime > dailyProfiles[dpDex].end).Open;

                    /*if (priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2)
                    {
                        Console.WriteLine("Start price {0} in Value Area {1}", priceStart, dailyProfiles[dpDex].valueArea);
                    }
                    else
                    {
                        Console.WriteLine("START PRICE NOT IN VALUE AREA");
                    }*/

                    var lowVA = dailyProfiles[dpDex].valueArea.Item1;
                    var highVA = dailyProfiles[dpDex].valueArea.Item2;

                    bool goodLongVA = false, goodShortVA = false;
                    //LONG: check if previous candle closed into value area

                    if (lineData[i].OpenTime == DateTime.Parse("11/16/2022 10:00:00 AM"))
                    {
                        var kk = 0;
                    }

                    if (!dayStart
                        && IsBetween(lineData[i - 1].Close, lowVA, highVA)
                        && lineData[i - 1].Open < lowVA)
                    {
                        goodLongVA = true;
                    }
                    //SHORT: check if previous candle closed into value area
                    if (!dayStart
                        && IsBetween(lineData[i - 1].Close, lowVA, highVA)
                        && lineData[i - 1].Open > highVA)
                    {
                        goodShortVA = true;
                    }

                    bool inVAbelPOC = priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2
                        && priceStart < dailyProfiles[dpDex].poc.Item1;
                    bool inVAaboPOC = priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2
                        && priceStart > dailyProfiles[dpDex].poc.Item2;

                    //LONG CHECK
                    if ((dayStart && inVAbelPOC) || (!dayStart && goodLongVA && inVAbelPOC))
                    {
                        if (true)
                        {
                            Console.WriteLine("-----");
                            Console.WriteLine("VA: {0}\nPOC: {1}\nCandle Open: {2}\n-----"
                                , dailyProfiles[dpDex].valueArea
                                , dailyProfiles[dpDex].poc
                                , lineData[i].OpenTime);

                            var entry = priceStart;
                            var target1 = (dailyProfiles[dpDex].poc.Item1 + dailyProfiles[dpDex].poc.Item2 + 0.01m) / 2;
                            var stopLoss = dailyProfiles[dpDex].valueArea.Item1; 
                            //stopLoss = Math.Max(stopLoss, entry - (target1 - entry));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"LONG SIGNAL BOUGHT!");
                            Console.ForegroundColor = ConsoleColor.White;
                            openPositions.Add(new openFuture
                            {
                                startTime = lineData[i].OpenTime,
                                price = priceStart,
                                riskRatio = (decimal)1,
                                stopLimitPrice = stopLoss,
                                targetPrice = target1,
                                type = "long",
                                //slDiff = lineSim[lineSim.Count - 1].Open - st2.LowerBand
                            });
                        }
                    }
                    //Console.WriteLine($"OPENED POSITION LONG # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");

                    // SHORT check
                    // See if EMA50 is on a bigger downtrend than EMA200
                    // check if difference is at least 0.003%
                    /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                        &&
                        lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                    if ((dayStart && inVAaboPOC) || (!dayStart && goodShortVA && inVAaboPOC))
                    {
                        if (true)
                        {
                            Console.WriteLine("-----");
                            Console.WriteLine("VA: {0}\nPOC: {1}\nCandle Open: {2}\n-----"
                                , dailyProfiles[dpDex].valueArea
                                , dailyProfiles[dpDex].poc
                                , lineData[i].OpenTime);

                            var entry = priceStart;
                            var target1 = (dailyProfiles[dpDex].poc.Item1 + dailyProfiles[dpDex].poc.Item2 + 0.01m) / 2;
                            var stopLoss = dailyProfiles[dpDex].valueArea.Item2; 
                            //stopLoss = Math.Min(stopLoss, entry + (entry - target1));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"SHORT SIGNAL BOUGHT!");
                            Console.ForegroundColor = ConsoleColor.White;
                            //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                            openPositions.Add(new openFuture
                            {
                                startTime = lineData[i].OpenTime,
                                price = priceStart,
                                riskRatio = (decimal)1,
                                stopLimitPrice = stopLoss,
                                targetPrice = target1,
                                type = "short",
                                //slDiff = st2.UpperBand - lineSim[lineSim.Count - 1].Open
                            });
                            //Console.WriteLine($"OPENED POSITION SHORT # OPEN: {openPositions.First().price}, S/L: {openPositions.First().stopLimitPrice}, TARGET PRICE: {openPositions.First().targetPrice}, SL DIFF: {openPositions.First().slDiff}");
                            //Console.WriteLine($"OPENED POSITION SHORT # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                        }
                    }

                }
                if (openPositions.Count > 0)
                {
                    checkOpenPositionsTargetStop(openPositions, lineData[i]);
                }
            }

            Console.WriteLine();

            return null;
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

        private static decimal? backTestStrat14(List<IBinanceKline> lineData) //31.02
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            //lineSim.AddRange(lineData.Take(1));
            //lineSim.AddRange(lineData);

            //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();

            //var sd = swingDetector(lineSim, 40);

            List<openFuture> openPositions = new List<openFuture>();
            List<zzResult> zList = new List<zzResult>();

            IEnumerable<Quote> quotes = GetQuoteFromKlines(lineData.ToList());

            var rsi = quotes.GetRsi(14).ToList();
            var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 10, Deviation: 5, BackStep: 2);
            //var pivots = zigZags.Where(it => it.PointType == "L" || it.PointType == "H").ToList();
            //var quotesTrim = quotes.Skip(1500).ToList();

            //ZigZagCalculator(quotes.ToList());

            var zigz = quotes.GetZigZag(EndType.HighLow, percentChange: 0.3m).ToList();
            var pivots = zigz.Where(it => it.PointType == "L" || it.PointType == "H").ToList();

            //newZZ
            //NewZZ(zList, quotesTrim);

            //var hhllFinder = new HHLLFinder(quotes.ToList(), depth: 10, deviation: 0.05m, backstep: 2);
            //var higherHighs = hhllFinder.FindHigherHighs();
            //var lowerLows = hhllFinder.FindLowerLows();

            //

            //var zzFinder = ZigZag2.GetZigZagPoints(quotesTrim, 10, 5, 2);

            //var zzPoints = ZigZagCalc(quotes.ToList(), 0.0005, 10, 2);
            //var zzPoints = zzFinder. FindZigzagPoints(quotes.ToList(), 10, 5, 2);

            //var finalPoints = quotes.Where((it, index) => zzPoints.Contains(index)).ToList();

            for (int i = 6; i < pivots.Count; i++)
            {
                if (pivots[i].PointType == "H"
                    && pivots[i - 2].PointType == "H"
                    && pivots[i - 4].PointType == "H")
                {
                    var rsi1 = rsi.First(it => it.Date == pivots[i - 4].Date).Rsi;
                    var rsi2 = rsi.First(it => it.Date == pivots[i - 2].Date).Rsi;
                    var rsi3 = rsi.First(it => it.Date == pivots[i - 0].Date).Rsi;

                    if (pivots[i].ZigZag > pivots[i - 2].ZigZag
                        && pivots[i - 2].ZigZag > pivots[i - 4].ZigZag
                        && pivots[i].ZigZag - pivots[i - 2].ZigZag < pivots[i - 2].ZigZag - pivots[i - 4].ZigZag
                        && rsi3 <= rsi2
                        && rsi2 >= 65)
                    {
                        Console.WriteLine($"Possible short @{pivots[i].Date} | {pivots[i - 2].Date} | {pivots[i - 4].Date}");
                    }
                }

                if (pivots[i].PointType == "L"
                    && pivots[i - 2].PointType == "L"
                    && pivots[i - 4].PointType == "L")
                {
                    var rsi1 = rsi.First(it => it.Date == pivots[i - 4].Date).Rsi;
                    var rsi2 = rsi.First(it => it.Date == pivots[i - 2].Date).Rsi;
                    var rsi3 = rsi.First(it => it.Date == pivots[i - 0].Date).Rsi;

                    if (pivots[i].ZigZag < pivots[i - 2].ZigZag
                        && pivots[i - 2].ZigZag < pivots[i - 4].ZigZag
                        && pivots[i - 2].ZigZag - pivots[i].ZigZag < pivots[i - 4].ZigZag - pivots[i - 2].ZigZag
                        && rsi3 >= rsi2
                        && rsi2 <= 35)
                    {
                        Console.WriteLine($"Possible long @{pivots[i].Date} | {pivots[i - 2].Date} | {pivots[i - 4].Date}");
                    }
                }
            }

            return null;
        }

        private static decimal? backTestStrat13(List<IBinanceKline> lineData, List<IBinanceKline> lineData2) //31.02
        {

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            //lineSim.AddRange(lineData.Take(1));
            //lineSim.AddRange(lineData);

            //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();

            //var sd = swingDetector(lineSim, 40);

            List<openFuture> openPositions = new List<openFuture>();

            IEnumerable<Quote> quotes = GetQuoteFromKlines(lineData.ToList());

            var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 10, Deviation: 5, BackStep: 2);

            var pivots = zigZags.Where(it => it.PointType == "L" || it.PointType == "H").ToList();

            /*for (int i = 1; i < pivots.Count; i++)
            {
                if (pivots[i].PointType == pivots[i - 1].PointType)
                {
                    if (pivots[i].PointType == "H")
                    {
                        ZigZagResult zzr = new ZigZagResult(pivots[i].Date.AddHours(-1));
                        zzr.PointType = "L";
                        zzr.ZigZag = lineData.First(it => it.CloseTime == zzr.Date).Low;
                        pivots.Insert(i, zzr);
                    }
                    if (pivots[i].PointType == "L")
                    {
                        ZigZagResult zzr = new ZigZagResult(pivots[i].Date.AddHours(-1));
                        zzr.PointType = "H";
                        zzr.ZigZag = lineData.First(it => it.CloseTime == zzr.Date).High;
                        pivots.Insert(i, zzr);
                    }
                }
            }*/

            for (int i = 0; i < pivots.Count - 1; i++)
            {
                string fs = pivots[i].PointType;
                string sc = pivots[i + 1].PointType;
                if (fs == sc)
                {
                    var zigz = lineData.Where(it => it.CloseTime > pivots[i].Date && it.CloseTime < pivots[i + 1].Date).ToList();

                    if (fs == "H")
                    {
                        var min = zigz.OrderBy(it => it.Low).FirstOrDefault();
                        if (min != null)
                        {
                            ZigZagResult zzr = new ZigZagResult(min.CloseTime);
                            zzr.PointType = "L";
                            zzr.ZigZag = min.Low;
                            pivots.Insert(i + 1, zzr);
                        }
                    }

                    if (fs == "L")
                    {
                        var max = zigz.OrderByDescending(it => it.High).FirstOrDefault();
                        if (max != null)
                        {
                            ZigZagResult zzr = new ZigZagResult(max.CloseTime);
                            zzr.PointType = "H";
                            zzr.ZigZag = max.High;
                            pivots.Insert(i + 1, zzr);
                        }
                    }
                }
            }

            for (int i = 2; i < pivots.Count; i++)
            {
                if (pivots[i].PointType == "L")
                {
                    if (pivots[i - 2].ZigZag < pivots[i].ZigZag) pivots[i].PointType = "HL";
                    else pivots[i].PointType = "LL";
                }
                if (pivots[i].PointType == "H")
                {
                    if (pivots[i - 2].ZigZag < pivots[i].ZigZag) pivots[i].PointType = "HH";
                    else pivots[i].PointType = "LH";
                }

                Console.WriteLine(String.Format("{0} @ {1}", pivots[i].PointType, pivots[i].Date));
            }

            // ------------------

            /*var LLPivots = pivots.Where(it => it.PointType == "LL").ToList();
            var HHPivots = pivots.Where(it => it.PointType == "HH").ToList();

            for (int i = 0; i < LLPivots.Count - 1; i++)
            {
                for (int j = i + 1; j < LLPivots.Count; j++)
                {
                    var a = LLPivots[i].ZigZag.Value;
                    var b = LLPivots[j].ZigZag.Value;

                    if (Math.Abs(a - b) / a <= 0.003M)
                    {
                        Console.WriteLine(String.Format("bottom Order Block at around: {0}", LLPivots[i].Date));
                    }
                }
            }

            for (int i = 0; i < HHPivots.Count - 1; i++)
            {
                for (int j = i + 1; j < HHPivots.Count; j++)
                {
                    var a = HHPivots[i].ZigZag.Value;
                    var b = HHPivots[j].ZigZag.Value;

                    if (Math.Abs(a - b) / a <= 0.003M)
                    {
                        Console.WriteLine(String.Format("topper Order Block at around: {0}", HHPivots[i].Date));
                    }
                }
            }*/

            // -------------------

            for (int i = 0; i < pivots.Count - 3; i++)
            {
                if (pivots[i].PointType == "LH" 
                    && (pivots[i + 1].PointType == "LL" || pivots[i + 1].PointType == "HL")
                    && (pivots[i + 2].PointType == "HH" || pivots[i + 2].PointType == "HH"))
                    //&& (pivots[i + 2].Date - pivots[i + 1].Date).TotalHours <= 9)
                {
                    var candle1 = lineData.Where(it => it.CloseTime == pivots[i + 1].Date).FirstOrDefault();
                    var candle2 = lineData.Where(it => it.CloseTime == pivots[i + 1].Date.AddHours(1)).FirstOrDefault();

                    if (candle1.Open >= candle2.Low && candle1.Open <= candle2.High
                        && candle1.Close >= candle2.Low && candle1.Close <= candle2.High)
                    {
                        Console.WriteLine(String.Format("engulfed bottom Order Block at around: {0}", pivots[i + 1].Date));
                    }
                }

                if (pivots[i].PointType == "HL"
                    && (pivots[i + 1].PointType == "HH" || pivots[i + 1].PointType == "LH")
                    && (pivots[i + 2].PointType == "LL" || pivots[i + 2].PointType == "LL"))
                    //&& (pivots[i + 2].Date - pivots[i + 1].Date).TotalHours <= 9)
                {
                    var candle1 = lineData.Where(it => it.CloseTime == pivots[i + 1].Date).FirstOrDefault();
                    var candle2 = lineData.Where(it => it.CloseTime == pivots[i + 1].Date.AddHours(1)).FirstOrDefault();

                    if (candle1.Open >= candle2.Low && candle1.Open <= candle2.High
                        && candle1.Close >= candle2.Low && candle1.Close <= candle2.High)
                    {
                        Console.WriteLine(String.Format("engulfed topper Order Block at around: {0}", pivots[i + 1].Date));
                    }
                }
            }

            for (int i = 0; i < lineData.Count; i++)
            {

                if (openPositions.Count == 0)// && lineData[i].OpenTime == lineData[i].OpenTime.Date)
                {

                    var dpDex = dailyProfiles.FindIndex(it => it.start <= lineData[i].OpenTime.AddDays(-1) && it.end >= lineData[i].OpenTime.AddDays(-1));
                    if (dpDex == -1) continue;

                    //check if start of day
                    bool dayStart = lineData[i].OpenTime == lineData[i].OpenTime.Date;


                    var dailyRange = lineData.Where(it
                        => it.OpenTime >= dailyProfiles[dpDex].start
                        && it.OpenTime <= dailyProfiles[dpDex].end);

                    var dailyMax = dailyRange.Max(it => it.High);
                    var dailyMin = dailyRange.Min(it => it.Low);

                    if ((dailyMax - dailyMin) * 0.4m < dailyProfiles[dpDex].valueArea.Item2 - dailyProfiles[dpDex].valueArea.Item1) continue;

                    var priceStart = lineData[i].Open;//lineData.First(it => it.OpenTime > dailyProfiles[dpDex].end).Open;

                    /*if (priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2)
                    {
                        Console.WriteLine("Start price {0} in Value Area {1}", priceStart, dailyProfiles[dpDex].valueArea);
                    }
                    else
                    {
                        Console.WriteLine("START PRICE NOT IN VALUE AREA");
                    }*/

                    var lowVA = dailyProfiles[dpDex].valueArea.Item1;
                    var highVA = dailyProfiles[dpDex].valueArea.Item2;

                    bool goodLongVA = false, goodShortVA = false;
                    //LONG: check if previous candle closed into value area

                    if (lineData[i].OpenTime == DateTime.Parse("11/16/2022 10:00:00 AM"))
                    {
                        var kk = 0;
                    }

                    if (!dayStart
                        && IsBetween(lineData[i - 1].Close, lowVA, highVA)
                        && lineData[i - 1].Open < lowVA)
                    {
                        goodLongVA = true;
                    }
                    //SHORT: check if previous candle closed into value area
                    if (!dayStart
                        && IsBetween(lineData[i - 1].Close, lowVA, highVA)
                        && lineData[i - 1].Open > highVA)
                    {
                        goodShortVA = true;
                    }

                    bool inVAbelPOC = priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2
                        && priceStart < dailyProfiles[dpDex].poc.Item1;
                    bool inVAaboPOC = priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2
                        && priceStart > dailyProfiles[dpDex].poc.Item2;

                    //LONG CHECK
                    if ((dayStart && inVAbelPOC) || (!dayStart && goodLongVA && inVAbelPOC))
                    {
                        if (true)
                        {
                            Console.WriteLine("-----");
                            Console.WriteLine("VA: {0}\nPOC: {1}\nCandle Open: {2}\n-----"
                                , dailyProfiles[dpDex].valueArea
                                , dailyProfiles[dpDex].poc
                                , lineData[i].OpenTime);

                            var entry = priceStart;
                            var target1 = (dailyProfiles[dpDex].poc.Item1 + dailyProfiles[dpDex].poc.Item2 + 0.01m) / 2;
                            var stopLoss = dailyProfiles[dpDex].valueArea.Item1;
                            //stopLoss = Math.Max(stopLoss, entry - (target1 - entry));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"LONG SIGNAL BOUGHT!");
                            Console.ForegroundColor = ConsoleColor.White;
                            openPositions.Add(new openFuture
                            {
                                startTime = lineData[i].OpenTime,
                                price = priceStart,
                                riskRatio = (decimal)1,
                                stopLimitPrice = stopLoss,
                                targetPrice = target1,
                                type = "long",
                                //slDiff = lineSim[lineSim.Count - 1].Open - st2.LowerBand
                            });
                        }
                    }
                    //Console.WriteLine($"OPENED POSITION LONG # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");

                    // SHORT check
                    // See if EMA50 is on a bigger downtrend than EMA200
                    // check if difference is at least 0.003%
                    /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                        &&
                        lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                    if ((dayStart && inVAaboPOC) || (!dayStart && goodShortVA && inVAaboPOC))
                    {
                        if (true)
                        {
                            Console.WriteLine("-----");
                            Console.WriteLine("VA: {0}\nPOC: {1}\nCandle Open: {2}\n-----"
                                , dailyProfiles[dpDex].valueArea
                                , dailyProfiles[dpDex].poc
                                , lineData[i].OpenTime);

                            var entry = priceStart;
                            var target1 = (dailyProfiles[dpDex].poc.Item1 + dailyProfiles[dpDex].poc.Item2 + 0.01m) / 2;
                            var stopLoss = dailyProfiles[dpDex].valueArea.Item2;
                            //stopLoss = Math.Min(stopLoss, entry + (entry - target1));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"SHORT SIGNAL BOUGHT!");
                            Console.ForegroundColor = ConsoleColor.White;
                            //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                            openPositions.Add(new openFuture
                            {
                                startTime = lineData[i].OpenTime,
                                price = priceStart,
                                riskRatio = (decimal)1,
                                stopLimitPrice = stopLoss,
                                targetPrice = target1,
                                type = "short",
                                //slDiff = st2.UpperBand - lineSim[lineSim.Count - 1].Open
                            });
                            //Console.WriteLine($"OPENED POSITION SHORT # OPEN: {openPositions.First().price}, S/L: {openPositions.First().stopLimitPrice}, TARGET PRICE: {openPositions.First().targetPrice}, SL DIFF: {openPositions.First().slDiff}");
                            //Console.WriteLine($"OPENED POSITION SHORT # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                        }
                    }

                }
                if (openPositions.Count > 0)
                {
                    checkOpenPositionsTargetStop(openPositions, lineData[i]);
                }
            }

            Console.WriteLine();

            return null;
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

        private static decimal? backTestStrat15(List<IBinanceKline> lineData, List<IBinanceKline> lineData2) //31.02
        {
            int length = 20; //Bollinger Bands length
            decimal mult = 2.0m; //Bollinger Bands multFactor
            int lengthKC = 20; //KC length
            decimal multKC = 1.5m; //KC multFactor

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();

            List<openFuture> openPositions = new List<openFuture>();

            IEnumerable<Quote> quotes = GetQuoteFromKlines(lineData.ToList());

            var partialQuotes = quotes.Take(quotes.Count() - 1).ToList();
            var highPivot = getPivotHighResistance(partialQuotes, 15, 15);
            var lowPivot = getPivotLowSupport(partialQuotes, 15, 15);

            // ----------------

            var BB = quotes.Use(CandlePart.Close).GetBollingerBands(length, (double)multKC);
            var KC = quotes.GetKeltner(lengthKC, (double)multKC);

            var bbKC = BB.Zip(KC, (x, y) => new Tuple<BollingerBandsResult, KeltnerResult>(x, y)).ToList();

            // sqzOn = (lowerBB > lowerKC) and(upperBB < upperKC)
            //sqzOff = (lowerBB < lowerKC) and(upperBB > upperKC)
            //noSqz = (sqzOn == false) and(sqzOff == false)

            var sqzOn = bbKC.Select(it => (it.Item1.LowerBand > it.Item2.LowerBand) && (it.Item1.UpperBand < it.Item2.UpperBand)).ToList();
            var sqzOff = bbKC.Select(it => (it.Item1.LowerBand < it.Item2.LowerBand) && (it.Item1.UpperBand > it.Item2.UpperBand)).ToList();
            var noSqz = sqzOn.Zip(sqzOff, (x, y) => new Tuple<bool, bool>(x, y)).ToList().Select(it => (it.Item1 == false) && (it.Item2 == false)).ToList();

            var val = new List<decimal>();
            for (int i = 0; i < quotes.Count(); i++)
            {
                if (i < lengthKC - 1)
                {
                    val.Add(0);
                    continue;
                }
                var highest = quotes.Skip(i - lengthKC + 1).Take(lengthKC).Max(it => it.High);
                var lowest = quotes.Skip(i - lengthKC + 1).Take(lengthKC).Min(it => it.Low);
                var sma = quotes.Skip(i - lengthKC + 1).Take(lengthKC).GetSma(lengthKC).Last().Sma;

                if (sma != null)
                {
                    decimal item = quotes.ToList()[i].Close - (((highest + lowest) / 2 + (decimal)sma.Value) / 2);
                    val.Add(item);
                }
                else
                {
                    val.Add(0);
                }
            }

            var linreg = new List<double>();
            double[] inputs = Enumerable.Range(0, 20)
                  .Select(i => (double)i)
                  .ToArray();
            OrdinaryLeastSquares ols = new OrdinaryLeastSquares();

            for (int i = 0; i < val.Count; i++)
            {
                if (i < lengthKC - 1)
                {
                    linreg.Add(0);
                } else
                {
                    var skip = i - lengthKC + 1;
                    var take = lengthKC;
                    var outputs = val.Skip(skip).Take(take).Select(it => (double)it).ToArray();
                    SimpleLinearRegression regression = ols.Learn(inputs, outputs);

                    linreg.Add(regression.Intercept + regression.Slope * (lengthKC - 1));
                }
            }

            var quotesList = quotes.ToList();

            //find all entries with stop/target
            for (int i = 40; i < linreg.Count; i++)
            {
                int orangeDots = 0;
                for (int j = i - 6; j < i; j++)
                {
                    if (sqzOn[j]) orangeDots++;
                }

                var pQ = quotes.Take(i - 1).ToList();
                var hP = getPivotHighResistance(pQ, 15, 15);
                var lP = getPivotLowSupport(pQ, 15, 15);

                //check Sell
                if (linreg[i] < linreg[i - 1] && sqzOn[i] == false && orangeDots > 3 && quotesList[i].Close < lP) //sell
                {
                    Console.WriteLine($"SELL SIGNAL @ TIME: {quotesList[i].Date} / Candle Low: {quotesList[i].Low}");
                }

                //check Buy
                if (linreg[i] > linreg[i - 1] && sqzOn[i] == false && orangeDots > 3 && quotesList[i].Close > hP) //sell
                {
                    Console.WriteLine($"BUY SIGNAL @ TIME: {quotesList[i].Date} / Candle High: {quotesList[i].High}");
                }
            }

            for (int i = 0; i < lineData.Count; i++)
            {

                if (openPositions.Count == 0)// && lineData[i].OpenTime == lineData[i].OpenTime.Date)
                {

                    var dpDex = dailyProfiles.FindIndex(it => it.start <= lineData[i].OpenTime.AddDays(-1) && it.end >= lineData[i].OpenTime.AddDays(-1));
                    if (dpDex == -1) continue;

                    //check if start of day
                    bool dayStart = lineData[i].OpenTime == lineData[i].OpenTime.Date;


                    var dailyRange = lineData.Where(it
                        => it.OpenTime >= dailyProfiles[dpDex].start
                        && it.OpenTime <= dailyProfiles[dpDex].end);

                    var dailyMax = dailyRange.Max(it => it.High);
                    var dailyMin = dailyRange.Min(it => it.Low);

                    if ((dailyMax - dailyMin) * 0.4m < dailyProfiles[dpDex].valueArea.Item2 - dailyProfiles[dpDex].valueArea.Item1) continue;

                    var priceStart = lineData[i].Open;//lineData.First(it => it.OpenTime > dailyProfiles[dpDex].end).Open;

                    /*if (priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2)
                    {
                        Console.WriteLine("Start price {0} in Value Area {1}", priceStart, dailyProfiles[dpDex].valueArea);
                    }
                    else
                    {
                        Console.WriteLine("START PRICE NOT IN VALUE AREA");
                    }*/

                    var lowVA = dailyProfiles[dpDex].valueArea.Item1;
                    var highVA = dailyProfiles[dpDex].valueArea.Item2;

                    bool goodLongVA = false, goodShortVA = false;
                    //LONG: check if previous candle closed into value area

                    if (lineData[i].OpenTime == DateTime.Parse("11/16/2022 10:00:00 AM"))
                    {
                        var kk = 0;
                    }

                    if (!dayStart
                        && IsBetween(lineData[i - 1].Close, lowVA, highVA)
                        && lineData[i - 1].Open < lowVA)
                    {
                        goodLongVA = true;
                    }
                    //SHORT: check if previous candle closed into value area
                    if (!dayStart
                        && IsBetween(lineData[i - 1].Close, lowVA, highVA)
                        && lineData[i - 1].Open > highVA)
                    {
                        goodShortVA = true;
                    }

                    bool inVAbelPOC = priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2
                        && priceStart < dailyProfiles[dpDex].poc.Item1;
                    bool inVAaboPOC = priceStart >= dailyProfiles[dpDex].valueArea.Item1 && priceStart <= dailyProfiles[dpDex].valueArea.Item2
                        && priceStart > dailyProfiles[dpDex].poc.Item2;

                    //LONG CHECK
                    if ((dayStart && inVAbelPOC) || (!dayStart && goodLongVA && inVAbelPOC))
                    {
                        if (true)
                        {
                            Console.WriteLine("-----");
                            Console.WriteLine("VA: {0}\nPOC: {1}\nCandle Open: {2}\n-----"
                                , dailyProfiles[dpDex].valueArea
                                , dailyProfiles[dpDex].poc
                                , lineData[i].OpenTime);

                            var entry = priceStart;
                            var target1 = (dailyProfiles[dpDex].poc.Item1 + dailyProfiles[dpDex].poc.Item2 + 0.01m) / 2;
                            var stopLoss = dailyProfiles[dpDex].valueArea.Item1;
                            //stopLoss = Math.Max(stopLoss, entry - (target1 - entry));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"LONG SIGNAL BOUGHT!");
                            Console.ForegroundColor = ConsoleColor.White;
                            openPositions.Add(new openFuture
                            {
                                startTime = lineData[i].OpenTime,
                                price = priceStart,
                                riskRatio = (decimal)1,
                                stopLimitPrice = stopLoss,
                                targetPrice = target1,
                                type = "long",
                                //slDiff = lineSim[lineSim.Count - 1].Open - st2.LowerBand
                            });
                        }
                    }
                    //Console.WriteLine($"OPENED POSITION LONG # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");

                    // SHORT check
                    // See if EMA50 is on a bigger downtrend than EMA200
                    // check if difference is at least 0.003%
                    /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                        &&
                        lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                    if ((dayStart && inVAaboPOC) || (!dayStart && goodShortVA && inVAaboPOC))
                    {
                        if (true)
                        {
                            Console.WriteLine("-----");
                            Console.WriteLine("VA: {0}\nPOC: {1}\nCandle Open: {2}\n-----"
                                , dailyProfiles[dpDex].valueArea
                                , dailyProfiles[dpDex].poc
                                , lineData[i].OpenTime);

                            var entry = priceStart;
                            var target1 = (dailyProfiles[dpDex].poc.Item1 + dailyProfiles[dpDex].poc.Item2 + 0.01m) / 2;
                            var stopLoss = dailyProfiles[dpDex].valueArea.Item2;
                            //stopLoss = Math.Min(stopLoss, entry + (entry - target1));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"SHORT SIGNAL BOUGHT!");
                            Console.ForegroundColor = ConsoleColor.White;
                            //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                            openPositions.Add(new openFuture
                            {
                                startTime = lineData[i].OpenTime,
                                price = priceStart,
                                riskRatio = (decimal)1,
                                stopLimitPrice = stopLoss,
                                targetPrice = target1,
                                type = "short",
                                //slDiff = st2.UpperBand - lineSim[lineSim.Count - 1].Open
                            });
                            //Console.WriteLine($"OPENED POSITION SHORT # OPEN: {openPositions.First().price}, S/L: {openPositions.First().stopLimitPrice}, TARGET PRICE: {openPositions.First().targetPrice}, SL DIFF: {openPositions.First().slDiff}");
                            //Console.WriteLine($"OPENED POSITION SHORT # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                        }
                    }

                }
                if (openPositions.Count > 0)
                {
                    checkOpenPositionsTargetStop(openPositions, lineData[i]);
                }
            }

            Console.WriteLine();

            return null;
        }


        private static decimal? backTestStrat10(List<IBinanceKline> lineData, List<IBinanceKline> vpData, bool useVP) //31.02
        {
            //VOLUME PROFILE
            bool candleStarted = false;

            int step = 25;
            for (int i = 0; i < 70000 - step; i += step)
            {
                var x = new VolumeProfile();
                x.interval = new Tuple<decimal, decimal>(i, (decimal)i + step - 0.01M);
                x.sum = 0;
                x.isInterestZone = false;
                VP.Add(x);
            }

            int rightBound = 0;
            int leftBound = 0;
            foreach (var child in vpData)
            {
                if (child.CloseTime >= lineData[151].CloseTime) break;

                if (child.OpenTime >= lineData[151].OpenTime.AddDays(-15))
                {
                    if (leftBound == 0) leftBound = rightBound;
                    var volumeToAdd = child.BaseVolume / Math.Max((child.High - child.Low), 1);
                    foreach (var dchild in VP)
                    {
                        if (child.Low <= dchild.interval.Item2 && dchild.interval.Item1 <= child.High)
                        {
                            dchild.sum += volumeToAdd;
                        }
                    }
                }
                rightBound++;
            }

            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(152));
            //lineSim.AddRange(lineData);

            //IEnumerable<Quote> quotesEma = GetQuoteFromKlines(lineData).ToList();

            //var sd = swingDetector(lineSim, 40);

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 152; i < lineData.Count; i++)
            {
                List<insaneEntry> inEntries = new List<insaneEntry>();  
                lineSim.Add(lineData[i]);
                lineSim.RemoveAt(0);

                //UPDATE VOLUME PROFILE
                for (; rightBound < vpData.Count && vpData[rightBound].CloseTime <= lineData[i].CloseTime; rightBound++, leftBound++)
                {
                    //Console.WriteLine("Removed volume profile candles: {0}", vpData[leftBound].OpenTime);
                    //Console.WriteLine("Added volume profile candles: {0}", vpData[rightBound].OpenTime);
                    var volumeToAdd = vpData[rightBound].BaseVolume / Math.Max((vpData[rightBound].High - vpData[rightBound].Low), 1);
                    foreach (var dchild in VP)
                    {
                        if (vpData[rightBound].Low <= dchild.interval.Item2 && dchild.interval.Item1 <= vpData[rightBound].High)
                        {
                            dchild.sum += volumeToAdd;
                        }
                    }

                    var volumeToSubtract = vpData[leftBound].BaseVolume / Math.Max((vpData[leftBound].High - vpData[leftBound].Low), 1);
                    foreach (var dchild in VP)
                    {
                        if (vpData[leftBound].Low <= dchild.interval.Item2 && dchild.interval.Item1 <= vpData[leftBound].High)
                        {
                            dchild.sum += volumeToSubtract;
                        }
                    }
                }
                //Console.WriteLine("");

                decimal down = 0;
                decimal up = 70000;
                int intcnt = 0;
                decimal sum = 0;
                decimal avg = 0;
                int ar = 30;

                for (int k = 0; k < ar; k++)
                {
                    avg += (VP[k].sum / ar);
                }
                int stu = 0;

                foreach (var child in VP)
                {
                    if (stu >= ar)
                    {
                        avg += VP[stu].sum / ar;
                        avg -= VP[stu - ar].sum / ar;
                    }
                    if (down <= child.interval.Item2 && child.interval.Item1 <= up)
                    {
                        if (child.sum > avg && child.sum > 300)
                        {
                            //    Console.Write("### "); 
                            child.isInterestZone = true;
                        }
                        else
                        {
                            //    Console.Write("--- "); 
                            child.isInterestZone = false;
                        }
                        /*Console.Write("{0} - {1}:  ", child.interval.Item1, child.interval.Item2);
                        for (int i = 0; i < child.sum / 100; i++)
                        {
                            Console.Write("#");
                        }
                        Console.WriteLine();*/
                    }
                    stu++;
                }

                if (lineSim.Last().OpenTime == DateTime.Parse("12/01/2021 17:28:00 PM"))
                {
                    var hh = 0;
                }

                //testOpenPositionsTrailingStopLoss(openPositions, lineSim);
                //testOpenPositions(openPositions, lineSim);
                if (openPositions.Count == 0)
                {
                    IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim.Skip(Math.Max(0, lineSim.Count() - 152)).Take(150).ToList());

                    var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 6, Deviation: 5, BackStep: 2);
                    //zigZags = zigZags.Skip(Math.Max(0, zigZags.Count() - 250)).ToList(); //15minutes
                    //zigZags = zigZags.Skip(Math.Max(0, zigZags.Count() - 120)).ToList();
                    //CalculateZigzags(quotes.ToList(), 12, 5);
                    //var zigZags = quotes.GetZigZag(EndType.HighLow, percentChange: 0.5M).ToList();

                    //var currClose = lineSim[lineSim.Count - 2].Close;

                    //List<decimal?> orderList = new List<decimal?>();

                    //PATTERN CHECKER

                    //var ptQuotes = lineSim.Skip(Math.Max(0, lineSim.Count() - 30)).ToList();
                    //zigZags = zigZags.Where(it => it.Date < Convert.ToDateTime("17 Aug 2022 08:00")).ToList();
                    var zzTemp = zigZags.Where(it => it.PointType == "H" || it.PointType == "L");
                    //var zz = zzTemp.Skip(Math.Max(0, zzTemp.Count() - 120)).ToList(); //15 minutes
                    var zz = zzTemp.Skip(Math.Max(0, zzTemp.Count() - 60)).ToList();

                    //Console.WriteLine("First ZigZag in testing: {0}", zz.FirstOrDefault() != null ? zz.First().Date : DateTime.UtcNow);

                    bool bullishBat = false;
                    bool bearishBat = false;
                    bool bullishAltBat = false;
                    bool bearishAltBat = false;
                    bool bullishButterfly = false;
                    bool bearishButterfly = false;
                    bool bullishCrab = false;
                    bool bearishCrab = false;
                    bool bullishDeepCrab = false;
                    bool bearishDeepCrab = false;
                    bool bullishGartley = false;
                    bool bearishGartley = false;
                    bool bullishShark = false;
                    bool bearishShark = false;
                    bool bullishCypher = false;
                    bool bearishCypher = false;
                    bool bullishNenStar = false;
                    bool bearishNenStar = false;
                    bool bullishNavarro200 = false;
                    bool bearishNavarro200 = false;
                    bool bullishAntiCrab = false;
                    bool bearishAntiCrab = false;
                    bool bullishAntiButterfly = false;
                    bool bearishAntiButterfly = false;
                    bool bullishAntiNenStar = false;
                    bool bearishAntiNenStar = false;
                    bool bullishAntiBat = false;
                    bool bearishAntiBat = false;
                    bool bullishAntiNewCypher = false;
                    bool bearishAntiNewCypher = false;
                    bool bullishAntiGartley = false;
                    bool bearishAntiGartley = false;
                    int countBull = 0;
                    int countBear = 0;
                    decimal? goodP1 = 0;
                    decimal? goodP2 = 0;
                    decimal? goodP3 = 0;
                    decimal? goodP4 = 0;
                    decimal? goodP5 = 0;
                    int md = 11; //max depth
                    int hFound = 0;

                    List<ZigZagPattern> genPat = new List<ZigZagPattern>();
                    var now = lineSim.Last().OpenTime;

                    for (int i1 = 0; i1 < zz.Count; i1++)
                    {
                        if ((now - zz[i1].Date).TotalHours > 120) continue;
                        for (int i2 = i1 + 1; i2 < Math.Min(i1 + md, zz.Count); i2++)
                        {
                            if ((now - zz[i2].Date).TotalHours > 120) continue;
                            if (zz[i1].PointType == zz[i2].PointType) continue;// || i2 > i1 + 30

                            for (int i3 = i2 + 1; i3 < Math.Min(i2 + md, zz.Count); i3++)
                            {
                                if ((now - zz[i3].Date).TotalHours > 120) continue;
                                if (zz[i2].PointType == zz[i3].PointType) continue;

                                for (int i4 = i3 + 1; i4 < Math.Min(i3 + md, zz.Count); i4++)
                                {
                                    if ((now - zz[i4].Date).TotalHours > 120) continue;
                                    if (zz[i3].PointType == zz[i4].PointType) continue;

                                    for (int i5 = zz.Count - 1; i5 < zz.Count; i5++)
                                    {
                                        int bull = 0;
                                        int bear = 0;

                                        if (zz[i4].PointType == zz[i5].PointType || i5 - i4 > md) continue;

                                        if (zz[i1].PointType == "L"
                                            && zz[i1].ZigZag < zz[i2].ZigZag
                                            && zz[i2].ZigZag > zz[i3].ZigZag
                                            && zz[i3].ZigZag < zz[i4].ZigZag
                                            && zz[i4].ZigZag > zz[i5].ZigZag) //
                                        {
                                            bullishBat = IsBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAltBat = IsAltBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishButterfly = IsButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishCrab = IsCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishDeepCrab = IsDeepCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishGartley = IsGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishShark = IsShark(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishCypher = IsCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishNenStar = IsNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiCrab = IsAntiCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiButterfly = IsAntiButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiNenStar = IsAntiNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiBat = IsAntiBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiNewCypher = IsAntiNewCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bullishAntiGartley = IsAntiGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);                                            
                                        }

                                        if (zz[i1].PointType == "H"
                                            && zz[i1].ZigZag > zz[i2].ZigZag
                                            && zz[i2].ZigZag < zz[i3].ZigZag
                                            && zz[i3].ZigZag > zz[i4].ZigZag
                                            && zz[i4].ZigZag < zz[i5].ZigZag) //
                                        {
                                            bearishBat = IsBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishAltBat = IsAltBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishButterfly = IsButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishCrab = IsCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishDeepCrab = IsDeepCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishGartley = IsGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishShark = IsShark(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishCypher = IsCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishNenStar = IsNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().High);
                                            bearishNavarro200 = IsNavarro200(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiCrab = IsAntiCrab(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiButterfly = IsAntiButterfly(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiNenStar = IsAntiNenStar(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiBat = IsAntiBat(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiNewCypher = IsAntiNewCypher(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low);
                                            bearishAntiGartley = IsAntiGartley(zz[i1].ZigZag, zz[i2].ZigZag, zz[i3].ZigZag, zz[i4].ZigZag, zz[i5].ZigZag);// lineSim.Last().Low); 
                                        }

                                        //##############################
                                        //Bat Harmonic Chart Pattern

                                        //##############################
                                        //ALT Bat Harmonic Chart Pattern

                                        //##############################
                                        //Butterfly Harmonic Chart Pattern

                                        //##############################
                                        //Crab Harmonic Chart Pattern

                                        //##############################
                                        //Deep Crab Harmonic Chart Pattern

                                        //##############################
                                        //Gartley Harmonic Chart Pattern

                                        //##############################
                                        //Shark Harmonic Chart Pattern

                                        //##############################
                                        //Cypher Harmonic Chart Pattern

                                        bool confirmation = false;
                                        //int bull = 0;
                                        //int bear = 0;
                                        bool log = false;

                                        if (bullishBat)
                                        {
                                            if (log) Console.Write("BULLISH BAT!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishBat)
                                        {
                                            if (log) Console.Write("BEARISH BAT!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAltBat)
                                        {
                                            if (log) Console.Write("BULLISH ALT BAT!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAltBat)
                                        {
                                            if (log) Console.Write("BEARISH ALT BAT!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishButterfly)
                                        {
                                            if (log) Console.Write("BULLISH BUTTERFLY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishButterfly)
                                        {
                                            if (log) Console.Write("BEARISH BUTTERFLY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishCrab)
                                        {
                                            if (log) Console.Write("BULLISH CRAB!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishCrab)
                                        {
                                            if (log) Console.Write("BEARISH CRAB!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishDeepCrab)
                                        {
                                            if (log) Console.Write("BULLISH DEEP CRAB!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishDeepCrab)
                                        {
                                            if (log) Console.Write("BEARISH DEEP CRAB!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishGartley)
                                        {
                                            if (log) Console.Write("BULLISH GARTLEY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishGartley)
                                        {
                                            if (log) Console.Write("BEARISH GARTLEY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishShark)
                                        {
                                            if (log) Console.Write("BULLISH SHARK!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishShark)
                                        {
                                            if (log) Console.Write("BEARISH SHARK!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishCypher)
                                        {
                                            if (log) Console.Write("BULLISH CYPHER!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishCypher)
                                        {
                                            if (log) Console.Write("BEARISH CYPHER!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishNavarro200)
                                        {
                                            if (log) Console.Write("BULLISH NAVARRO 200!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishNavarro200)
                                        {
                                            if (log) Console.Write("BEARISH NAVARRO 200!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiCrab)
                                        {
                                            if (log) Console.Write("BULLISH ANTI CRAB!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiCrab)
                                        {
                                            if (log) Console.Write("BEARISH ANTI CRAB!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiBat)
                                        {
                                            if (log) Console.Write("BULLISH ANTI BAT!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiBat)
                                        {
                                            if (log) Console.Write("BEARISH ANTI BAT!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiNewCypher)
                                        {
                                            if (log) Console.Write("BULLISH ANTI NEW CYPHER!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiNewCypher)
                                        {
                                            if (log) Console.Write("BEARISH ANTI NEW CYPHER!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiGartley)
                                        {
                                            if (log) Console.Write("BULLISH ANTI GARTLEY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiGartley)
                                        {
                                            if (log) Console.Write("BEARISH ANTI GARTLEY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiButterfly)
                                        {
                                            if (log) Console.Write("BULLISH ANTI BUTTERFLY!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiButterfly)
                                        {
                                            if (log) Console.Write("BEARISH ANTI BUTTERFLY!");
                                            confirmation = true; ++bear;
                                        }
                                        if (bullishAntiNenStar)
                                        {
                                            if (log) Console.Write("BULLISH ANTI NEN STAR!");
                                            confirmation = true; ++bull;
                                        }
                                        if (bearishAntiNenStar)
                                        {
                                            if (log) Console.Write("BEARISH ANTI NEN STAR!");
                                            confirmation = true; ++bear;
                                        }

                                        if (confirmation)
                                        {
                                            genPat.Add(new ZigZagPattern
                                            {
                                                p1 = zz[i1],
                                                p2 = zz[i2],
                                                p3 = zz[i3],
                                                p4 = zz[i4],
                                                p5 = zz[i5]
                                            });
                                        }
                                        if (confirmation && log)
                                        {
                                            Console.WriteLine(String.Format(" ~~~ {0}, {1}, {2}, {3}, {4}"
                                                , zz[i1].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i2].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i3].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i4].Date.ToString("dd/MM hh:mm tt")
                                                , zz[i5].Date.ToString("dd/MM hh:mm tt")));
                                            //i1 = i2 = i3 = i4 = i5 = zz.Count;
                                            //break;
                                        }

                                        if (bull > 0 && bear == 0)
                                        {
                                            //Console.WriteLine("INSANE BUY POSITION");
                                            countBull++;
                                        }
                                        if (bear > 0 && bull == 0)
                                        {
                                            countBear++;
                                            //Console.WriteLine("INSANE SELL POSITION");
                                        }

                                        bullishBat = false;
                                        bearishBat = false;
                                        bullishAltBat = false;
                                        bearishAltBat = false;
                                        bullishButterfly = false;
                                        bearishButterfly = false;
                                        bullishCrab = false;
                                        bearishCrab = false;
                                        bullishDeepCrab = false;
                                        bearishDeepCrab = false;
                                        bullishGartley = false;
                                        bearishGartley = false;
                                        bullishShark = false;
                                        bearishShark = false;
                                        bullishCypher = false;
                                        bearishCypher = false;
                                        bullishNenStar = false;
                                        bearishNenStar = false;
                                        bullishNavarro200 = false;
                                        bearishNavarro200 = false;
                                        bearishAntiCrab = false;
                                        bearishAntiButterfly = false;
                                        bearishAntiNenStar = false;
                                        bearishAntiBat = false;
                                        bearishAntiNewCypher = false;
                                        bearishAntiGartley = false;
                                    }
                                }

                            }
                        }
                    }

                    if (genPat.Count > 0)
                    {
                        if (genPat.First().p5.ZigZag > genPat.First().p4.ZigZag)
                        {
                            genPat = genPat.OrderByDescending(it => it.p3.ZigZag).ToList();
                        }
                        else
                        {
                            genPat = genPat.OrderBy(it => it.p3.ZigZag).ToList();
                        }
                    }
                    //genPat = genPat.OrderByDescending(it => it.p3.Date).ToList();
                    ZigZagPattern zzp = genPat.FirstOrDefault();
                    if (zzp != null)
                    {
                        goodP1 = zzp.p1.ZigZag; goodP2 = zzp.p2.ZigZag; goodP3 = zzp.p3.ZigZag; goodP4 = zzp.p4.ZigZag; goodP5 = zzp.p5.ZigZag;
                    }
                    //Console.WriteLine("Harmonic points: {0}, {1}, {2}, {3}, {4}", goodP1, goodP2, goodP3, goodP4, goodP5);

                    if (countBull > 5 && countBear == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("========== Current Kline Open Time: {0}\n", lineSim.Last().OpenTime);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write(String.Format("INSANE BUY POSITION {0} | countBull: {1} | countBear: {2} | "
                            , zz[zz.Count - 1].Date.ToString("dd/MM hh:mm tt")
                            , countBull
                            , countBear));
                        inEntries.Add(new insaneEntry { time = zz[zz.Count - 1].Date, type = "buy" }); //SCHIMBAT
                        Console.ForegroundColor = ConsoleColor.White;
                        hFound = countBull;
                    }

                    if (countBear > 5 && countBull == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("========== Current Kline Open Time: {0}\n", lineSim.Last().OpenTime);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(String.Format("INSANE SELL POSITION {0} | countBull: {1} | countBear: {2} | "
                            , zz[zz.Count - 1].Date.ToString("dd/MM hh:mm tt")
                            , countBull
                            , countBear));
                        inEntries.Add(new insaneEntry { time = zz[zz.Count - 1].Date, type = "sell" });//zz[zz.Count - 1].Date,  SCHIMBAT
                        Console.ForegroundColor = ConsoleColor.White;
                        hFound = countBear;
                    }

                    decimal BTCBUSDprice = lineSim.Last().Open;

                    var vpZone = VP.FirstOrDefault(it => it.interval.Item1 <= BTCBUSDprice && it.interval.Item2 >= BTCBUSDprice);
                    var vpIndex = VP.FindIndex(it => it.interval.Item1 <= BTCBUSDprice && it.interval.Item2 >= BTCBUSDprice);

                    bool goodVPLong = true;
                    bool goodVPShort = true;

                    for (int v = vpIndex - 1; v >= vpIndex - 5; v--)
                    {
                        if (VP[v].sum > VP[v + 1].sum)
                        {
                            goodVPLong = false;
                            break;
                        }
                    }

                    for (int v = vpIndex + 1; v <= vpIndex + 5; v++)
                    {
                        if (VP[v].sum > VP[v - 1].sum)
                        {
                            goodVPShort = false;
                            break;
                        }
                    }

                    //LONG CHECK
                    if (openPositions.Count == 0 && inEntries.Count > 0 && inEntries.First().type == "buy")
                    {
                        if (!useVP || (useVP && goodVPLong))//!vpZone.isInterestZone)
                        {
                            //orderList.Add(st1.LowerBand);
                            //orderList.Add(st2.LowerBand);
                            //orderList.Add(st3.LowerBand);

                            //orderList.Sort();
                            //if (!(orderList[0] * 1.001M > orderList[1] && orderList[1] * 1.001M > orderList[2])) continue;

                            var topPoint = Math.Max(goodP2.Value, goodP4.Value);
                            var lowPoint = Math.Min(goodP1.Value, Math.Min(goodP3.Value, goodP5.Value));
                            var entry = (goodP5 + ((topPoint - goodP5) * 0.1M)).Value;
                            var target1 = goodP3.Value;
                            var stopLoss = Math.Min((goodP5 * 0.999M).Value, (entry - (target1 - entry)));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            if ((BTCBUSDprice < goodP5 + ((goodP3 - goodP5) * 2 / 3)) && (target1 - BTCBUSDprice) > 50
                                && lineSim[lineSim.Count - 2].Low > stopLoss)//pChange > 0 && pChange < 0.4M && //inEntries.First().time.AddMilliseconds(1) == lineSim.Last().OpenTime)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"LONG SIGNAL BOUGHT!");
                                Console.ForegroundColor = ConsoleColor.White;
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 1].Open,
                                    riskRatio = (decimal)1,
                                    stopLimitPrice = stopLoss,
                                    targetPrice = target1,
                                    type = "long",
                                    //slDiff = lineSim[lineSim.Count - 1].Open - st2.LowerBand
                                });
                                //Console.WriteLine($"OPENED POSITION LONG # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("LONG SIGNAL AVOIDED!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        } else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("LONG AVOIDED: VOLUME PROFILE @ {0}", lineSim.Last().CloseTime.AddMilliseconds(100));
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }


                    // SHORT check
                    // See if EMA50 is on a bigger downtrend than EMA200
                    // check if difference is at least 0.003%
                    /*if (dir < 0 && dirPrev >= 0 && shortvs1 != null && longvs1 != null
                        &&
                        lineSim[lineSim.Count - 2].Close < zlsma) // && macdhH > 0*/
                    if (openPositions.Count == 0 && inEntries.Count > 0 && inEntries.First().type == "sell")
                    {
                        if (!useVP || (useVP && goodVPShort))//!vpZone.isInterestZone)
                        {
                            //orderList.Add(st1.UpperBand);
                            //orderList.Add(st2.UpperBand);
                            //orderList.Add(st3.UpperBand);

                            //orderList.Sort();
                            //if (!(orderList[0] * 1.001M > orderList[1] && orderList[1] * 1.001M > orderList[2])) continue;

                            var topPoint = Math.Max(goodP1.Value, Math.Max(goodP3.Value, goodP5.Value));
                            var lowPoint = Math.Min(goodP2.Value, goodP4.Value);
                            var entry = (goodP5 - ((goodP5 - lowPoint) * 0.1M)).Value;
                            var target1 = goodP3.Value;
                            var stopLoss = Math.Max((goodP5 * 1.001M).Value, entry + (entry - target1));
                            //Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", stopLoss, entry, target1);
                            if ((BTCBUSDprice > goodP5 - ((goodP5 - goodP3) * 2 / 3)) && (BTCBUSDprice - target1) > 50
                                && lineSim[lineSim.Count - 2].High < stopLoss)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"SHORT SIGNAL BOUGHT!");
                                Console.ForegroundColor = ConsoleColor.White;
                                //Console.WriteLine($"Test buying short ... previous VWAP is: {Math.Round((double)vwap[vwap.Count - 2].Vwap, 4)}");

                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 1].Open,
                                    riskRatio = (decimal)1,
                                    stopLimitPrice = stopLoss,
                                    targetPrice = target1,
                                    type = "short",
                                    //slDiff = st2.UpperBand - lineSim[lineSim.Count - 1].Open
                                });
                                //Console.WriteLine($"OPENED POSITION SHORT # OPEN: {openPositions.First().price}, S/L: {openPositions.First().stopLimitPrice}, TARGET PRICE: {openPositions.First().targetPrice}, SL DIFF: {openPositions.First().slDiff}");
                                //Console.WriteLine($"OPENED POSITION SHORT # TIME: {openPositions.First().startTime} | OPEN: {Math.Round(openPositions.First().price, 2)}, S/L: {Math.Round((double)openPositions.First().stopLimitPrice, 2)}, TARGET PRICE: {Math.Round(openPositions.First().targetPrice, 2)}, SL DIFF: {Math.Round((double)openPositions.First().slDiff, 2)}");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"SHORT SIGNAL AVOIDED!");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("SHORT AVOIDED: VOLUME PROFILE @ {0}", lineSim.Last().CloseTime.AddMilliseconds(100));
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                }
                if (openPositions.Count > 0)
                {
                    checkOpenPositionsTargetStop(openPositions, lineSim.Last());
                }
                if (inEntries.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n========== Current Kline Ended\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            Console.WriteLine();

            return null;
        }

        private static void checkOpenPositionsTargetStop(List<openFuture> openPositions, IBinanceKline binanceKline)
        {
            foreach (var childpos in openPositions)
            {
                if (childpos.liquidated) continue;
                //check target met
                var tp = childpos.targetPrice;
                var sl = childpos.stopLimitPrice;
                var ep = childpos.price;
                var dir = childpos.type == "long" ? 1 : -1;
                var startTime = childpos.startTime;
                if (!childpos.liquidated && tp >= binanceKline.Low && tp <= binanceKline.High)
                {
                    //target met
                    var PnL = calculatePnL(ep, tp, childpos.investment, 1, dir);
                    if (PnL > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Win for position! Started: {startTime} | Won: {Math.Round((double)(PnL - (childpos.investment * ep * fee * 1.5M / 100)), 2)} | Entry: {Math.Round(ep, 2)} | Got out: {Math.Round((double)tp, 2)} | TIME @ {binanceKline.OpenTime}");
                        Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", sl, ep, tp);
                        Console.ForegroundColor = ConsoleColor.White;
                        ++wins;
                        totalProfit += (PnL - (childpos.investment * ep * fee * 2 / 100));
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Loss for position! Started: {startTime} | Lost: {Math.Round((double)(PnL - (childpos.investment * ep * fee * 1.5M / 100)), 2)} | Entry: {Math.Round(ep, 2)} | Got out: {Math.Round((double)tp, 2)} | TIME @ {binanceKline.OpenTime}");
                        Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", sl, ep, tp);
                        Console.ForegroundColor = ConsoleColor.White;
                        ++losses;
                        totalProfit += (PnL - (childpos.investment * ep * fee * 2 / 100));
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }

                    childpos.liquidated = true;
                }
                else
                {
                    if (!childpos.liquidated && sl >= binanceKline.Low && sl <= binanceKline.High)
                    {
                        //STOPLOSS met
                        var PnL = calculatePnL(ep, sl, childpos.investment, 1, dir);
                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {startTime} | Won: {Math.Round((double)(PnL - (childpos.investment * ep * fee * 2 / 100)), 2)} | Entry: {Math.Round(ep, 2)} | Got out: {Math.Round((double)sl, 2)} | TIME @ {binanceKline.OpenTime}");
                            Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", sl, ep, tp);
                            Console.ForegroundColor = ConsoleColor.White;
                            ++wins;
                            totalProfit += (PnL - (childpos.investment * ep * fee * 2 / 100));
                            //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {startTime} | Lost: {Math.Round((double)(PnL - (childpos.investment * ep * fee * 2 / 100)), 2)} | Entry: {Math.Round(ep, 2)} | Got out: {Math.Round((double)sl, 2)} | TIME @ {binanceKline.OpenTime}");
                            Console.WriteLine("##########STOPLOSS: {0} | ENTRY: {1} | TARGET1: {2}##########", sl, ep, tp);
                            Console.ForegroundColor = ConsoleColor.White;
                            ++losses;
                            totalProfit += (PnL - (childpos.investment * ep * fee * 2 / 100));
                            //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                        }

                        childpos.liquidated = true;
                    }
                }
            }

            openPositions.RemoveAll(it => it.liquidated == true);
        }

        private static List<Swing> swingDetector(List<IBinanceKline> lineSim, int sL)
        {
            var swings = new List<Swing>();
            for (int i = sL; i < lineSim.Count() - sL; i++)
            {
                bool swingHigh = true, swingLow = true;

                //detect swing high/low
                for (int j = i - sL; j < i; j++)
                {
                    if (lineSim[j].High > lineSim[i].High) swingHigh = false; // * 1.0001M 
                    if (lineSim[j].Low < lineSim[i].Low) swingLow = false; // * 0.9999M
                }

                for (int j = i + 1; j < i + sL + 1; j++)
                {
                    if (lineSim[j].High > lineSim[i].High) swingHigh = false; // * 1.0001M
                    if (lineSim[j].Low < lineSim[i].Low) swingLow = false; // * 0.9999M
                }

                if (swingHigh) swings.Add(new Swing(lineSim[i], 1));
                if (swingLow) swings.Add(new Swing(lineSim[i], -1));
            }

            return swings;
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

        private static void testRealOpenPositionsTrailingStopLoss(List<binanceOpenFuture> openPositions, decimal? price, BinanceClient client)
        {
            if (openPositions.Count == 1 && openPositions.First().type == "long")
            {
                if (price < openPositions.First().stopLimitPrice) //test if Low candle got under stop loss
                {
                    var child = openPositions.First();
                    var PnL = calculatePnL(child.price, child.stopLimitPrice, (decimal?)investment, 1, 1);

                    if (PnL > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)(PnL - (investment * child.price * fee * 2 / 100)), 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {price}");
                        Console.ForegroundColor = ConsoleColor.White;
                        ++wins;
                        totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)(PnL - (investment * child.price * fee * 2 / 100)), 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {price}");
                        Console.ForegroundColor = ConsoleColor.White;
                        ++losses;
                        totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }

                    //var closeOpenPosition = client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCUSDT").Result;
                    var closeOpenPosition = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Sell, OrderType.Limit, price: BTCUSDTprice, quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true).Result;
                    openPositions.Remove(child);

                    if (closeOpenPosition.Success == true)
                    {
                        Console.WriteLine($"Close position! Status: {closeOpenPosition.Data.Status} | At price: {closeOpenPosition.Data.Price}");                        
                    }
                    else
                    {
                        Console.WriteLine($"Position couldn't close! ERROR: {closeOpenPosition.Error}");
                    }
                }
            }

            if (openPositions.Count == 1 && openPositions.First().type == "short")
            {
                if (price > openPositions.First().stopLimitPrice) //test if Low candle got under stop loss
                {
                    var child = openPositions.First();
                    var PnL = calculatePnL(child.price, child.stopLimitPrice, (decimal?)investment, 1, -1);

                    if (PnL > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {price}");
                        Console.ForegroundColor = ConsoleColor.White;
                        ++wins;
                        totalProfit += PnL;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {price}");
                        Console.ForegroundColor = ConsoleColor.White;
                        ++losses;
                        totalProfit += PnL;
                    }

                    
                    //var closeOpenPosition = client.FuturesUsdt.Order.CancelAllOrdersAsync("BTCUSDT").Result;
                    var closeOpenPosition = client.FuturesUsdt.Order.PlaceOrderAsync("BTCBUSD", OrderSide.Buy, OrderType.Limit, price: BTCUSDTprice, quantity: investment, timeInForce: TimeInForce.GoodTillCancel, reduceOnly: true).Result;
                    openPositions.Remove(child);

                    if (closeOpenPosition.Success == true)
                    {
                        Console.WriteLine($"Close position! Status: {closeOpenPosition.Data.Status} | At price: {closeOpenPosition.Data.Price}");
                    }
                    else
                    {
                        Console.WriteLine($"Position couldn't close! ERROR: {closeOpenPosition.Error}");
                    }

                }
            }
        }

        private static void testOpenPositionsTrailingStopLoss(List<openFuture> openPositions, List<IBinanceKline> lineSim)
        {
            if (openPositions.Count == 1 && openPositions.First().type == "long")
            {
                if (lineSim.Last().Low < openPositions.First().stopLimitPrice) //test if Low candle got under stop loss
                {
                    var child = openPositions.First();
                    var PnL = calculatePnL(child.price, child.stopLimitPrice, (decimal?)investment, 1, 1);
                    if (PnL > 100)
                    {
                        var yyy = 0;
                    }
                    if (PnL > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)(PnL - (investment * child.price * fee * 2 / 100)), 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round((double)child.stopLimitPrice, 2)}");
                        Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                        Console.ForegroundColor = ConsoleColor.White;
                        winTradesRsi.Add(child.rsi);
                        ++wins;
                        totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)(PnL - (investment * child.price * fee * 2 / 100)), 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round((double)child.stopLimitPrice, 2)}");
                        Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                        Console.ForegroundColor = ConsoleColor.White;
                        lossTradesRsi.Add(child.rsi);
                        ++losses;
                        totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }

                    openPositions.RemoveAt(0);

                } else
                {
                    if (lineSim.Last().High > openPositions.First().targetPrice) //test if Low candle got under stop loss
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, child.targetPrice, (decimal?)investment, 1, 1);
                        if (PnL > 100)
                        {
                            var yyy = 0;
                        }
                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)(PnL - (investment * child.price * fee * 2 / 100)), 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(child.targetPrice, 2)}");
                            Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                            Console.ForegroundColor = ConsoleColor.White;
                            winTradesRsi.Add(child.rsi);
                            ++wins;
                            totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                            //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)(PnL - (investment * child.price * fee * 2 / 100)), 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(child.targetPrice, 2)}");
                            Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                            Console.ForegroundColor = ConsoleColor.White;
                            lossTradesRsi.Add(child.rsi);
                            ++losses;
                            totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                            //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                        }

                        openPositions.RemoveAt(0);

                    }
                    else
                    {
                        //openPositions.First().stopLimitPrice = (decimal?)Math.Max((double)openPositions.First().stopLimitPrice, Math.Min((double)lineSim[lineSim.Count - 2].Low, (double)lineSim[lineSim.Count - 1].Low));
                        //openPositions.First().stopLimitPrice = (decimal?)Math.Max((double)openPositions.First().stopLimitPrice, (double)lineSim[lineSim.Count - 1].High - (double)openPositions.First().slDiff);
                    }
                }
            }

            if (openPositions.Count == 1 && openPositions.First().type == "short")
            {
                if (lineSim.Last().High > openPositions.First().stopLimitPrice) //test if Low candle got under stop loss
                {
                    var child = openPositions.First();
                    var PnL = calculatePnL(child.price, child.stopLimitPrice, (decimal?)investment, 1, -1);
                    if (PnL > 100)
                    {
                        var yyy = 0;
                    }
                    if (PnL > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round((double)child.stopLimitPrice, 2)}");
                        Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                        Console.ForegroundColor = ConsoleColor.White;
                        winTradesRsi.Add(child.rsi);
                        ++wins;
                        totalProfit += PnL;
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round((double)child.stopLimitPrice, 2)}");
                        Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                        Console.ForegroundColor = ConsoleColor.White;
                        lossTradesRsi.Add(child.rsi);
                        ++losses;
                        totalProfit += PnL;
                        //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                    }

                    openPositions.RemoveAt(0);

                }
                else
                {
                    if (lineSim.Last().Low < openPositions.First().targetPrice) //test if Low candle got under stop loss
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, child.targetPrice, (decimal?)investment, 1, -1);
                        if (PnL > 100)
                        {
                            var yyy = 0;
                        }
                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(child.targetPrice, 2)}");
                            Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                            Console.ForegroundColor = ConsoleColor.White;
                            winTradesRsi.Add(child.rsi);
                            ++wins;
                            totalProfit += PnL;
                            //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(child.targetPrice, 2)}");
                            Console.WriteLine($"LAST STOPLOSS WAS: {child.stopLimitPrice} | SL DIFF WAS: {child.slDiff}");
                            Console.ForegroundColor = ConsoleColor.White;
                            lossTradesRsi.Add(child.rsi);
                            ++losses;
                            totalProfit += PnL;
                            //investment += (decimal)(PnL / child.stopLimitPrice); //do it with all money added
                        }

                        openPositions.RemoveAt(0);

                    }
                    else
                    {
                        //openPositions.First().stopLimitPrice = (decimal?)Math.Min((double)openPositions.First().stopLimitPrice, Math.Max((double)lineSim[lineSim.Count - 2].High, (double)lineSim[lineSim.Count - 1].High));
                        //openPositions.First().stopLimitPrice = (decimal?)Math.Min((double)openPositions.First().stopLimitPrice, (double)lineSim[lineSim.Count - 1].Low + (double)openPositions.First().slDiff);
                    }
                }
            }
        }

        private static void testOpenPositionsZlsma(List<openFuture> openPositions, List<IBinanceKline> lineSim, decimal? zlsma)
        {
            if (openPositions.Count == 1 && openPositions.First().type == "long")
            {
                if (lineSim.Last().Close < lineSim.Last().Open) //test if its a red candle
                {
                    if (lineSim.Last().Close < zlsma)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 1].Close, (decimal?)investment, 1, 1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            winTradesRsi.Add(child.rsi);
                            ++wins;
                            totalProfit += PnL;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            lossTradesRsi.Add(child.rsi);
                            ++losses;
                            totalProfit += PnL;
                        }

                        openPositions.RemoveAt(0);
                    }
                }
            }

            if (openPositions.Count == 1 && openPositions.First().type == "short")
            {
                if (lineSim.Last().Close > lineSim.Last().Open) //test if its a green candle
                {
                    if (lineSim.Last().Close > zlsma)
                    {
                        var child = openPositions.First();
                        var PnL = calculatePnL(child.price, lineSim[lineSim.Count - 1].Close, (decimal?)investment, 1, -1);

                        if (PnL > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            winTradesRsi.Add(child.rsi);
                            ++wins;
                            totalProfit += PnL;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Got out: {Math.Round(lineSim[lineSim.Count - 2].Close, 2)}");
                            Console.ForegroundColor = ConsoleColor.White;
                            lossTradesRsi.Add(child.rsi);
                            ++losses;
                            totalProfit += PnL;
                        }

                        openPositions.RemoveAt(0);
                    }
                }
            }
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
            return PnL;
        }

        private static decimal? backTestStrat5(List<IBinanceKline> lineData)
        {
            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(120));

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 120; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                if (lineSim.Count == 693)
                {
                    int bb = 0;
                }

                testOpenPositions(openPositions, lineSim);

                //adjust stoploss:

                if (openPositions.Count > 0)
                {
                    var op = openPositions.Last();
                    if ((op.type == "L" && op.stopLimitPrice < op.price) || (op.type == "S" && op.stopLimitPrice > op.price))
                    {
                        if (lineSim.Last().Low < op.targetRatioOne && lineSim.Last().High > op.targetRatioOne)
                        {
                            op.stopLimitPrice = op.price;
                            Console.WriteLine("ADJUSTED STOP LOSS");
                        }                        
                    }
                }


                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                var macd = quotes.GetMacd(3, 21, 3).ToList();
                var ema100 = quotes.GetEma(100).ToList();
                var ema200 = quotes.GetEma(200).ToList();
                var donch = quotes.GetDonchian(20).ToList();
                var zigZags = ZigZag.CalculateZZ(quotes.ToList(), Depth: 3, Deviation: 3, BackStep: 2);

                //INTRI CAND INCEPE UN OPEN PE UN CANDLE NOU SI VERIFICI TOTI PARAMETRII DE LA CLOSE-UL CANDLE-ULUI DE-ABIA TERMINAT
                // LONG check
                // See if EMA50 is on a bigger uptrend than EMA200
                // check if difference is at least 0.003%
                if (lineSim[lineSim.Count - 2].Close > (decimal)ema100[ema100.Count - 2].Ema && ema100[ema100.Count - 2].Ema > ema200[ema200.Count - 2].Ema && macd[macd.Count - 2].Macd > 0 && macdCrossesAbove(macd))
                {
                    if (openPositions.Count == 0)
                    {
                        Console.Write("LONG SIGNAL! - ");
                        Console.WriteLine(lineSim.Last().OpenTime);
                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = 3,
                            stopLimitPrice = donch[donch.Count - 2].LowerBand,//zigZags.Last(it => it.PointType == "L").ZigZag,
                            type = "L"
                        });
                        var op = openPositions.Last();
                        op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                        op.targetRatioOne = op.price + (op.price - (decimal)op.stopLimitPrice) * 1;
                    }
                }
                else
                {
                    //Console.WriteLine("Not in order for long");
                }


                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                if (lineSim[lineSim.Count - 2].Close < (decimal)ema100[ema100.Count - 2].Ema && ema100[ema100.Count - 2].Ema < ema200[ema200.Count - 2].Ema && macd[macd.Count - 2].Macd < 0 && macdCrossesBelow(macd))
                {
                    if (openPositions.Count == 0)
                    {
                        Console.Write("SHORT SIGNAL! - ");
                        Console.WriteLine(lineSim.Last().OpenTime);
                        openPositions.Add(new openFuture
                        {
                            startTime = lineSim.Last().OpenTime,
                            price = lineSim[lineSim.Count - 1].Open,
                            riskRatio = 3,
                            stopLimitPrice = donch[donch.Count - 2].UpperBand, //zigZags.Last(it => it.PointType == "H").ZigZag,
                            type = "S"
                        });
                        var op = openPositions.Last();
                        op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                        op.targetRatioOne = op.price + (op.price - (decimal)op.stopLimitPrice) * 1;
                    }
                }
                else
                {
                    //Console.WriteLine("Not in order for long");
                }
            }
            return null;
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

        private static decimal? backTestStrat4(List<IBinanceKline> lineData)
        {
            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(450));

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 450; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                testOpenPositions(openPositions, lineSim);

                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                var ema200 = quotes.GetEma(200).ToList();
                var stochRsi = quotes.GetStochRsi(14, 14, 3, 3).ToList();
                var st1 = quotes.GetSuperTrend(12, 3).ToList();
                var st2 = quotes.GetSuperTrend(11, 2).ToList();
                var st3 = quotes.GetSuperTrend(10, 1).ToList();

                var ema200H = ema200[ema200.Count - 2].Ema;

                //INTRI CAND INCEPE UN OPEN PE UN CANDLE NOU SI VERIFICI TOTI PARAMETRII DE LA CLOSE-UL CANDLE-ULUI DE-ABIA TERMINAT
                // LONG check
                // See if EMA50 is on a bigger uptrend than EMA200
                // check if difference is at least 0.003%
                if (false)//ema200H < lineSim[lineSim.Count - 2].Low) // && macdhH < 0
                {
                    if (stochRsiBelowCross(stochRsi))
                    {
                        if (numberOfBelowSt(st1, st2, st3, lineSim) >= 2)
                        {
                            if (openPositions.Count == 0)
                            {
                                Console.WriteLine($"LONG SIGNAL!");
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 2].Close,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = st3[st3.Count - 2].SuperTrend,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for long");
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                if (ema200H > (double)lineSim[lineSim.Count - 2].Low) // && macdhH < 0
                {
                    if (stochRsiAboveCross(stochRsi))
                    {
                        if (numberOfAboveSt(st1, st2, st3, lineSim) >= 2)
                        {
                            if (openPositions.Count == 0)
                            {
                                Console.WriteLine($"SHORT SIGNAL!");
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 2].Close,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = st3[st3.Count - 2].SuperTrend,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for long");
                    }
                }
            }
            return null;
        }

        private static int numberOfAboveSt(List<SuperTrendResult> st1, List<SuperTrendResult> st2, List<SuperTrendResult> st3, List<IBinanceKline> lineSim)
        {
            int nr = 0;
            if (st1[st1.Count - 2].SuperTrend > lineSim[lineSim.Count - 2].High) ++nr;
            if (st2[st2.Count - 2].SuperTrend > lineSim[lineSim.Count - 2].High) ++nr;
            if (st3[st3.Count - 2].SuperTrend > lineSim[lineSim.Count - 2].High) ++nr;

            return nr;
        }

        private static int numberOfBelowSt(List<SuperTrendResult> st1, List<SuperTrendResult> st2, List<SuperTrendResult> st3, List<IBinanceKline> lineSim)
        {
            int nr = 0;
            if (st1[st1.Count - 2].SuperTrend < lineSim[lineSim.Count - 2].High) ++nr;
            if (st2[st2.Count - 2].SuperTrend < lineSim[lineSim.Count - 2].High) ++nr;
            if (st3[st3.Count - 2].SuperTrend < lineSim[lineSim.Count - 2].High) ++nr;

            return nr;
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
            var client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("nn52gCw4GxraLGGyORnzTJUY50IILsoGLVACF3eb4PPfE01Jm27opvOrYZhcqUvU", "DrwClwpiKEPacecmr0zqdRzoqF85YwX2c2TvFdhnmEr5oecttXCi0AJBFqWiLY6Z"),
                BaseAddress = "https://fapi.binance.com",
            });

            var startTime = dateTime;
            List<IBinanceKline> ret = new List<IBinanceKline>();

            while (true)
            {
                var aux = (await client.FuturesUsdt.Market.GetKlinesAsync(pair, inter, startTime: startTime, limit: 1500)).Data;
                if (aux.Count() == 0) break;
                ret.AddRange(aux);
                startTime = ret.Last().CloseTime.AddSeconds(0);
            }

            var aux2 = (await client.FuturesUsdt.Market.GetAggregatedTradeHistoryAsync(pair, startTime: Convert.ToDateTime("19 Sep 2022 04:00"), endTime: Convert.ToDateTime("20 Sep 2022 22:00"), limit: 1000)).Data;

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

        private static decimal? backTestStrat3(List<IBinanceKline> lineData)
        {
            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(550));

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 550; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);
                //Console.WriteLine($"Added {i}");
                lineSim.RemoveAt(0);

                var sk = openPositions.Count == 1;
                int skip = testOpenPositions(openPositions, lineSim);
                if (sk && openPositions.Count == 0) 
                    continue;

                for (int j = 0; j < skip && i < lineData.Count - 1; j++)
                {
                    lineSim.Add(lineData[++i]);
                    lineSim.RemoveAt(0);
                }

                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                var ema50 = quotes.GetEma(50).ToList();
                var ema200 = quotes.GetEma(200).ToList();
                //var macd = quotes.GetMacd(fastPeriods: 12, slowPeriods: 26, signalPeriods: 9).ToList();
                var rsi = quotes.GetRsi(14).ToList();
                var atr = quotes.GetAtr().ToList();

                var ema50H = ema50[ema50.Count - 2].Ema;
                var ema200H = ema200[ema200.Count - 2].Ema;
                //var macdhH = macd[macd.Count - 2].Histogram;
                var rsiH = rsi[rsi.Count - 2].Rsi;

                //INTRI CAND INCEPE UN OPEN PE UN CANDLE NOU SI VERIFICI TOTI PARAMETRII DE LA CLOSE-UL CANDLE-ULUI DE-ABIA TERMINAT
                // LONG check
                // See if EMA50 is on a bigger uptrend than EMA200
                // check if difference is at least 0.003%
                if (rsiH >= 50 && ema50H > ema200H) // && macdhH < 0
                {
                    if (true) //macdNegAvg(macd, 50) > (double)macdhH && macdhH < -10
                    {
                        Point p1 = new Point() { X = (ema50.Count - 2) * 100, Y = (double)ema50[ema50.Count - 2].Ema };
                        Point p2 = new Point() { X = (ema50.Count - 1) * 100, Y = (double)ema50[ema50.Count - 1].Ema };

                        Point p3 = new Point() { X = (ema200.Count - 2) * 100, Y = (double)ema200[ema200.Count - 2].Ema };
                        Point p4 = new Point() { X = (ema200.Count - 1) * 100, Y = (double)ema200[ema200.Count - 1].Ema };

                        var csp1p2 = CalculateSlope(p1, p2);
                        var csp3p4 = CalculateSlope(p3, p4);

                        if (csp1p2 > csp3p4 && csp1p2 > 0 && csp1p2 > 0.0) //check angle of the EMAs - csp1p2 / csp3p4 > 3.2
                        {
                            if (openPositions.Count == 0)
                            {
                                Console.WriteLine($"LONG SIGNAL!");
                                Console.WriteLine("Test buying long ...");
                                /*openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 2].Close,
                                    riskRatio = 0,
                                    stopLimitPrice = lineSim[lineSim.Count - 2].Close * (decimal)0.996,
                                    targetPrice = lineSim[lineSim.Count - 2].Close * (decimal)1.005
                                });*/
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 1].Open,
                                    riskRatio = (decimal)1.2,
                                    stopLimitPrice = lineSim[lineSim.Count - 1].Open - (decimal)atr[atr.Count - 1].Atr * (decimal)1.85,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for long");
                    }
                }

                // SHORT check
                // See if EMA50 is on a bigger downtrend than EMA200
                // check if difference is at least 0.003%
                if (rsiH <= 50 && ema200H > ema50H) // && macdhH > 0
                {
                    if (true) //macdPosAvg(macd, 50) < (double)macdhH && macdhH > 10
                    {
                        Point p1 = new Point() { X = (ema50.Count - 2) * 100, Y = (double)ema50[ema50.Count - 2].Ema };
                        Point p2 = new Point() { X = (ema50.Count - 1) * 100, Y = (double)ema50[ema50.Count - 1].Ema };

                        Point p3 = new Point() { X = (ema200.Count - 2) * 100, Y = (double)ema200[ema200.Count - 2].Ema };
                        Point p4 = new Point() { X = (ema200.Count - 1) * 100, Y = (double)ema200[ema200.Count - 1].Ema };

                        var csp1p2 = CalculateSlope(p1, p2);
                        var csp3p4 = CalculateSlope(p3, p4);

                        if (csp1p2 < csp3p4 && csp1p2 < 0 && csp1p2 < 0.0) //check angle of the EMAs && csp3p4 / csp1p2 > 3.2
                        {
                            if (openPositions.Count == 0)
                            {
                                Console.WriteLine($"SHORT SIGNAL!");
                                Console.WriteLine("Test buying short ...");
                                /*openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 2].Close,
                                    riskRatio = 0,
                                    stopLimitPrice = lineSim[lineSim.Count - 2].Close * (decimal)1.004,
                                    targetPrice = lineSim[lineSim.Count - 2].Close * (decimal)0.995
                                });*/
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 1].Open,
                                    riskRatio = (decimal)1.2,
                                    stopLimitPrice = lineSim[lineSim.Count - 1].Open + (decimal)atr[atr.Count - 1].Atr * (decimal)1.85,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for long");
                    }
                }
            }
            return null;
        }

        private static decimal? backTestStrat1(List<Binance.Net.Interfaces.IBinanceKline> lineData)
        {
            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(200));

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 200; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                testOpenPositions(openPositions, lineSim);

                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                var ema20 = quotes.GetEma(20).ToList();
                var ema50 = quotes.GetEma(50).ToList();
                var ema100 = quotes.GetEma(100).ToList();
                var adx = quotes.GetAdx(14).ToList();
                var willf = quotes.GetFractal(2).ToList();

                //Console.WriteLine($"Bull: Last 3 Willf: {willf[willf.Count - 1].FractalBull}, {willf[willf.Count - 2].FractalBull}, {willf[willf.Count - 3].FractalBull}");
                //Console.WriteLine($"Bear: Last 3 Willf: {willf[willf.Count - 1].FractalBear}, {willf[willf.Count - 2].FractalBear}, {willf[willf.Count - 3].FractalBear}");

                // LONG check
                // See if 2 bars ago we have Fractal
                if (willf[willf.Count - 4].FractalBull != null)
                {
                    //see if ema20 > ema50 > ema 100
                    if (ema20[ema20.Count - 5].Ema > ema50[ema50.Count - 5].Ema && ema50[ema50.Count - 5].Ema > ema100[ema100.Count - 5].Ema)
                    {
                        if (false)//ema20[ema20.Count - 5].Ema > willf[willf.Count - 4].FractalBull && willf[willf.Count - 4].FractalBull > ema100[ema100.Count - 5].Ema && adx.Last().Adx > 25)
                        {
                            Console.WriteLine($"LONG SIGNAL!  WFractal Bull: {willf[willf.Count - 4].FractalBull} | ADX value: {adx.Last().Adx}");
                            Console.WriteLine("Test buying long ...");
                            if (openPositions.Count == 0)
                            {
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim.Last().Open,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = 0//(ema20[ema20.Count - 5].Ema > willf[willf.Count - 4].FractalBull) && (willf[willf.Count - 4].FractalBull > ema50[ema50.Count - 5].Ema) ? ema50[ema50.Count - 1].Ema : ema100[ema100.Count - 1].Ema,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                                //op.
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for long");
                    }
                }

                if (willf[willf.Count - 4].FractalBear != null)
                {
                    //see if ema20 > ema50 > ema 100
                    if (ema20[ema20.Count - 5].Ema < ema50[ema50.Count - 5].Ema && ema50[ema50.Count - 5].Ema < ema100[ema100.Count - 5].Ema)
                    {
                        if (false)//ema20[ema20.Count - 5].Ema < willf[willf.Count - 4].FractalBear && willf[willf.Count - 4].FractalBear < ema100[ema100.Count - 5].Ema && adx.Last().Adx > 25)
                        {
                            Console.WriteLine($"SHORT SIGNAL!  WFractal Bear: {willf[willf.Count - 4].FractalBear} | ADX value: {adx.Last().Adx}");
                            Console.WriteLine("Test buying short ...");
                            if (openPositions.Count == 0)
                            {
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim.Last().Open,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = 0//(ema20[ema20.Count - 5].Ema < willf[willf.Count - 4].FractalBull) && (willf[willf.Count - 4].FractalBull < ema50[ema50.Count - 5].Ema) ? ema50[ema50.Count - 1].Ema : ema100[ema100.Count - 1].Ema,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema100 & ema20 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for short");
                    }
                }
            }

            return null;
        }

        private static decimal? backTestStrat2(List<Binance.Net.Interfaces.IBinanceKline> lineData)
        {
            List<Binance.Net.Interfaces.IBinanceKline> lineSim = new List<Binance.Net.Interfaces.IBinanceKline>();
            lineSim.AddRange(lineData.Take(450));

            List<openFuture> openPositions = new List<openFuture>();

            for (int i = 450; i < lineData.Count; i++)
            {
                lineSim.Add(lineData[i]);

                testOpenPositions(openPositions, lineSim);

                IEnumerable<Quote> quotes = GetQuoteFromKlines(lineSim);

                var smma21 = quotes.GetSmma(21).ToList();
                var smma50 = quotes.GetSmma(50).ToList();
                var smma200 = quotes.GetSmma(200).ToList();
                var willf = quotes.GetFractal(2).ToList();
                var rsi = quotes.GetRsi(14).ToList();

                var smma21H = smma21[smma21.Count - 5].Smma;
                var smma50H = smma50[smma50.Count - 5].Smma;
                var smma200H = smma200[smma200.Count - 5].Smma;
                var willfBullH = willf[willf.Count - 4].FractalBull;
                var willfBearH = willf[willf.Count - 4].FractalBear;
                var rsiH = rsi[rsi.Count - 5].Rsi;

                //Console.WriteLine($"Bull: Last 3 Willf: {willf[willf.Count - 1].FractalBull}, {willf[willf.Count - 2].FractalBull}, {willf[willf.Count - 3].FractalBull}");
                //Console.WriteLine($"Bear: Last 3 Willf: {willf[willf.Count - 1].FractalBear}, {willf[willf.Count - 2].FractalBear}, {willf[willf.Count - 3].FractalBear}");

                // LONG check
                // See if 2 bars ago we have Fractal
                if (willfBullH != null)
                {
                    //see if ema20 > ema50 > ema 100
                    if (smma21H > smma50H && smma50H > smma200H)
                    {
                        if (false)//willfBullH > smma21H && rsiH > 50)
                        {
                            Console.WriteLine($"LONG SIGNAL!  WFractal Bull: {willfBullH} | RSI value: {rsiH}");
                            Console.WriteLine("Test buying long ...");
                            if (openPositions.Count == 0)
                            {
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 3].Open,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = willfBullH,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema20 & ema100 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for long");
                    }
                }

                if (willfBearH != null)
                {
                    //see if ema20 > ema50 > ema 100
                    if (smma21H < smma50H && smma50H < smma200H)
                    {
                        if (false)//willfBearH < smma21H && rsiH < 50)
                        {
                            Console.WriteLine($"SHORT SIGNAL!  WFractal Bear: {willfBearH} | RSI value: {rsiH}");
                            Console.WriteLine("Test buying short ...");
                            if (openPositions.Count == 0)
                            {
                                openPositions.Add(new openFuture
                                {
                                    startTime = lineSim.Last().OpenTime,
                                    price = lineSim[lineSim.Count - 3].Open,
                                    riskRatio = (decimal)1.5,
                                    stopLimitPrice = willfBearH,
                                });
                                var op = openPositions.Last();
                                op.targetPrice = op.price + (op.price - (decimal)op.stopLimitPrice) * op.riskRatio;
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Fractal not between ema100 & ema20 | ADX Value: {adx.Last().Adx}");
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not in order for short");
                    }
                }
            }

            return null;
        }

        public static int losses = 0;
        public static int wins = 0;
        public static decimal? totalProfit = 0;
        public static decimal? fee = 0.03M;

        private static int testOpenPositions(List<openFuture> openPositions, List<Binance.Net.Interfaces.IBinanceKline> lineSim)
        {
            List<openFuture> toRemove = new List<openFuture>();
            int skip = 0;

            foreach (var child in openPositions)
            {
                var startIndex = lineSim.FindIndex(it => it.OpenTime > child.startTime);
                for (int i = startIndex; i < lineSim.Count; i++)
                {
                    decimal? PnL = 0;
                    if (i < 0 || (child.stopLimitPrice >= lineSim[i].Low && child.stopLimitPrice <= lineSim[i].High))
                    {
                        PnL = calculatePnL(child.price, child.stopLimitPrice, investment, 1, child.targetPrice > child.price ? 1 : -1);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Loss for position! Started: {child.startTime} | Lost: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Target: {Math.Round(child.targetPrice, 2)} | StopLoss: {Math.Round((double)child.stopLimitPrice, 2)} | TOTAL PROFIT: {totalProfit}");
                        Console.ForegroundColor = ConsoleColor.White;
                        ++losses;
                        totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                        toRemove.Add(child);
                        break;
                    }
                    if (child.targetPrice >= lineSim[i].Low && child.targetPrice <= lineSim[i].High)
                    {
                        PnL = calculatePnL(child.price, child.targetPrice, investment, 1, child.targetPrice > child.price ? 1 : -1);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"Win for position! Started: {child.startTime} | Won: {Math.Round((double)PnL, 2)} | Entry: {Math.Round(child.price, 2)} | Target: {Math.Round(child.targetPrice, 2)} | StopLoss: {Math.Round((double)child.stopLimitPrice, 2)} | TOTAL PROFIT: {totalProfit}");
                        Console.ForegroundColor = ConsoleColor.White;
                        ++wins;
                        totalProfit += (PnL - (investment * child.price * fee * 2 / 100));
                        toRemove.Add(child);
                        break;
                    }
                }
            }

            foreach (var child in toRemove)
            {
                openPositions.Remove(child);
            }

            return skip;
        }

        private static IEnumerable<Quote> GetQuoteFromKlines(List<Binance.Net.Interfaces.IBinanceKline> lineData)
        {
            var qq = lineData.Select(it => new Quote()
            {
                Close = it.Close,
                High = it.High,
                Date = it.CloseTime,
                Low = it.Low,
                Open = it.Open,
                Volume = it.BaseVolume
            });
            return qq;
        }

        private static IEnumerable<Quote> GetQuoteFromKlinesStartDay(List<Binance.Net.Interfaces.IBinanceKline> lineData, DateTime startDay)
        {
            var qq = lineData.Where(it => it.OpenTime >= startDay.AddMinutes(-1)).Select(it => new Quote()
            {
                Close = it.Close,
                High = it.High,
                Date = it.CloseTime,
                Low = it.Low,
                Open = it.Open,
                Volume = it.BaseVolume
            });
            return qq;
        }
    }
}
