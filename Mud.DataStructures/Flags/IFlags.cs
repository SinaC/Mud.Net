using System.Collections.Generic;

namespace Mud.DataStructures.Flags
{
    public interface IFlags<T>
    {
        bool IsSet(T flag);
        bool HasAny(params T[] flags);
        bool HasAll(params T[] flags);

        bool Set(T flag);
        void Set(params T[] flags);

        bool Unset(T flag);
        void Unset(params T[] flags);

        int Count { get; }

        IEnumerable<T> Items { get; }

        string Map();
    }

    public interface IFlags<T, TFlagValues>
        where TFlagValues : IFlagValues<T>
    {
        bool IsSet(T flag);
        bool HasAny(params T[] flags);
        bool HasAny(IFlags<T, TFlagValues> flags);
        bool HasAll(params T[] flags);
        bool HasAll(IFlags<T, TFlagValues> flags);

        bool Set(T flag);
        void Set(params T[] flags);
        void Set(IFlags<T, TFlagValues> flags);

        bool Unset(T flag);
        void Unset(params T[] flags);
        void Unset(IFlags<T, TFlagValues> flags);

        int Count { get; }

        IEnumerable<T> Items { get; }

        string Map();
    }
}
