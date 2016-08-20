using System;
using System.Collections.Generic;
using Mud.Logger;
using Mud.Server.Common;

namespace Mud.Server.Blueprints.Quest
{
    // Quest loot table stores global drop percentage instead of a relative percentage
    public class QuestKillLootTable<T> // this represents additional provided by kill mob while working on this quest
        where T:IEquatable<T>
    {
        public string Name { get; set; }
        public List<QuestKillLootTableEntry<T>> Entries { get; set; }

        public bool AddItem(T item, int percentage)
        {
            Entries = Entries ?? new List<QuestKillLootTableEntry<T>>();
            // TODO: check if already exists ?
            Entries.Add(new QuestKillLootTableEntry<T>
            {
                Value = item,
                Percentage = percentage
            });
            return true;
        }

        public List<T> GenerateLoots()
        {
            List<T> loots = new List<T>();
            if (Entries != null)
            {
                foreach (QuestKillLootTableEntry<T> entry in Entries)
                {
                    int percentage = 1 + RandomizeHelpers.Instance.Randomizer.Next(0, 100); // from 1 to 100
                    if (percentage <= entry.Percentage)
                        loots.Add(entry.Value);
                }
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "QuestLootTable.GenerateLoots: No entries");
            return loots;
        }
    }
}
