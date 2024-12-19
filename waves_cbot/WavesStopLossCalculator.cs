using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class WavesStopLossCalculator : IStopLossCalculator
{
    private readonly double _stopLossInPipsFixed;
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _stopLossRelativeToSlowBand;
    private readonly Symbol _symbol;

    public WavesStopLossCalculator(double stopLossInPipsFixed, Bars bars, FourMovingAveragesWithCloud waves,
        double stopLossRelativeToSlowBand, Symbol symbol)
    {
        _stopLossInPipsFixed = stopLossInPipsFixed;
        _bars = bars;
        _waves = waves;
        _stopLossRelativeToSlowBand = stopLossRelativeToSlowBand;
        _symbol = symbol;
    }

    public double? GetStopLoss(TradeType tradeType)
    {
        if (_stopLossRelativeToSlowBand > 0)
        {
            double stopLossRelativeInPips = 0;

            if (tradeType == TradeType.Buy)
            {
                stopLossRelativeInPips = (_bars.LastBar.Close - (_waves.SlowLowMA.LastValue - _stopLossRelativeToSlowBand * _symbol.PipSize))/_symbol.PipSize;
            }
            else
            {
                stopLossRelativeInPips = ((_waves.SlowHighMA.LastValue + _stopLossRelativeToSlowBand * _symbol.PipSize) - _bars.LastBar.Close)/_symbol.PipSize;
            }

            if (_stopLossInPipsFixed > 0 && _stopLossInPipsFixed < stopLossRelativeInPips)
            {
                return _stopLossInPipsFixed;
            }

            return stopLossRelativeInPips;
        }

        return _stopLossInPipsFixed;
    }

    public double? GetStopLossInPrice(Position position)
    {
        if (_stopLossRelativeToSlowBand > 0)
        {
            double stopLossRelativeInPrice;

            if (position.TradeType == TradeType.Buy)
            {
                stopLossRelativeInPrice = _waves.SlowLowMA.LastValue - _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
            else
            {
                stopLossRelativeInPrice = _waves.SlowHighMA.LastValue + _stopLossRelativeToSlowBand * _symbol.PipSize;
            }

            if (_stopLossInPipsFixed > 0)
            {
                double stopLossFixedInPrice = position.StopLoss.Value;

                if (position.TradeType == TradeType.Buy && stopLossFixedInPrice > stopLossRelativeInPrice)
                {
                    return stopLossFixedInPrice;
                }
                else if (position.TradeType == TradeType.Sell && stopLossFixedInPrice < stopLossRelativeInPrice)
                {
                    return stopLossFixedInPrice;
                }
            }

            return stopLossRelativeInPrice;
        }

        return _stopLossInPipsFixed * _symbol.PipSize;
    }
}
