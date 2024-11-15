using cAlgo.API;

namespace cAlgo.Robots;

public interface IStopLossCalculator
{
    double? GetStopLoss(TradeType tradeType);
}
