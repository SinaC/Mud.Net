using System;
using System.Collections.Generic;

namespace Mud.Server.Common
{
    public interface IRandomManager
    {
        Random Randomizer { get; }
        bool Chance(int percentage);
        int Dice(int count, int value);
        T Random<T>(IEnumerable<IOccurancy<T>> occurancies);
        T Random<T>(IEnumerable<IOccurancy<T>> occurancies, IEnumerable<T> history);
        int SumOccurancies<T>(IEnumerable<IOccurancy<T>> occurancies);
        T Random<T, U>(IEnumerable<T> occurancies)
             where T : IOccurancy<U>;
    }
}
