using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class WavesEntrySignalGenerator : IEntrySignalGenerator
{
    private readonly Bars _bars;
    private readonly Symbol _symbol;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _requiredBandsDistanceToEnter;

    public WavesEntrySignalGenerator(Bars bars, Symbol symbol, FourMovingAveragesWithCloud waves,
        double requiredBandsDistanceToEnter)
    {
        _bars = bars;
        _symbol = symbol;
        _waves = waves;
        _requiredBandsDistanceToEnter = requiredBandsDistanceToEnter;
    }

    public bool CanBuy()
    {
        return FastBandMovesAboveSlowBand() && AreBandsFarEnoughToBuy();
    }

    private bool FastBandMovesAboveSlowBand()
    {
        return _bars.LastBar.Close > _waves.FastHighMA.LastValue
               && _waves.FastLowMA.LastValue > _waves.SlowHighMA.LastValue;
    }

    private bool AreBandsFarEnoughToBuy()
    {
        return _waves.FastLowMA.LastValue - _waves.SlowHighMA.LastValue > _requiredBandsDistanceToEnter * _symbol.PipSize;
    }

    public bool CanSell()
    {
        return FastBandMovesBelowSlowBand() && AreBandsFarEnoughToSell();
    }

    private bool FastBandMovesBelowSlowBand()
    {
        return _bars.LastBar.Close < _waves.FastLowMA.LastValue
               && _waves.FastHighMA.LastValue < _waves.SlowLowMA.LastValue;
    }

    private bool AreBandsFarEnoughToSell()
    {
        return _waves.SlowLowMA.LastValue - _waves.FastHighMA.LastValue > _requiredBandsDistanceToEnter * _symbol.PipSize;
    }
}
