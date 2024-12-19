using cAlgo.API;

namespace cAlgo.Robots;

public interface IStopLossCalculator
{
    double? GetStopLoss(TradeType tradeType);
    double? GetStopLossInPrice(Position position);
}
