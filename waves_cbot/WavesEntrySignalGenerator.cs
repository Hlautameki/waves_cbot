using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class WavesEntrySignalGenerator : IEntrySignalGenerator
{
    private readonly Bars _bars;
    private readonly Symbol _symbol;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _requiredBandsDistanceToEnter;
    private readonly double _priceToFastBandMaximalDistance;

    public WavesEntrySignalGenerator(Bars bars, Symbol symbol, FourMovingAveragesWithCloud waves,
        double requiredBandsDistanceToEnter, double priceToFastBandMaximalDistance)
    {
        _bars = bars;
        _symbol = symbol;
        _waves = waves;
        _requiredBandsDistanceToEnter = requiredBandsDistanceToEnter;
        _priceToFastBandMaximalDistance = priceToFastBandMaximalDistance;
    }

    public bool CanBuy()
    {
        return FastBandMovesAboveSlowBand() && AreBandsFarEnoughToBuy() && IsPriceCloseEnoughToFastBand(TradeType.Buy);
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
        return FastBandMovesBelowSlowBand() && AreBandsFarEnoughToSell() && IsPriceCloseEnoughToFastBand(TradeType.Sell);
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

    private bool IsPriceCloseEnoughToFastBand(TradeType tradeType)
    {
        if (_priceToFastBandMaximalDistance > 0)
        {
            if (tradeType == TradeType.Buy)
            {
                return _bars.LastBar.Close - _waves.FastHighMA.LastValue <=
                       _priceToFastBandMaximalDistance * _symbol.PipSize;
            }
            else if (tradeType == TradeType.Sell)
            {
                return _waves.FastLowMA.LastValue - _bars.LastBar.Close <=
                       _priceToFastBandMaximalDistance * _symbol.PipSize;
            }
        }

        return true;
    }
}
