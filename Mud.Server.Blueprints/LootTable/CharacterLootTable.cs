using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Common;
using Mud.Logger;
using Mud.Container;

namespace Mud.Server.Blueprints.LootTable
{
    // http://www.gammon.com.au/forum/bbshowpost.php?bbsubject_id=9715
    public class CharacterLootTable<T>
        where T:IEquatable<T>
    {
        private IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();

        public int MinLoot { get; set; }
        public int MaxLoot { get; set; }
        public List<CharacterLootTableEntry<T>> Entries { get; set; }
        //public List<T> AlwaysDrop { get; set; }

        public CharacterLootTable()
        {
            MinLoot = 1;
            MaxLoot = 1;
        }

        public bool AddTreasureList(TreasureTable<T> table, int occurancy, int maxLoot)
        {
            Entries = Entries ?? new List<CharacterLootTableEntry<T>>();
            // TODO: check if already exists ?
            Entries.Add(new CharacterLootTableEntry<T>
            {
                Value = table,
                Occurancy = occurancy,
                Max = maxLoot
            });
            return true;
        }

        public List<T> GenerateLoots()
        {
            List<CharacterLootTableEntry<T>> history = new List<CharacterLootTableEntry<T>>();
            List<T> lootList = new List<T>();
            int lootCount = RandomManager.Next(MinLoot, MaxLoot + 1);
            //Log.Default.WriteLine(LogLevels.Debug, "#Loot: {0}", lootCount);
            if (Entries != null)
            {
                for (int loop = 1; loop <= lootCount; loop++)
                {
                    CharacterLootTableEntry<T> randomEntry = RandomManager.RandomOccurancy<CharacterLootTableEntry<T>, TreasureTable<T>>(Entries);
                    if (randomEntry != null)
                    {
                        //Log.Default.WriteLine(LogLevels.Debug, "Table: {0}", randomEntry.Value.Name);
                        if (randomEntry.Value.Entries != null) // shortcut for empty table
                        {
                            if (history.Count(x => x.Value == randomEntry.Value) < randomEntry.Max) // check if max loot in this entry reached
                            {
                                history.Add(randomEntry);
                                T loot = randomEntry.Value.GenerateLoot(lootList);
                                if (!loot.Equals(default))
                                    lootList.Add(loot);
                            }
                            //else
                            //    Log.Default.WriteLine(LogLevels.Debug, "Table rejected #>Max");
                        }
                        //else
                        //    Log.Default.WriteLine(LogLevels.Debug, "Empty table");
                    }
                    else
                        Log.Default.WriteLine(LogLevels.Warning, "CharacterLootTable.GenerateLoots: no treasure table found");
                }
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "CharacterLootTable.GenerateLoots: No entries");
            //if (AlwaysDrop != null)
            //    lootList.AddRange(AlwaysDrop);
            return lootList;
        }
    }
}
