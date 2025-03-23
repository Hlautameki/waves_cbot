using cAlgo.API;

namespace cAlgo.Robots;

public interface IInitialStopLossCalculator
{
    double? GetInitialStopLossInPips(TradeType tradeType);
}
