namespace Mud.DataStructures.Flags;

public interface IFlags<T>
{
    bool IsNone { get; }

    bool IsSet(T flag);

    bool HasAny(IFlags<T> flags);
    bool HasAny(params T[] flags);
    bool HasAny(IEnumerable<T> flags);

    bool HasAll(IFlags<T> flags);
    bool HasAll(params T[] flags);
    bool HasAll(IEnumerable<T> flags);

    void Set(IFlags<T> flags);
    void Set(T flags);
    void Set(params T[] flags);

    void Unset(IFlags<T> flags);
    void Unset(T flags);
    void Unset(params T[] flags);

    void Copy(IFlags<T> flags);

    int Count { get; }

    IEnumerable<T> Values { get; }

    string Serialize();
}
