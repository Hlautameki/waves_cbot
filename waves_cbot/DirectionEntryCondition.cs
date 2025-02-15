namespace cAlgo.Robots;

public class DirectionEntryCondition : IEntryCondition
{
    private readonly DirectionEnum _direction;

    public DirectionEntryCondition(DirectionEnum direction)
    {
        _direction = direction;
    }

    public bool CanBuy()
    {
        if (_direction is DirectionEnum.Both or DirectionEnum.Long)
        {
            return true;
        }

        return false;
    }

    public bool CanSell()
    {
        if (_direction is DirectionEnum.Both or DirectionEnum.Short)
        {
            return true;
        }

        return false;
    }
}
