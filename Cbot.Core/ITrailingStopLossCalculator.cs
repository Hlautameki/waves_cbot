using cAlgo.API;

namespace cAlgo.Robots;

public interface ITrailingStopLossCalculator
{
    double? GetStopLossInPrice(Position position);
}
