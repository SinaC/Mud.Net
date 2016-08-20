using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;

namespace Mud.Server
{
    public class Quest : IQuest
    {
        private readonly Dictionary<int, int> _killObjectivesKilled;
        private readonly List<QuestObjectiveBase> _objectives;
        private readonly ICharacter _character;

        public Quest(QuestBlueprint blueprint, ICharacter character, ICharacter giver)
        {
            _character = character;
            Blueprint = blueprint;
            Giver = giver;
            _killObjectivesKilled = blueprint.KillObjectives?.ToDictionary(x => x.CharacterBlueprintId, x => 0);
            _objectives = new List<QuestObjectiveBase>();
            if (Blueprint.ItemObjectives != null)
            {
                foreach (QuestItemObjective itemObjective in Blueprint.ItemObjectives)
                {
                    ItemBlueprintBase itemBlueprint = Repository.World.GetItemBlueprint(itemObjective.ItemBlueprintId);
                    if (itemBlueprint != null)
                        _objectives.Add(new ItemQuestObjective
                        {
                            Blueprint = itemBlueprint,
                            Count = character.Content.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == itemObjective.ItemBlueprintId), // should always be 0
                            Total = itemObjective.Count
                        });
                    else
                        Log.Default.WriteLine(LogLevels.Warning, $"Item objective {itemObjective.ItemBlueprintId} doesn't exist for quest {blueprint.Id}");
                }
            }
            if (Blueprint.KillObjectives != null)
                foreach (QuestKillObjective killObjective in Blueprint.KillObjectives)
                {
                    CharacterBlueprint characterBlueprint = Repository.World.GetCharacterBlueprint(killObjective.CharacterBlueprintId);
                    if (characterBlueprint != null)
                        _objectives.Add(new KillQuestObjective
                        {
                            Blueprint = characterBlueprint,
                            Count = 0,
                            Total = killObjective.Count
                        });
                    else
                        Log.Default.WriteLine(LogLevels.Warning, $"Item objective {killObjective.CharacterBlueprintId} doesn't exist for quest {blueprint.Id}");
                }
        }

        #region IQuest

        public QuestBlueprint Blueprint { get; }

        public ICharacter Giver { get; }

        public IEnumerable<QuestObjectiveBase> Objectives {
            get
            {
                // Update item/kill
                if (_objectives != null)
                {
                    foreach (ItemQuestObjective itemObjective in _objectives.OfType<ItemQuestObjective>())
                        itemObjective.Count = Math.Min(itemObjective.Total, _character.Content.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == itemObjective.Blueprint.Id) + _character.Equipments.Where(x => x.Item?.Blueprint != null).Count(x => x.Item.Blueprint.Id == itemObjective.Blueprint.Id));
                    foreach (KillQuestObjective killObjective in _objectives.OfType<KillQuestObjective>())
                        killObjective.Count = Math.Min(killObjective.Total, _killObjectivesKilled[killObjective.Blueprint.Id]);
                }
                //
                return _objectives;
            }
        }

        public void Update(ICharacter victim)
        {
            if (victim.Blueprint == null)
                return;
            if (_killObjectivesKilled.ContainsKey(victim.Blueprint.Id))
                _killObjectivesKilled[victim.Blueprint.Id]++;
        }

        public bool IsCompleted
        {
            get
            {
                //// Check kill objectives
                //if (_killObjectivesKilled != null && _killObjectivesKilled.Any(x => x.Value != 0))
                //    return false;
                //// Check item objectives
                //if (Blueprint.ItemObjectives != null && Blueprint.ItemObjectives.Any(o => _character.Content.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == o.ItemBlueprintId) < o.Count))
                //    return false;
                ////foreach (QuestItemObjective objective in Blueprint.ItemObjectives)
                ////{
                ////    int count = character.Content.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == objective.ItemBlueprintId);
                ////    if (count < objective.Count)
                ////        return false;
                ////}
                return Objectives == null || Objectives.All(x => x.Count >= x.Total);
            }
        }

        public void Complete()
        {
            // TODO: give xp/gold/loot
            if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
                DestroyQuestItems();
        }

        public void Abandon()
        {
            // TODO: xp loss ?
            if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
                DestroyQuestItems();
        }

        #endregion

        private void DestroyQuestItems()
        {
            // Gather quest items
            List<IItem> questItems = _character.Content.Where(x => x.Blueprint != null && Blueprint.ItemObjectives.Any(i => i.ItemBlueprintId == x.Blueprint.Id)).ToList();
            foreach (IItem questItem in questItems)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Destroying quest item {0} in {1}", questItem.DebugName, _character.DebugName);
                Repository.World.RemoveItem(questItem);
            }
        }
    }
}
