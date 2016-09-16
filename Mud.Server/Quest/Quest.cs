using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Logger;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Item;
using Mud.Server.Server;

namespace Mud.Server.Quest
{
    public class Quest : IQuest
    {
        private readonly List<QuestObjectiveBase> _objectives;
        private readonly ICharacter _character;

        public Quest(QuestBlueprint blueprint, ICharacter character, ICharacter giver)
        {
            _character = character;
            Blueprint = blueprint;
            Giver = giver;
            _objectives = new List<QuestObjectiveBase>();
            if (Blueprint.ItemObjectives != null)
            {
                foreach (QuestItemObjectiveBlueprint itemObjective in Blueprint.ItemObjectives)
                {
                    ItemQuestBlueprint itemBlueprint = Repository.World.GetItemBlueprint(itemObjective.ItemBlueprintId) as ItemQuestBlueprint;
                    if (itemBlueprint != null)
                        _objectives.Add(new ItemQuestObjective
                        {
                            Blueprint = itemBlueprint,
                            Count = character.Content.Where(x => x.Blueprint != null).Count(x => x.Blueprint.Id == itemObjective.ItemBlueprintId), // should always be 0
                            Total = itemObjective.Count
                        });
                    else
                        Log.Default.WriteLine(LogLevels.Warning, $"Loot objective {itemObjective.ItemBlueprintId} doesn't exist (or is not quest item) for quest {blueprint.Id}");
                }
            }
            if (Blueprint.KillObjectives != null)
            {
                foreach (QuestKillObjectiveBlueprint killObjective in Blueprint.KillObjectives)
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
                        Log.Default.WriteLine(LogLevels.Warning, $"Kill objective {killObjective.CharacterBlueprintId} doesn't exist for quest {blueprint.Id}");
                }
            }
            if (Blueprint.LocationObjectives != null)
            {
                foreach (QuestLocationObjectiveBlueprint locationObjective in Blueprint.LocationObjectives)
                {
                    RoomBlueprint roomBlueprint = Repository.World.GetRoomBlueprint(locationObjective.RoomBlueprintId);
                    if (roomBlueprint != null)
                        _objectives.Add(new LocationQuestObjective
                        {
                            Blueprint = roomBlueprint,
                            Explored = character.Room?.Blueprint?.Id == roomBlueprint.Id
                        });
                    else
                        Log.Default.WriteLine(LogLevels.Warning, $"Location objective {locationObjective.RoomBlueprintId} doesn't exist for quest {blueprint.Id}");
                }
            }
        }

        #region IQuest

        public QuestBlueprint Blueprint { get; }

        public ICharacter Giver { get; }

        public IEnumerable<QuestObjectiveBase> Objectives => _objectives;

        public void GenerateKillLoot(ICharacter victim, IContainer container)
        {
            if (victim.Blueprint == null)
                return;
            QuestKillLootTable<int> table;
            if (!Blueprint.KillLootTable.TryGetValue(victim.Blueprint.Id, out table))
                return;
            List<int> killLoots = table.GenerateLoots();
            if (killLoots != null)
            {
                foreach (int loot in killLoots)
                {
                    ItemQuestBlueprint questItemBlueprint = Repository.World.GetItemBlueprint(loot) as ItemQuestBlueprint;
                    if (questItemBlueprint != null)
                    {
                        IItemQuest questItem = Repository.World.AddItemQuest(Guid.NewGuid(), questItemBlueprint, container);
                        Log.Default.WriteLine(LogLevels.Debug, $"Loot objective {loot} generated for {_character.DisplayName}");
                    }
                    else
                        Log.Default.WriteLine(LogLevels.Warning, $"Loot objective {loot} doesn't exist (or is not quest item) for quest {Blueprint.Id}");
                }
            }
        }

        public void Update(ICharacter victim)
        {
            if (victim.Blueprint == null)
                return;
            if (IsCompleted)
                return;
            foreach (KillQuestObjective objective in _objectives.OfType<KillQuestObjective>().Where(x => !x.IsCompleted && x.Blueprint.Id == victim.Blueprint.Id))
            {
                objective.Count++;
                _character.Send($"%y%Quest {Blueprint.Title}: {objective.CompletionState}%x%");
                if (IsCompleted)
                    _character.Send($"%R%Quest {Blueprint.Title}: complete%x%");
            }
        }

        public void Update(IItemQuest item)
        {
            if (item.Blueprint == null)
                return;
            if (IsCompleted)
                return;
            foreach (ItemQuestObjective objective in _objectives.OfType<ItemQuestObjective>().Where(x => !x.IsCompleted && x.Blueprint.Id == item.Blueprint.Id))
            {
                objective.Count++;
                _character.Send($"%y%Quest {Blueprint.Title}: {objective.CompletionState}%x%");
                if (IsCompleted)
                    _character.Send($"%R%Quest {Blueprint.Title}: complete%x%");
            }
        }

        public void Update(IRoom room)
        {
            if (room.Blueprint == null)
                return;
            if (IsCompleted)
                return;
            foreach (LocationQuestObjective objective in _objectives.OfType<LocationQuestObjective>().Where(x => !x.IsCompleted && x.Blueprint.Id == room.Blueprint.Id))
            {
                objective.Explored = true;
                _character.Send($"%y%Quest {Blueprint.Title}: {objective.CompletionState}%x%");
                if (IsCompleted)
                    _character.Send($"%R%Quest {Blueprint.Title}: complete%x%");
            }
        }

        public bool IsCompleted => Objectives == null || Objectives.All(x => x.IsCompleted);

        public void Complete()
        {
            // TODO: give xp/gold/loot
            if (Blueprint.ShouldQuestItemBeDestroyed && Blueprint.ItemObjectives != null)
                DestroyQuestItems();

            int xpGain = 0;
            int goldGain = Blueprint.Gold;

            // XP: http://wow.gamepedia.com/Experience_point#Quest_XP
            if (_character.Level < ServerOptions.MaxLevel)
            {
                int factorPercentage = 100;
                if (_character.Level == Blueprint.Level + 6)
                    factorPercentage = 80;
                else if (_character.Level == Blueprint.Level + 7)
                    factorPercentage = 60;
                else if (_character.Level == Blueprint.Level + 8)
                    factorPercentage = 40;
                else if (_character.Level == Blueprint.Level + 9)
                    factorPercentage = 20;
                else if (_character.Level >= Blueprint.Level + 10)
                    factorPercentage = 10;
                xpGain = (Blueprint.Experience*factorPercentage)/100;
            }
            else
                goldGain = Blueprint.Experience*6;

            // Display
            _character.Send("%y%You receive {0} exp and {1} gold.%x%", xpGain, goldGain);

            // Give rewards
            if (xpGain > 0)
                _character.GainExperience(xpGain);
            // TODO: goldGain
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
