using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("zap", "Abilities", "Skills")]
    [Skill(SkillName, AbilityEffects.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
    public class Wands : ItemCastSpellSkillBase<IItemWand>
    {
        public const string SkillName = "Wands";

        public Wands(IRandomManager randomManager, IAbilityManager abilityManager, IItemManager itemManager)
            : base(randomManager, abilityManager, itemManager)
        {
        }

        protected override bool Invoke()
        {
            bool success;
            // TODO
            //User.Act(ActOptions.ToAll, "{0:N} zap{0:v} {1} with {2}.", User, target, Item);
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
                return string.Empty;
            }
            return SetSpellTargets(Item.SpellName, Item.SpellLevel, skillActionInput.RawParameters, skillActionInput.Parameters);
        }
        /*
        IEntity target;
            if (parameters.Length == 0)
                target = source.Fighting;
            else
                target = FindHelpers.FindByName(source.Room.People, parameters[0]) as IEntity 
                         ?? FindHelpers.FindItemHere(source, parameters[0]) as IEntity;

            if (target == null)
            {
                source.Send("Zap whom or what?");
                return UseResults.TargetNotFound;
            }

            IItemWand wand = source.GetEquipment<IItemWand>(EquipmentSlots.OffHand);
            if (wand == null)
            {
                source.Send("You can zap only with a wand.");
                return UseResults.InvalidTarget;
            }

            bool? success = null;
            if (wand.CurrentChargeCount > 0)
            {
                source.Act(ActOptions.ToAll, "{0:N} zap{0:v} {1} with {2}.", source, target, wand);
                int chance = 20 + (4 * learned) / 5;
                if (source.Level < wand.Level
                    || !RandomManager.Chance(chance))
                {
                    source.Act(ActOptions.ToAll, "{0:P} efforts with {1} produce only smoke and sparks.", source, wand);
                    success = false;
                }
                else
                {
                    CastFromItem(wand.Spell, wand.SpellLevel, source, target, rawParameters, parameters);
                    success = true;
                }
                wand.Use();
            }

            if (wand.CurrentChargeCount == 0)
            {
                source.Act(ActOptions.ToAll, "{0:P} {1} explodes into fragments.", source, wand);
                World.RemoveItem(wand);
            }

            return success.HasValue
                ? (success.Value
                    ? UseResults.Ok
                    : UseResults.Failed)
                : UseResults.InvalidParameter;
        */
    }
}
