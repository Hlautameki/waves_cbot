using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class RetestEntryCondition : IEntryCondition
{
    private readonly bool _retestRequired;
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly int _retestValidityLength;

    public RetestEntryCondition(bool retestRequired, Bars bars, FourMovingAveragesWithCloud waves, int retestValidityLength)
    {
        _retestRequired = retestRequired;
        _bars = bars;
        _waves = waves;
        _retestValidityLength = retestValidityLength;
    }

    public bool CanBuy()
    {
        if (!_retestRequired)
        {
            return true;
        }

        if (CheckIfContactTookPlace(TradeType.Buy))
        {
            return true;
        }

        return false;
    }

    public bool CanSell()
    {
        if (!_retestRequired)
        {
            return true;
        }

        if (CheckIfContactTookPlace(TradeType.Sell))
        {
            return true;
        }

        return false;
    }

    private bool CheckIfContactTookPlace(TradeType tradeType)
    {
        var lastCrossoverTime = LastCrossoverTimeHelper.GetLastCrossoverTime(_bars, _waves);

        if (lastCrossoverTime is null)
            return false;

        if (_retestValidityLength > 0)
        {
            int pastIndex = _bars.OpenTimes.GetIndexByTime(lastCrossoverTime.Value);

            if (pastIndex == -1)
            {
                return false;
            }

            // Get the latest bar index
            int currentIndex = _bars.Count - 1;

            // Calculate distance in bars
            int distanceInBars = currentIndex - pastIndex;

            Logger.Log($"pastIndex: {pastIndex}; currentIndex: {currentIndex}; distance: {distanceInBars}");

            if (distanceInBars > _retestValidityLength)
            {
                return true;
            }
        }

        var lastContactTime = GetTimeOfLastPriceWithFastBandContact(tradeType);

        Logger.Log($"MCE LastCrossOverTime: {lastCrossoverTime} LastContactTime: {lastContactTime}");

        if (lastCrossoverTime is not null && lastContactTime is not null && lastCrossoverTime < lastContactTime)
        {
            return true;
        }

        return false;
    }

    private DateTime? GetTimeOfLastPriceWithFastBandContact(TradeType tradeType)
    {
        for (int i = _bars.Count; i >= 0; i--)
        {
            if (tradeType == TradeType.Buy)
            {
                if (_bars.LowPrices[i] <= _waves.FastHighMA[i])
                {
                    return _bars.OpenTimes[i];
                }
            }
            else if (tradeType == TradeType.Sell)
            {
                if (_bars.HighPrices[i] >= _waves.FastLowMA[i])
                {
                    return _bars.OpenTimes[i];
                }
            }
        }

        return null; // Return null if no contact is found
    }
}
