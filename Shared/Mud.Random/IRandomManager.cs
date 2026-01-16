namespace Mud.Random;

public interface IRandomManager
{
    int Next(int maxExcluded);
    int Next(int minIncluded, int maxExcluded);
    bool Chance(int percentage);
    int Dice(int count, int value);
    int Range(int min, int max);
    long Range(long min, long max);
    int Fuzzy(int number);

    T? Random<T>()
        where T : struct, Enum;
    T? Random<T>(Func<T, bool> filterFunc)
        where T : struct, Enum;

    T? Random<T>(IEnumerable<T> values);
    T? RandomOccurancy<T>(IEnumerable<IOccurancy<T>> occurancies);
    T? RandomOccurancy<T>(IEnumerable<IOccurancy<T>> occurancies, IEnumerable<T> history);
    int SumOccurancies<T>(IEnumerable<IOccurancy<T>> occurancies);
    T? RandomOccurancy<T, U>(IEnumerable<T> occurancies)
         where T : IOccurancy<U>;
}
