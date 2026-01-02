using System.Collections;

namespace Mud.DataStructures;

// https://stackoverflow.com/questions/981776/using-an-enum-as-an-array-index-in-c-sharp
/// <summary>An array indexed by an Enum</summary>
/// <typeparam name="T">Type stored in array</typeparam>
/// <typeparam name="U">Indexer Enum type</typeparam>
public class ArrayByEnum<T, U> : IEnumerable<T>
    where U : Enum
{
    private readonly T[] _array;
    private readonly int _lower;

    public ArrayByEnum()
    {
        _lower = Convert.ToInt32(Enum.GetValues(typeof(U)).Cast<U>().Min());
        int upper = Convert.ToInt32(Enum.GetValues(typeof(U)).Cast<U>().Max());
        _array = new T[1 + upper - _lower];
    }

    public T this[U key]
    {
        get => _array[Convert.ToInt32(key) - _lower];
        set => _array[Convert.ToInt32(key) - _lower] = value;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Enum.GetValues(typeof(U)).Cast<U>().Select(i => this[i]).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}