using cAlgo.API;

namespace cAlgo.Robots;

public interface IStopLossCalculator
{
    double? GetStopLossInPrice(Position position);
}
