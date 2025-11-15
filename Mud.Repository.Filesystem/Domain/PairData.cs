namespace Mud.Repository.Filesystem.Domain;

public class PairData<TKey, TValue>
{
    public TKey Key { get; set; }

    public TValue Value { get; set; }

    public PairData()
    {
    }

    public PairData(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}
