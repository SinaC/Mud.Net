using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills
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

        protected override string SetTargets(ISkillActionInput skillActionInput)
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
            IEntity target;
            string setupResult = SetupSpellAndPredefinedTarget(Item.SpellName, Item.SpellLevel, out target, skillActionInput.RawParameters, skillActionInput.Parameters);
            Target = target;
            return setupResult;
        }
    }
}
