using cAlgo.API;

namespace cAlgo.Robots;

public class WavesExitSignalGenerator : IExitSignalGenerator
{
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly bool _exitIfPriceCrossesSlowerBand;

    public WavesExitSignalGenerator(Bars bars, FourMovingAveragesWithCloud waves, bool exitIfPriceCrossesSlowerBand)
    {
        _bars = bars;
        _waves = waves;
        _exitIfPriceCrossesSlowerBand = exitIfPriceCrossesSlowerBand;
    }

    public bool CloseBuy()
    {
        if (_exitIfPriceCrossesSlowerBand)
            return _bars.LastBar.Close < _waves.SlowLowMA.LastValue;

        return false;
    }

    public bool CloseSell()
    {
        if (_exitIfPriceCrossesSlowerBand)
            return _bars.LastBar.Close > _waves.SlowHighMA.LastValue;

        return false;
    }
}
