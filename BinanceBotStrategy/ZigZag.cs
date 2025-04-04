#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: StockSharp.Algo.Indicators.Algo
File: ZigZag.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License
namespace BinanceBotStrategy
{
    using Skender.Stock.Indicators;
    using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;

	/// <summary>
	/// ZigZag.
	/// </summary>
	/// <remarks>
	/// ZigZag traces and combines extreme points of the chart, distanced for not less than specified percentage by the price scale.
	/// </remarks>
	[DisplayName("ZigZag")]

	public static class ZigZag
    {
        #region Private fields

        //private IndicatorDataSeries _highZigZags;
        //private IndicatorDataSeries _lowZigZags;

        #endregion

        //protected override void Initialize()
        //{
        //    _highZigZags = CreateDataSeries();
        //    _lowZigZags = CreateDataSeries();
        //    _point = Symbol.PointSize;
        //}

        private static bool isHZone(List<Quote> quotes, int pivot, int lookup = 2)
        {
            if (pivot - lookup < 0 || pivot + lookup > quotes.Count - 1) return true;
            for (int i = pivot - lookup; i < pivot + lookup + 1; i++)
            {
                if (i != pivot)
                {
                    if (quotes[i].High > quotes[pivot].High) return false;
                }
            }
            return true;
        }

        private static bool isLZone(List<Quote> quotes, int pivot, int lookup = 2)
        {
            if (pivot - lookup < 0 || pivot + lookup > quotes.Count - 1) return true;
            for (int i = pivot - lookup; i < pivot + lookup + 1; i++)
            {
                if (i != pivot)
                {
                    if (quotes[i].Low < quotes[pivot].Low) return false;
                }
            }
            return true;
        }

        public static List<ZigZagResult> CalculateZZ(List<Quote> quotes, int Depth = 10, int Deviation = 5, int BackStep = 2)
        {
            double _lastLow = 0;
            double _lastHigh = 0;
            double _low = 0;
            double _high = 0;
            int _lastHighIndex = 0;
            int _lastLowIndex = 0;
            int _type = 0;
            double _point = 0.1;
            double _currentLow = 0;
            double _currentHigh = 0;
            double[] Result = new double[quotes.Count];
            double[] _highZigZags = new double[quotes.Count];
            double[] _lowZigZags = new double[quotes.Count];

            for (int index = 0; index < quotes.Count; index++)
            {
                if (index < Depth)
                {
                    Result[index] = 0;
                    _highZigZags[index] = 0;
                    _lowZigZags[index] = 0;
                    continue;
                }

                _currentLow = (double)quotes.GetRange(index - Depth + 1, Depth).Min(it => it.Low);//Functions.Minimum(MarketSeries.Low, Depth);
                if (Math.Abs((double)(_currentLow - _lastLow)) < double.Epsilon)
                    _currentLow = 0;
                else
                {
                    _lastLow = _currentLow;

                    if (((double)quotes[index].Low - _currentLow) > (Deviation * _point))//(MarketSeries.Low[index] - _currentLow) > (Deviation * _point))
                        _currentLow = 0;
                    else
                    {
                        for (int i = 1; i <= BackStep; i++)
                        {
                            if (Math.Abs((double)_lowZigZags[index - i]) > double.Epsilon
                                && _lowZigZags[index - i] > _currentLow)
                                _lowZigZags[index - i] = 0;
                        }
                    }
                }
                if (Math.Abs((double)quotes[index].Low - _currentLow) < double.Epsilon)
                    _lowZigZags[index] = _currentLow;
                else
                    _lowZigZags[index] = 0;

                _currentHigh = (double)quotes.GetRange(index - Depth + 1, Depth).Max(it => it.High); //MarketSeries.High.Maximum(Depth);

                if (Math.Abs(_currentHigh - _lastHigh) < double.Epsilon)
                    _currentHigh = 0;
                else
                {
                    _lastHigh = _currentHigh;

                    if ((_currentHigh - (double)quotes[index].High) > (Deviation * _point))
                        _currentHigh = 0;
                    else
                    {
                        for (int i = 1; i <= BackStep; i++)
                        {
                            if (Math.Abs(_highZigZags[index - i]) > double.Epsilon && _highZigZags[index - i] < _currentHigh)
                                _highZigZags[index - i] = 0;
                        }
                    }
                }

                if (Math.Abs((double)quotes[index].High - _currentHigh) < double.Epsilon)
                    _highZigZags[index] = _currentHigh;
                else
                    _highZigZags[index] = 0;


                switch (_type)
                {
                    case 0:
                        if (Math.Abs(_low - 0) < double.Epsilon && Math.Abs(_high - 0) < double.Epsilon)
                        {
                            if (Math.Abs(_highZigZags[index]) > double.Epsilon)
                            {
                                _high = (double)quotes[index].High;
                                _lastHighIndex = index;
                                _type = -1;
                                Result[index] = _high;
                            }
                            if (Math.Abs(_lowZigZags[index]) > double.Epsilon)
                            {
                                _low = (double)quotes[index].Low;
                                _lastLowIndex = index;
                                _type = 1;
                                Result[index] = _low;
                            }
                        }
                        break;
                    case 1:
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon && _lowZigZags[index] < _low && Math.Abs(_highZigZags[index] - 0) < double.Epsilon)
                        {
                            Result[_lastLowIndex] = double.NaN;
                            _lastLowIndex = index;
                            _low = _lowZigZags[index];
                            Result[index] = _low;
                        }
                        if (Math.Abs(_highZigZags[index] - 0) > double.Epsilon && Math.Abs(_lowZigZags[index] - 0) < double.Epsilon)
                        {
                            _high = _highZigZags[index];
                            _lastHighIndex = index;
                            Result[index] = _high;
                            _type = -1;
                        }
                        break;
                    case -1:
                        if (Math.Abs(_highZigZags[index]) > double.Epsilon && _highZigZags[index] > _high && Math.Abs(_lowZigZags[index] - 0) < double.Epsilon)
                        {
                            Result[_lastHighIndex] = double.NaN;
                            _lastHighIndex = index;
                            _high = _highZigZags[index];
                            Result[index] = _high;
                        }
                        if (Math.Abs(_lowZigZags[index]) > double.Epsilon && Math.Abs(_highZigZags[index]) <= double.Epsilon)
                        {
                            _low = _lowZigZags[index];
                            _lastLowIndex = index;
                            Result[index] = _low;
                            _type = 1;
                        }
                        break;
                }
            }

            var gg = 0;

            List<ZigZagResult> zzr = new List<ZigZagResult>();
            for (int i = 0; i < quotes.Count; i++)
            {
                if (quotes[i].Date > Convert.ToDateTime("17 Aug 2022 09:00"))
                {
                    var tt = 0;
                }
                ZigZagResult x = new ZigZagResult(quotes[i].Date);
                if (Result[i] > double.Epsilon)
                {
                    if (_lowZigZags[i] > double.Epsilon)
                    {
                        x.PointType = "L";
                        x.ZigZag = (decimal)_lowZigZags[i];
                    }
                    if (_highZigZags[i] > double.Epsilon)
                    {
                        x.PointType = "H";
                        x.ZigZag = (decimal)_highZigZags[i];
                    }
                } else
                {
                    x.RetraceHigh = quotes[i].High;
                    x.RetraceLow = quotes[i].Low;

                    if (isHZone(quotes, i) && isLZone(quotes, i))
                    {
                        x.PointType = "HLZ";
                    }
                    else
                    {
                        if (isHZone(quotes, i))
                        {
                            x.PointType = "HZ";
                        } 
                        else
                        {
                            if (isLZone(quotes, i))
                            {
                                x.PointType = "LZ";
                            }
                        }
                    }
                }
                zzr.Add(x);
            }

            return zzr;
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
        class zzResult
        {
            public Quote buffer { get; set; }
            public decimal lowBuffer { get; set; }
            public decimal highBuffer { get; set; }
            public decimal zigZagBuffer { get; set; }
        }

        static void NewZZ(List<zzResult> zList, List<Quote> quotes, int Depth = 12, int Deviation = 5, int BackStep = 2)
        {
            zList.Add(new zzResult
            { buffer = quotes[0]
            , highBuffer = 0
            , lowBuffer = 0
            , zigZagBuffer = 0
            });

            int level = 3;
            int limit;
            decimal lastHigh = 0;
            decimal lastLow = 0;

            if (zList.Count - 1 == 0)
            {
                limit = zList.Count - Depth;
            } else
            {
                int i = 0, count = 0;
                while (count < level && i < zList.Count - Depth)
                {
                    var res = zList[i].zigZagBuffer;
                    if (res != 0)
                    {
                        count++;
                    }
                    i++;
                }
                limit = --i;
            }

            for (var shift = limit; shift >= 0; shift--)
            {
                //--- low
                var val = zList.Skip(shift).Take(Depth).Min(it => it.buffer.Low);
                if (val == lastLow)
                {
                    val = 0.0m;
                } else
                {
                    lastLow = val;
                    if (zList[shift].buffer.Low - val >= 0.0m * val / 100)
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
                if (zList[shift].lowBuffer == val)
                {
                    zList[shift].lowBuffer = val;
                }
                else
                {
                    zList[shift].lowBuffer = 0m;
                }

                //--- high
                val = zList.Skip(shift).Take(Depth).Min(it => it.buffer.High);
                if (val == lastHigh)
                {
                    val = 0.0m;
                }
                else
                {
                    lastHigh = val;
                    if (val - zList[shift].buffer.High > 0.0m * val / 100)
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
                if (zList[shift].highBuffer == val)
                {
                    zList[shift].highBuffer = val;
                }
                else
                {
                    zList[shift].lowBuffer = 0m;
                }
            }

            //--- final cutting
            lastHigh = -1;
            lastLow = -1;
            var lastHighPos = -1;
            var lastLowPos = -1;

            for (var shift = limit; shift >= 0; shift--)
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
                        } else
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
                if(curLow != 0)
                {
                    if (lastLow > 0)
                    {
                        if (lastLow > curLow)
                        {
                            zList[lastLowPos].lowBuffer = 0;
                        } else
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

            for (var shift = limit; shift >= 0; shift--)
            {
                if (shift >= zList.Count - Depth)
                {
                    zList[shift].zigZagBuffer = 0.0m;
                } else
                {
                    var res = zList[shift].highBuffer;
                    if (res != 0.0m)
                    {
                        zList[shift].zigZagBuffer = res;
                    } else
                    {
                        zList[shift].zigZagBuffer = zList[shift].lowBuffer;
                    }
                }
            }
        }
    }
}