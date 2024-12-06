using cAlgo.API;

namespace cAlgo.Robots;

public class WavesExitSignalGenerator : IExitSignalGenerator
{
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly bool _exitIfPriceCrossesSlowerBand;
    private readonly bool _bandsCrossoverExit;

    public WavesExitSignalGenerator(Bars bars, FourMovingAveragesWithCloud waves, bool exitIfPriceCrossesSlowerBand,
        bool bandsCrossoverExit)
    {
        _bars = bars;
        _waves = waves;
        _exitIfPriceCrossesSlowerBand = exitIfPriceCrossesSlowerBand;
        _bandsCrossoverExit = bandsCrossoverExit;
    }

    public bool CloseBuy()
    {
        if (_exitIfPriceCrossesSlowerBand)
            return _bars.LastBar.Close < _waves.SlowLowMA.LastValue;

        if (_bandsCrossoverExit)
            return _waves.FastHighMA.LastValue < _waves.SlowLowMA.LastValue;

        return false;
    }

    public bool CloseSell()
    {
        if (_exitIfPriceCrossesSlowerBand)
            return _bars.LastBar.Close > _waves.SlowHighMA.LastValue;

        if (_bandsCrossoverExit)
            return _waves.FastLowMA.LastValue > _waves.SlowHighMA.LastValue;

        return false;
    }
}
