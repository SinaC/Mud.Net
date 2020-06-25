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
    }

    public interface IFlags<T, TFlagValues>
        where TFlagValues : IFlagValues<T>
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
    }
}
