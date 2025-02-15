using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class EntrySignalGenerator : IEntrySignalGenerator
{
    private readonly Bars _bars;
    private readonly Symbol _symbol;
    private readonly History _history;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _requiredBandsDistanceToEnter;
    private readonly double _priceToFastBandMaximalDistance;
    private readonly int _entryNumberPerCrossOver;
    private readonly IEntryCondition _higherTimeFrameEntryCondition;
    private readonly IEntryCondition _directionEntryCondition;
    private readonly IEntryCondition _retestEntryCondition;

    public EntrySignalGenerator(
        Bars bars,
        Symbol symbol,
        History history,
        FourMovingAveragesWithCloud waves,
        double requiredBandsDistanceToEnter,
        double priceToFastBandMaximalDistance,
        int entryNumberPerCrossOver,
        IEntryCondition higherTimeFrameEntryCondition,
        IEntryCondition directionEntryCondition,
        IEntryCondition retestEntryCondition)
    {
        _bars = bars;
        _symbol = symbol;
        _history = history;
        _waves = waves;
        _requiredBandsDistanceToEnter = requiredBandsDistanceToEnter;
        _priceToFastBandMaximalDistance = priceToFastBandMaximalDistance;
        _entryNumberPerCrossOver = entryNumberPerCrossOver;
        _higherTimeFrameEntryCondition = higherTimeFrameEntryCondition;
        _directionEntryCondition = directionEntryCondition;
        _retestEntryCondition = retestEntryCondition;
    }

    public bool CanBuy()
    {
        return FastBandMovesAboveSlowBand()
               && AreBandsFarEnoughToBuy()
               && IsPriceCloseEnoughToFastBand(TradeType.Buy)
               && IsThereARoomForAdditionalTradeInCrossOver(TradeType.Buy)
               && _higherTimeFrameEntryCondition.CanBuy()
               && _directionEntryCondition.CanBuy()
               && _retestEntryCondition.CanBuy();
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
        return FastBandMovesBelowSlowBand()
               && AreBandsFarEnoughToSell()
               && IsPriceCloseEnoughToFastBand(TradeType.Sell)
               && IsThereARoomForAdditionalTradeInCrossOver(TradeType.Sell)
               && _higherTimeFrameEntryCondition.CanSell()
               && _directionEntryCondition.CanSell()
               && _retestEntryCondition.CanSell();
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

    private bool IsThereARoomForAdditionalTradeInCrossOver(TradeType tradeType)
    {
        if (_entryNumberPerCrossOver == 0)
            return true;

        int buyOrders = 0;
        int sellOrders = 0;

        DateTime? lastCrossoverTime = LastCrossoverTimeHelper.GetLastCrossoverTime(_bars, _waves);

        foreach (var deal in _history)
        {
            if (lastCrossoverTime == null || deal.ClosingTime >= lastCrossoverTime)
            {
                if (deal.TradeType == TradeType.Buy)
                    buyOrders++;
                else if (deal.TradeType == TradeType.Sell)
                    sellOrders++;
            }
        }

        if (tradeType == TradeType.Buy)
        {
            return buyOrders < _entryNumberPerCrossOver;
        }
        else
        {
            return sellOrders < _entryNumberPerCrossOver;
        }
    }
}
