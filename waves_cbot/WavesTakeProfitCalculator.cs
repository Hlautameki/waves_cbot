namespace cAlgo.Robots;

public class WavesTakeProfitCalculator : ITakeProfitCalculator
{
    private readonly double _takeProfit;

    public WavesTakeProfitCalculator(double takeProfit)
    {
        _takeProfit = takeProfit;
    }

    public double? CalculateTakeProfit()
    {
        return _takeProfit;
    }
}
