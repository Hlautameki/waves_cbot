using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots;

public class WavesStopLossCalculator : IStopLossCalculator
{
    private readonly double _stopLossInPips;
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;

    public WavesStopLossCalculator(double stopLossInPips, Bars bars, FourMovingAveragesWithCloud waves)
    {
        _stopLossInPips = stopLossInPips;
        _bars = bars;
        _waves = waves;
    }

    public double? GetStopLoss(TradeType tradeType)
    {
        return _stopLossInPips;
    }
}
