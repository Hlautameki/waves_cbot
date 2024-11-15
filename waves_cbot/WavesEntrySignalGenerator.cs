using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots;

public class WavesEntrySignalGenerator : IEntrySignalGenerator
{
    private readonly Bars _bars;
    private readonly HullMovingAverage _hullMa;

    public WavesEntrySignalGenerator(Bars bars, HullMovingAverage hullMa)
    {
        _bars = bars;
        _hullMa = hullMa;
    }

    public bool CanBuy()
    {
        return _bars.LastBar.Close > _hullMa.Result.LastValue;
    }

    public bool CanSell()
    {
        return _bars.LastBar.Close < _hullMa.Result.LastValue;
    }
}
