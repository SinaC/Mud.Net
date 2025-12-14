using Mud.DataStructures.FlagValues;

namespace Mud.DataStructures.Flags;

public interface IFlags<T>
{
    bool IsNone { get; }

    bool IsSet(T flag);

    bool HasAny(params T[] flags);
    bool HasAny(IEnumerable<string> flags);

    bool HasAll(params T[] flags);
    bool HasAll(IEnumerable<T> flags);

    void Set(T flags);
    void Set(IFlags<T> flags);
    void Set(params T[] flags);

    void Unset(T flags);
    void Unset(IFlags<T> flags);
    void Unset(params T[] flags);

    void Copy(IFlags<T> flags);

    int Count { get; }

    IEnumerable<T> Values { get; }

    string Serialize();
}

public interface IFlags<T, TFlagValues>
    where TFlagValues : IFlagValues<T>
{
    void Copy(IFlags<T, TFlagValues> flags);

    bool IsSet(T flag);
    bool IsNone { get; }
    bool HasAny(params T[] flags);
    bool HasAny(IFlags<T, TFlagValues> flags);
    bool HasAll(params T[] flags);
    bool HasAll(IFlags<T, TFlagValues> flags);

    void Set(T flag);
    void Set(params T[] flags);
    void Set(IFlags<T, TFlagValues> flags);

    void Unset(T flag);
    void Unset(params T[] flags);
    void Unset(IFlags<T, TFlagValues> flags);

    int Count { get; }

    IEnumerable<T> Values { get; }

    string Serialize();
}
