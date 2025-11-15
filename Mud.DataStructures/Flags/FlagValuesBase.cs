namespace Mud.DataStructures.Flags;

public abstract class FlagValuesBase<T> : IFlagValues<T>
{
    protected abstract HashSet<T> HashSet { get; }

    public virtual bool this[T flag] => HashSet.Contains(flag);

    public virtual IEnumerable<T> AvailableValues => HashSet;

    public virtual string PrettyPrint(T flag, bool shortDisplay) => flag?.ToString() ?? string.Empty;

    public virtual void OnUnknownValues(UnknownFlagValueContext context, IEnumerable<T> values)
    {
        throw new ArgumentException($"Flags '{string.Join(",", values)}' not found in {GetType().FullName}");
    }
}
