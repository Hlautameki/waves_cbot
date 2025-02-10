using cAlgo.API;

namespace cAlgo.Robots;

public class HigherTimeFrameEntryCondition : IEntryCondition
{
    private readonly Bars _bars;
    private readonly FourMovingAveragesWithCloud _waves;
    private readonly HigherTimeFrameConditionEnum _higherTimeFrameConditionEnum;

    public HigherTimeFrameEntryCondition(Bars bars,
        FourMovingAveragesWithCloud waves,
        HigherTimeFrameConditionEnum higherTimeFrameConditionEnum)
    {
        _bars = bars;
        _waves = waves;
        _higherTimeFrameConditionEnum = higherTimeFrameConditionEnum;
    }

    public bool CanBuy()
    {
        if (_higherTimeFrameConditionEnum == HigherTimeFrameConditionEnum.None)
        {
            return true;
        }
        else if (_higherTimeFrameConditionEnum == HigherTimeFrameConditionEnum.SameDirectionCross)
        {
            return FastBandMovesAboveSlowBand();
        }
        else if (_higherTimeFrameConditionEnum == HigherTimeFrameConditionEnum.NotOppositeDirectionCross)
        {
            return !FastBandMovesBelowSlowBand();
        }

        return true;
    }

    public bool CanSell()
    {
        if (_higherTimeFrameConditionEnum == HigherTimeFrameConditionEnum.None)
        {
            return true;
        }
        else if (_higherTimeFrameConditionEnum == HigherTimeFrameConditionEnum.SameDirectionCross)
        {
            return FastBandMovesBelowSlowBand();
        }
        else if (_higherTimeFrameConditionEnum == HigherTimeFrameConditionEnum.NotOppositeDirectionCross)
        {
            return !FastBandMovesAboveSlowBand();
        }

        return true;
    }

    private bool FastBandMovesAboveSlowBand()
    {
        return _waves.SlowLowMA.LastValue > _waves.UltimateSlowHighMA.LastValue;
    }

    private bool FastBandMovesBelowSlowBand()
    {
        return _waves.SlowHighMA.LastValue < _waves.UltimateSlowLowMA.LastValue;
    }
}
