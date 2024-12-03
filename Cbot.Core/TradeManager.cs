using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class TradeManager
{
    private readonly IEntrySignalGenerator _entrySignalGenerator;
    private readonly Action<string, object[]> _printAction;
    private readonly PositionManager _positionManager;
    private readonly IExitSignalGenerator _exitSignalGenerator;

    public TradeManager(IEntrySignalGenerator entrySignalGenerator,
        Action<string, object[]> print,
        PositionManager positionManager,
        IExitSignalGenerator exitSignalGenerator)
    {
        _entrySignalGenerator = entrySignalGenerator;
        _printAction = print;
        _positionManager = positionManager;
        _exitSignalGenerator = exitSignalGenerator;
    }

    public void ManageTrade()
    {
        if (_exitSignalGenerator.CloseBuy())
        {
            _positionManager.CloseAll(TradeType.Buy);
        }

        if (_exitSignalGenerator.CloseSell())
        {
            _positionManager.CloseAll(TradeType.Sell);
        }

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
