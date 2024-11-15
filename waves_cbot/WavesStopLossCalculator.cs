using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots;

public class WavesStopLossCalculator : IStopLossCalculator
{
    private readonly double _stopLossInPips;
    private readonly Bars _bars;
    private readonly HullMovingAverage _hullMa;

    public WavesStopLossCalculator(double stopLossInPips, Bars bars, HullMovingAverage hullMa)
    {
        _stopLossInPips = stopLossInPips;
        _bars = bars;
        _hullMa = hullMa;
    }

    public double? GetStopLoss(TradeType tradeType)
    {
        return _stopLossInPips;
    }
}
