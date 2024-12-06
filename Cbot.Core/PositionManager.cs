using System;
using System.Linq;
using cAlgo.API;

namespace cAlgo.Robots;

public class PositionManager
{
    private readonly Func<Position, TradeResult> _closePosition;
    private readonly Positions _positions;
    private readonly string _label;
    private readonly string _symbolName;
    private readonly Action<string, object[]> _printAction;
    private readonly Func<TradeType, string, double, string, double?, double?, TradeResult> _executeMarketOrder;
    private readonly IStopLossCalculator _stopLossCalculator;
    private readonly PositionSizeCalculator _positionSizeCalculator;


    public PositionManager(
        Func<Position, TradeResult> closePosition,
        Positions positions, string label,
        string symbolName,
        Action<string, object[]> print,
        Func<TradeType, string, double, string, double?, double?, TradeResult> executeMarketOrder,
        IStopLossCalculator stopLossCalculator,
        PositionSizeCalculator positionSizeCalculator)
    {
        _closePosition = closePosition;
        _positions = positions;
        _label = label;
        _symbolName = symbolName;
        _printAction = print;
        _executeMarketOrder = executeMarketOrder;
        _stopLossCalculator = stopLossCalculator;
        _positionSizeCalculator = positionSizeCalculator;
    }

    public void CloseAll(TradeType tradeType)
    {
        var openedPositions = _positions.FindAll(_label, _symbolName, tradeType);
        foreach (var position in openedPositions)
        {
            _closePosition(position);
        }
    }

    public void OpenPosition(TradeType tradeType)
    {
        var openedPositions = _positions.FindAll(_label, _symbolName, tradeType);
        if (!openedPositions.Any())
        {
            var stopLoss = _stopLossCalculator.GetStopLoss(tradeType);
            var positionSize = _positionSizeCalculator.CalculatePositionSize(stopLoss);
            _executeMarketOrder(tradeType, _symbolName, positionSize, _label, stopLoss, null);
        }
    }

    public void UpdateStopLoss()
    {
        if (_positions.Any())
        {
            var lastPosition = _positions.Last();

            var stopLoss = _stopLossCalculator.GetStopLossInPrice(lastPosition.TradeType);

            lastPosition.ModifyStopLossPrice(stopLoss);
        }
    }

    private void Print(string message)
    {
        _printAction(message, new object[] { });
    }
}
