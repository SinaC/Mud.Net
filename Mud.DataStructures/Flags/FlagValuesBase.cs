using Microsoft.Extensions.Logging;

namespace Mud.DataStructures.Flags;

public abstract class FlagValuesBase<T> : IFlagValues<T>
{
    protected ILogger<FlagValuesBase<T>> Logger { get; }

    protected abstract HashSet<T> HashSet { get; }

    public virtual bool this[T flag] => HashSet.Contains(flag);

    public virtual IEnumerable<T> AvailableValues => HashSet;

    protected FlagValuesBase(ILogger<FlagValuesBase<T>> logger)
    {
        Logger = logger;
    }

    public virtual void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<T> values)
    {
        throw new ArgumentException($"Flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
