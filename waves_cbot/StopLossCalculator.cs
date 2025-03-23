using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class StopLossCalculator : IStopLossCalculator
{
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly double _stopLossRelativeToSlowBand;
    private readonly Symbol _symbol;
    private readonly double _stopLossRelativeToFastBand;
    private readonly double _stopLossRelativeToFastBandTrigger;

    public StopLossCalculator(Bars bars, FourMovingAveragesWithCloud waves,
        double stopLossRelativeToSlowBand, Symbol symbol, double stopLossRelativeToFastBand,
        double stopLossRelativeToFastBandTrigger)
    {
        _bars = bars;
        _waves = waves;
        _stopLossRelativeToSlowBand = stopLossRelativeToSlowBand;
        _symbol = symbol;
        _stopLossRelativeToFastBand = stopLossRelativeToFastBand;
        _stopLossRelativeToFastBandTrigger = stopLossRelativeToFastBandTrigger;
    }

    public double? GetStopLossInPrice(Position position)
    {
        double stopLossInPriceRelativeToSlowBand = GetStopLossInPriceRelativeToSlowBand(position);
        double stopLossInPriceRelativeToFastBand = GetStopLossInPriceRelativeToFastBand(position);

        return GetStopLossPrice(stopLossInPriceRelativeToSlowBand, stopLossInPriceRelativeToFastBand,
            position.StopLoss.HasValue ? position.StopLoss.Value : 0, position.TradeType);
    }

    private double GetStopLossInPriceRelativeToSlowBand(Position position)
    {
        if (_stopLossRelativeToSlowBand > 0)
        {
            if (position.TradeType == TradeType.Buy)
            {
                return _waves.SlowLowMA.LastValue - _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
            else
            {
                return _waves.SlowHighMA.LastValue + _stopLossRelativeToSlowBand * _symbol.PipSize;
            }
        }

        return 0;
    }

    private double GetStopLossInPriceRelativeToFastBand(Position position)
    {
        if (_stopLossRelativeToFastBand > 0)
        {
            var priceMovementSinceEntry = GetPriceMovementSinceEntry(position.EntryPrice, position.EntryTime, position.TradeType);

            if (_stopLossRelativeToFastBandTrigger > 0 && priceMovementSinceEntry < _stopLossRelativeToFastBandTrigger)
                return 0;

            if (position.TradeType == TradeType.Buy)
            {
                return _waves.FastLowMA.LastValue - _stopLossRelativeToFastBand * _symbol.PipSize;
            }
            else
            {
                return _waves.FastHighMA.LastValue + _stopLossRelativeToFastBand * _symbol.PipSize;
            }
        }

        return 0;
    }

    private double GetStopLossPrice(double price1, double price2, double price3, TradeType tradeType)
    {
        // Exclude ignored prices (0 or less)
        var validPrices = new List<double>();
        if (price1 > 0) validPrices.Add(price1);
        if (price2 > 0) validPrices.Add(price2);
        if (price3 > 0) validPrices.Add(price3);

        // If all prices are ignored, return 0 (no update possible)
        if (validPrices.Count == 0)
            return 0;

        // Determine the price based on the trade type
        return tradeType == TradeType.Buy
            ? validPrices.Max() // For long positions, get the highest price
            : validPrices.Min(); // For short positions, get the lowest price
    }

    private double GetPriceMovementSinceEntry(double entryPrice, DateTime entryTime, TradeType tradeType)
    {
        // Find the index of the bar corresponding to the entry time
        int entryIndex = _bars.OpenTimes.GetIndexByTime(entryTime);

        if (entryIndex < 0)
            return 0;

        double priceMovement = 0;

        // Iterate through the bars from entry time to current
        for (int i = entryIndex; i < _bars.Count; i++)
        {
            double movement;

            if (tradeType == TradeType.Buy)
            {
                // For Buy, check how far the price has risen
                movement = (_bars.HighPrices[i] - entryPrice) / _symbol.PipSize;
            }
            else
            {
                // For Sell, check how far the price has fallen
                movement = (entryPrice - _bars.LowPrices[i]) / _symbol.PipSize;
            }

            if (movement > priceMovement)
            {
                priceMovement = movement;
            }
        }

        return priceMovement;
    }

}
