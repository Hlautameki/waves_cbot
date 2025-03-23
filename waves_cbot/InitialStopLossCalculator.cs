using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class InitialStopLossCalculator : IInitialStopLossCalculator
{
    private readonly double _stopLossInPipsFixed;
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _stopLossRelativeToSlowBand;
    private readonly Symbol _symbol;
    private readonly double _stopLossRelativeToFastBand;

    public InitialStopLossCalculator(
        double stopLossInPipsFixed,
        Bars bars,
        FourMovingAveragesWithCloud waves,
        double stopLossRelativeToSlowBand,
        Symbol symbol,
        double stopLossRelativeToFastBand)
    {
        _stopLossInPipsFixed = stopLossInPipsFixed;
        _bars = bars;
        _waves = waves;
        _stopLossRelativeToSlowBand = stopLossRelativeToSlowBand;
        _symbol = symbol;
        _stopLossRelativeToFastBand = stopLossRelativeToFastBand;
    }

    public double? GetInitialStopLossInPips(TradeType tradeType)
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

    static double GetLowestNonZeroValue(double a, double b, double c)
    {
        double[] values = { a, b, c };
        var nonZeroValues = values.Where(val => val != 0).ToArray();

        if (!nonZeroValues.Any())
            return 0;

        return nonZeroValues.Min();
    }
}
