using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Cure)]
    public class RemoveCurse : ItemOrDefensiveSpellBase
    {
        public const string SpellName = "Remove Curse";

        private IDispelManager DispelManager { get; }

        public RemoveCurse(IRandomManager randomManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            DispelManager = dispelManager;
        }

        protected override void Invoke(ICharacter victim)
        {
            if (DispelManager.TryDispel(Level, victim, Curse.SpellName) == TryDispelReturnValues.Dispelled)
            {
                victim.Send("You feel better.");
                victim.Act(ActOptions.ToRoom, "{0:N} looks more relaxed.", victim);
            }

            // attempt to remove curse on one item in inventory or equipment
            foreach (IItem carriedItem in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).Where(x => (x.ItemFlags.HasFlag(ItemFlags.NoDrop) || x.ItemFlags.HasFlag(ItemFlags.NoRemove)) && !x.ItemFlags.HasFlag(ItemFlags.NoUncurse)))
                if (!DispelManager.SavesDispel(Level, carriedItem.Level, 0))
                {
                    carriedItem.RemoveBaseItemFlags(ItemFlags.NoRemove | ItemFlags.NoDrop, true);
                    victim.Act(ActOptions.ToAll, "{0:P} {1} glows blue.", victim, carriedItem);
                    break;
                }
            return;
        }

        protected override void Invoke(IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop) || item.ItemFlags.HasFlag(ItemFlags.NoRemove))
            {
                if (!item.ItemFlags.HasFlag(ItemFlags.NoUncurse) && !DispelManager.SavesDispel(Level + 2, item.Level, 0))
                {
                    item.RemoveBaseItemFlags(ItemFlags.NoRemove | ItemFlags.NoDrop, true);
                    Caster.Act(ActOptions.ToAll, "{0:N} glows blue.", item);
                    return;
                }
                Caster.Act(ActOptions.ToCharacter, "The curse on {0} is beyond your power.", item);
                return;
            }
            Caster.Act(ActOptions.ToCharacter, "There doesn't seem to be a curse on {0}.", item);
        }
    }
}
