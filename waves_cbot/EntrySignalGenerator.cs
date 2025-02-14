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

    public EntrySignalGenerator(Bars bars, Symbol symbol, History history, FourMovingAveragesWithCloud waves,
        double requiredBandsDistanceToEnter, double priceToFastBandMaximalDistance, int entryNumberPerCrossOver, IEntryCondition higherTimeFrameEntryCondition)
    {
        _bars = bars;
        _symbol = symbol;
        _history = history;
        _waves = waves;
        _requiredBandsDistanceToEnter = requiredBandsDistanceToEnter;
        _priceToFastBandMaximalDistance = priceToFastBandMaximalDistance;
        _entryNumberPerCrossOver = entryNumberPerCrossOver;
        _higherTimeFrameEntryCondition = higherTimeFrameEntryCondition;
    }

    public bool CanBuy()
    {
        return FastBandMovesAboveSlowBand()
               && AreBandsFarEnoughToBuy()
               && IsPriceCloseEnoughToFastBand(TradeType.Buy)
               && IsThereARoomForAdditionalTradeInCrossOver(TradeType.Buy)
               && _higherTimeFrameEntryCondition.CanBuy();
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
               && _higherTimeFrameEntryCondition.CanSell();
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

        DateTime? lastCrossoverTime = GetLastCrossoverTime();

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

    private DateTime? GetLastCrossoverTime()
    {
        for (int i = _bars.Count - 2; i >= 0; i--) // Start from the second-to-last bar
        {
            double fastPrev = _waves.FastLowMA[i + 1];// _fastMA.Result[i + 1];
            double slowPrev = _waves.SlowHighMA[i + 1];// _slowMA.Result[i + 1];
            double fastCurrent = _waves.FastLowMA[i];// _fastMA.Result[i];
            double slowCurrent = _waves.SlowHighMA[i];// _slowMA.Result[i];

            // Check for crossover
            if (CrossLong() || CrossShort())
            {
                return _bars.OpenTimes[i]; // Return the time of the crossover
            }

            bool CrossLong()
            {
                return _waves.FastLowMA[i + 1] <= _waves.SlowHighMA[i + 1] && _waves.FastLowMA[i] > _waves.SlowHighMA[i];
            }

            bool CrossShort()
            {
                return _waves.FastHighMA[i + 1] >= _waves.SlowLowMA[i + 1] && _waves.FastHighMA[i] < _waves.SlowLowMA[i];
            }
        }


        return null; // Return null if no crossover is found
    }
}
