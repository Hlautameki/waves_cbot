using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class WavesStopLossCalculator : IStopLossCalculator
{
    private readonly double _stopLossInPipsFixed;
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _stopLossRelativeToSlowBand;
    private readonly Symbol _symbol;
    private readonly double _stopLossRelativeToFastBand;

    public WavesStopLossCalculator(double stopLossInPipsFixed, Bars bars, FourMovingAveragesWithCloud waves,
        double stopLossRelativeToSlowBand, Symbol symbol, double stopLossRelativeToFastBand)
    {
        _stopLossInPipsFixed = stopLossInPipsFixed;
        _bars = bars;
        _waves = waves;
        _stopLossRelativeToSlowBand = stopLossRelativeToSlowBand;
        _symbol = symbol;
        _stopLossRelativeToFastBand = stopLossRelativeToFastBand;
    }

    public double? GetStopLoss(TradeType tradeType)
    {
        var stopLossInPipsRelativeToSlowBand = GetStopLossInPipsRelativeToSlowBand(tradeType);

        var stopLossInPipsRelativeToFastBand = GetStopLossInPipsRelativeToFastBand(tradeType);

        return GetLowestNonZeroValue(stopLossInPipsRelativeToSlowBand, stopLossInPipsRelativeToFastBand, _stopLossInPipsFixed);
    }

    private double GetStopLossInPipsRelativeToSlowBand(TradeType tradeType)
    {
        if (_stopLossRelativeToSlowBand > 0)
        {
            if (tradeType == TradeType.Buy)
            {
                return (_bars.LastBar.Close - (_waves.SlowLowMA.LastValue - _stopLossRelativeToSlowBand * _symbol.PipSize))/_symbol.PipSize;
            }
            else
            {
                return ((_waves.SlowHighMA.LastValue + _stopLossRelativeToSlowBand * _symbol.PipSize) - _bars.LastBar.Close)/_symbol.PipSize;
            }
        }

        return 0;
    }

    private double GetStopLossInPipsRelativeToFastBand(TradeType tradeType)
    {
        if (_stopLossRelativeToFastBand > 0)
        {
            if (tradeType == TradeType.Buy)
            {
                return (_bars.LastBar.Close -
                        (_waves.FastLowMA.LastValue - _stopLossRelativeToFastBand * _symbol.PipSize)) / _symbol.PipSize;
            }
            else
            {
                return ((_waves.FastHighMA.LastValue + _stopLossRelativeToFastBand * _symbol.PipSize) -
                        _bars.LastBar.Close) / _symbol.PipSize;
            }
        }

        return 0;
    }

    public double? GetStopLossInPrice(Position position)
    {
        double stopLossInPriceRelativeToSlowBand = GetStopLossInPriceRelativeToSlowBand(position);
        double stopLossInPriceRelativeToFastBand = GetStopLossInPriceRelativeToFastBand(position);

        return GetStopLossPrice(stopLossInPriceRelativeToSlowBand, stopLossInPriceRelativeToFastBand,
            position.StopLoss.HasValue ? position.StopLoss.Value : 0, position.TradeType);
    }

    private double GetStopLossInPriceRelativeToSlowBand(Position position)
    {
        if (_stopLossRelativeToSlowBand > 0)
        {
            if (position.TradeType == TradeType.Buy)
            {
                return _waves.SlowLowMA.LastValue - _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
            else
            {
                return _waves.SlowHighMA.LastValue + _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
        }

        return 0;
    }

    private double GetStopLossInPriceRelativeToFastBand(Position position)
    {
        if (_stopLossRelativeToFastBand > 0)
        {
            if (position.TradeType == TradeType.Buy)
            {
                return _waves.FastLowMA.LastValue - _stopLossRelativeToFastBand * _symbol.PipSize;
            }
            else
            {
                return _waves.FastHighMA.LastValue + _stopLossRelativeToFastBand * _symbol.PipSize;
            }
        }

        return 0;
    }

    static double GetLowestNonZeroValue(double a, double b, double c)
    {
        double[] values = { a, b, c };
        var nonZeroValues = values.Where(val => val != 0).ToArray();

        if (!nonZeroValues.Any())
            return 0;

        return nonZeroValues.Min();
    }

    private double GetStopLossPrice(double price1, double price2, double price3, TradeType tradeType)
    {
        // Exclude ignored prices (0 or less)
        var validPrices = new List<double>();
        if (price1 > 0) validPrices.Add(price1);
        if (price2 > 0) validPrices.Add(price2);
        if (price3 > 0) validPrices.Add(price3);

        // If all prices are ignored, return 0 (no update possible)
        if (validPrices.Count == 0)
            return 0;

        // Determine the price based on the trade type
        return tradeType == TradeType.Buy
            ? validPrices.Max() // For long positions, get the highest price
            : validPrices.Min(); // For short positions, get the lowest price
    }
}
