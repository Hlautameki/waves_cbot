using System;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class PositionSizeCalculator
{
    private readonly IAccount _account;
    private readonly double _depositRiskPercentage;
    private readonly Symbol _symbol;
    private readonly double _quantity;
    private readonly PositionSizeType _positionSizeType;

    public PositionSizeCalculator(IAccount account, double depositRiskPercentage, Symbol symbol, double quantity, PositionSizeType positionSizeType)
    {
        _account = account;
        _depositRiskPercentage = depositRiskPercentage;
        _symbol = symbol;
        _quantity = quantity;
        _positionSizeType = positionSizeType;
    }

    public double CalculatePositionSize(double? stopLossPips)
    {
        if (_positionSizeType == PositionSizeType.Fixed)
        {
            return GetVolumeInUnits(_quantity);
        }
        else
        {
            if (stopLossPips is null or 0)
            {
                throw new ArgumentException("Stop Loss In Pips can not be null or zero if Position Size Type is Relative");
            }

            return GetRelativePositionSize((double)stopLossPips);
        }
    }

    private double GetRelativePositionSize(double stopLossInPips)
    {
        // Retrieve the value of the deposit (account equity)
        double accountEquity = _account.Equity;

        // Calculate risk (in currency) based on equity and risk percentage
        double riskAmount = accountEquity * (_depositRiskPercentage / 100);

        // Calculate the value of one pip for a standard lot
        double pipValuePerLot = _symbol.PipValue * 100000;

        // Calculate the position size (in lots) needed to risk the given amount with the specified stop loss
        double positionSize = (riskAmount / (stopLossInPips * pipValuePerLot));

        return _symbol.NormalizeVolumeInUnits(GetVolumeInUnits(positionSize));
    }

    private double GetVolumeInUnits(double quantity)
    {
        return _symbol.QuantityToVolumeInUnits(quantity);
    }
}
