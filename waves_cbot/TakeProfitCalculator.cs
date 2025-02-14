namespace cAlgo.Robots;

public class TakeProfitCalculator : ITakeProfitCalculator
{
    private readonly double _takeProfit;

    public TakeProfitCalculator(double takeProfit)
    {
        _takeProfit = takeProfit;
    }

    public double? CalculateTakeProfit()
    {
        return _takeProfit;
    }
}
