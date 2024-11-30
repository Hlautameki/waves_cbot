using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots;

public class WavesEntrySignalGenerator : IEntrySignalGenerator
{
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;

    public WavesEntrySignalGenerator(Bars bars, FourMovingAveragesWithCloud waves)
    {
        _bars = bars;
        _waves = waves;
    }

    public bool CanBuy()
    {
        return _bars.LastBar.Close > _waves.FastHighMA.LastValue && _bars.LastBar.Close > _waves.SlowHighMA.LastValue;
    }

    public bool CanSell()
    {
        return _bars.LastBar.Close < _waves.FastLowMA.LastValue && _bars.LastBar.Close < _waves.SlowLowMA.LastValue;
    }
}
