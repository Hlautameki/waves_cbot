using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    public class PositionSizeCalculator
    {
        private readonly IAccount _account;
        private readonly History _history;
        private readonly double _depositRiskPercentage;
        private readonly Symbol _symbol;
        private readonly double _quantity;
        private readonly PositionSizeType _positionSizeType;
        private readonly int _pyramidSize;
        private readonly string _label;

        public PositionSizeCalculator(IAccount account, History history, double depositRiskPercentage, Symbol symbol, double quantity, PositionSizeType positionSizeType, int pyramidSize, string label)
        {
            _account = account;
            _history = history;
            _depositRiskPercentage = depositRiskPercentage;
            _symbol = symbol;
            _quantity = quantity;
            _positionSizeType = positionSizeType;
            _pyramidSize = pyramidSize;
            _label = label;
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
                    throw new ArgumentException("Stop Loss in Pips cannot be null or zero if Position Size Type is Relative");
                }

                return GetRelativePositionSize((double)stopLossPips);
            }
        }

        private double GetRelativePositionSize(double stopLossInPips)
        {
            // Retrieve the account equity
            double accountEquity = _account.Equity;

            // Base risk amount (1% of equity, for example)
            double baseRiskAmount = accountEquity * (_depositRiskPercentage / 100);

            baseRiskAmount = GetPyramidAdjustedPositionSize();

            double GetPyramidAdjustedPositionSize()
            {
                return baseRiskAmount + GetConsecutiveProfitableTradesValue();
            }

            // Calculate the value of one pip for a standard lot
            double pipValuePerLot = _symbol.PipValue * _symbol.LotSize;

            // Calculate position size based on adjusted risk
            double positionSize = (baseRiskAmount / (stopLossInPips * pipValuePerLot));

            return _symbol.NormalizeVolumeInUnits(GetVolumeInUnits(positionSize));
        }

        private double GetConsecutiveProfitableTradesValue()
        {
            // Get last `_pyramidSize` closed trades for the current symbol
            var closedTrades = _history.FindAll(_label)
                .OrderByDescending(trade => trade.ClosingTime)
                .Take(_pyramidSize);

            double cumulateValue = 0;

            int consecutiveProfitableTrades = 1;

            foreach (var trade in closedTrades)
            {
                if (trade.NetProfit > 0 && consecutiveProfitableTrades < _pyramidSize)
                {
                    consecutiveProfitableTrades++;
                    cumulateValue += trade.NetProfit;
                }
                else
                {
                    break; // Stop counting if a loss is encountered
                }
            }

            return cumulateValue;
        }

        private double GetVolumeInUnits(double quantity)
        {
            return _symbol.QuantityToVolumeInUnits(quantity);
        }
    }
}
