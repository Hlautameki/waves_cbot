using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class TradeManager
{
    private readonly IEntrySignalGenerator _entrySignalGenerator;
    private readonly Action<string, object[]> _printAction;
    private readonly PositionManager _positionManager;

    public TradeManager(IEntrySignalGenerator entrySignalGenerator, Action<string, object[]> print, PositionManager positionManager)
    {
        _entrySignalGenerator = entrySignalGenerator;
        _printAction = print;
        _positionManager = positionManager;
    }

    public void ManageTrade()
    {
        if (_entrySignalGenerator.CanBuy())
        {
            _positionManager.CloseAll(TradeType.Sell);

            _positionManager.OpenPosition(TradeType.Buy);
        }

        if (_entrySignalGenerator.CanSell())
        {
            _positionManager.CloseAll(TradeType.Buy);

            _positionManager.OpenPosition(TradeType.Sell);
        }
    }

    private void Print(string message)
    {
        _printAction(message, new object[] {});
    }
}
