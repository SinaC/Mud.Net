using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Container;
using Mud.Logger;
using Mud.Server.Common;

namespace Mud.Server.Blueprints.LootTable
{
    public class TreasureTable<T>
        where T:IEquatable<T>
    {
        private IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();

        public string Name { get; set; }
        public List<TreasureTableEntry<T>> Entries { get; set; }

        public bool AddItem(T item, int occurancy, int maxOccurancy)
        {
            Entries = Entries ?? new List<TreasureTableEntry<T>>();
            // TODO: check if already exists ?
            Entries.Add(new TreasureTableEntry<T>
            {
                Value = item,
                Occurancy = occurancy,
                MaxOccurancy = maxOccurancy
            });
            return true;
        }

        public T GenerateLoot()
        {
            T randomId = RandomManager.RandomOccurancy(Entries);
            if (randomId.Equals(default))
                Log.Default.WriteLine(LogLevels.Warning, "TreasureTable.GenerateLoot: no loot found");
            return randomId;
        }

        public T GenerateLoot(IEnumerable<T> history)
        {
            if (Entries == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "TreasureTable.GenerateLoot: No entries");
                return default; // max occurancy reached, no loot
            }
            TreasureTableEntry<T> randomEntry = RandomManager.RandomOccurancy<TreasureTableEntry<T>, T>(Entries);
            if (randomEntry == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "TreasureTable.GenerateLoot: no loot found");
                return default;
            }
            //Log.Default.WriteLine(LogLevels.Debug, "Loot: {0}", randomEntry.Value);
            if (history.Count(x => x.Equals(randomEntry.Value)) >= randomEntry.MaxOccurancy)
            {
                //Log.Default.WriteLine(LogLevels.Debug, "Loot rejected #>Max");
                return default; // max occurancy reached, no loot
            }
            return randomEntry.Value;
        }
    }
}
