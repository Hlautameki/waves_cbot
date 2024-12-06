using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class WavesStopLossCalculator : IStopLossCalculator
{
    private readonly double _stopLossInPips;
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _stopLossRelativeToSlowBand;
    private readonly Symbol _symbol;

    public WavesStopLossCalculator(double stopLossInPips, Bars bars, FourMovingAveragesWithCloud waves,
        double stopLossRelativeToSlowBand, Symbol symbol)
    {
        _stopLossInPips = stopLossInPips;
        _bars = bars;
        _waves = waves;
        _stopLossRelativeToSlowBand = stopLossRelativeToSlowBand;
        _symbol = symbol;
    }

    public double? GetStopLoss(TradeType tradeType)
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

        return _stopLossInPips;
    }

    public double? GetStopLossInPrice(TradeType tradeType)
    {
        if (_stopLossRelativeToSlowBand > 0)
        {
            if (tradeType == TradeType.Buy)
            {
                return _waves.SlowLowMA.LastValue - _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
            else
            {
                return _waves.SlowHighMA.LastValue + _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
        }

        return _stopLossInPips;
    }
}
