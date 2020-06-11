using Mud.Container;
using Mud.Logger;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("zap", "Abilities", "Skills")]
    [Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    public class Wands : ItemCastSpellSkillBase<IItemWand>
    {
        public const string SkillName = "Wands";

        protected IEntity Target { get; private set; }

        public Wands(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
            : base(randomManager, abilityManager, itemManager)
        {
        }

        protected override bool Invoke()
        {
            bool success;
            if (Target != null)
                User.Act(ActOptions.ToAll, "{0:N} zap{0:v} {1} with {2}.", User, Target, Item);
            else
                User.Act(ActOptions.ToAll, "{0:N} use{0:v} {1}.", User, Item);
            int chance = 20 + (4 * Learned) / 5;
            if (User.Level < Item.Level
                || !RandomManager.Chance(chance))
            {
                User.Act(ActOptions.ToAll, "{0:P} efforts with {1} produce only smoke and sparks.", User, Item);
                success = false;
            }
            else
            {
                CastSpells();
                success = true;
            }
            Item.Use();

            if (Item.CurrentChargeCount <= 0)
            {
                User.Act(ActOptions.ToAll, "{0:P} {1} explodes into fragments.", User, Item);
                ItemManager.RemoveItem(Item);
            }

            return success;
        }

        protected override string SetTargets(SkillActionInput skillActionInput)
        {
            Item = User.GetEquipment<IItemWand>(EquipmentSlots.OffHand);
            if (Item == null)
                return "You can zap only with a wand.";
            if (Item.CurrentChargeCount == 0)
            {
                User.Act(ActOptions.ToAll, "{0:P} {1} explodes into fragments.", User, Item);
                ItemManager.RemoveItem(Item);
                return string.Empty; // stop but don't display anything
            }
            return SetupSpellAndPredefinedTarget(Item.SpellName, Item.SpellLevel, skillActionInput.RawParameters, skillActionInput.Parameters);
            // return SetupSpell(Item.SpellName, Item.SpellLevel, skillActionInput.RawParameters, skillActionInput.Parameters);
        }

        private string SetupSpellAndPredefinedTarget(string spellName, int spellLevel, string rawParameters, params CommandParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(spellName))
                return null; // not really an error but don't continue
            var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
            if (abilityInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Unknown spell '{0}' on item {1}.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }
            if (DependencyContainer.Current.GetRegistration(abilityInfo.AbilityExecutionType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "Spell '{0}' on item {1} has been not found in DependencyContainer.", spellName, Item.DebugName);
                return "Something goes wrong.";
            }

            SpellActionInput spellActionInput;
            // no target specified
            if (parameters.Length == 0)
            {
                spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = null }, rawParameters, parameters);
            }
            else
            {
                var getTargetedAction = DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType) as ITargetedAction;
                if (getTargetedAction == null)
                {
                    spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = null }, rawParameters, parameters);
                }
                else
                {
                    IEnumerable<IEntity> predefinedTargets = getTargetedAction.ValidTargets(User);
                    if (parameters.Length == 0)
                    {
                        Target = User.Fighting;
                        if (Target == null)
                            return "Zap whom or what?";
                    }
                    else
                        Target = FindHelpers.FindByName(predefinedTargets, parameters[0]);
                    if (Target == null)
                        return "You can't find it.";

                    spellActionInput = new SpellActionInput(abilityInfo, User, spellLevel, new CastFromItemOptions { Item = Item, PredefinedTarget = Target }, rawParameters, parameters);
                }
            }
            var spellInstance = (ISpell)DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType);
            string spellInstanceGuards = spellInstance.Setup(spellActionInput);
            if (spellInstanceGuards != null)
                return spellInstanceGuards;
            SpellInstances.Add(spellInstance);

            return null;
        }
    }
}
