using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Remove Curse", AbilityEffects.Cure)]
    public class RemoveCurse : ItemOrDefensiveSpellBase
    {
        private IAbilityManager AbilityManager { get; }

        public RemoveCurse(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AbilityManager = abilityManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            if (TryDispel(Level, victim, "Curse") == CheckDispelReturnValues.Dispelled)
            {
                victim.Send("You feel better.");
                victim.Act(ActOptions.ToRoom, "{0:N} looks more relaxed.", victim);
            }

            // attempt to remove curse on one item in inventory or equipment
            foreach (IItem carriedItem in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).Where(x => (x.ItemFlags.HasFlag(ItemFlags.NoDrop) || x.ItemFlags.HasFlag(ItemFlags.NoRemove)) && !x.ItemFlags.HasFlag(ItemFlags.NoUncurse)))
                if (!SavesDispel(Level, carriedItem.Level, 0))
                {
                    carriedItem.RemoveBaseItemFlags(ItemFlags.NoRemove);
                    carriedItem.RemoveBaseItemFlags(ItemFlags.NoDrop);
                    victim.Act(ActOptions.ToAll, "{0:P} {1} glows blue.", victim, carriedItem);
                    break;
                }
            return;
        }

        protected override void Invoke(IItem Item)
        {
            if (Item.ItemFlags.HasFlag(ItemFlags.NoDrop) || Item.ItemFlags.HasFlag(ItemFlags.NoRemove))
            {
                if (!Item.ItemFlags.HasFlag(ItemFlags.NoUncurse) && !SavesDispel(Level + 2, Item.Level, 0))
                {
                    Item.RemoveBaseItemFlags(ItemFlags.NoRemove);
                    Item.RemoveBaseItemFlags(ItemFlags.NoDrop);
                    Caster.Act(ActOptions.ToAll, "{0:N} glows blue.", Item);
                    return;
                }
                Caster.Act(ActOptions.ToCharacter, "The curse on {0} is beyond your power.", Item);
                return;
            }
            Caster.Act(ActOptions.ToCharacter, "There doesn't seem to be a curse on {0}.", Item);
        }

        // TODO: refactoring, almost same code in DispelMagic, Cancellation and CureSpellBase
        protected CheckDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName) // was called check_dispel in Rom24
        {
            bool found = false;
            foreach (IAura aura in victim.Auras.Where(x => x.AbilityName == abilityName)) // no need to clone because at most one entry will be removed
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                    AbilityInfo abilityInfo = AbilityManager[aura.AbilityName];
                    string dispelRoomMessage = abilityInfo?.DispelRoomMessage;
                    if (!string.IsNullOrWhiteSpace(dispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, dispelRoomMessage, victim);
                    return CheckDispelReturnValues.Dispelled; // stop at first aura dispelled
                }
                else
                    aura.DecreaseLevel();
                found = true;
            }
            return found
                ? CheckDispelReturnValues.FoundAndNotDispelled
                : CheckDispelReturnValues.NotFound;
        }
        protected enum CheckDispelReturnValues
        {
            NotFound,
            Dispelled,
            FoundAndNotDispelled
        }
    }
}
