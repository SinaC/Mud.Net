using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Cure), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax(
    "cast [spell] <character>",
    "cast [spell] <object>")]
[Help(
@"This spell removes a curse from a character, and might possibly uncurse
a cursed object.  It may also be targeted on an object in the caster's
inventory, in which case it's chance of success is significantly higher.")]
[OneLineHelp("removes malevolent magic from players and items")]
public class RemoveCurse : ItemOrDefensiveSpellBase
{
    private const string SpellName = "Remove Curse";

    private IDispelManager DispelManager { get; }

    public RemoveCurse(ILogger<RemoveCurse> logger, IRandomManager randomManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        DispelManager = dispelManager;
    }

    protected override void Invoke(ICharacter victim)
    {
        if (DispelManager.TryDispel(Level, victim, "Curse") == TryDispelReturnValues.Dispelled)
        {
            victim.Send("You feel better.");
            victim.Act(ActOptions.ToRoom, "{0:N} looks more relaxed.", victim);
        }

        // attempt to remove curse on one item in inventory or equipment
        foreach (var carriedItem in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item!)).Where(x => (x.ItemFlags.HasAny("NoDrop", "NoRemove")) && !x.ItemFlags.IsSet("NoUncurse")))
            if (!DispelManager.SavesDispel(Level, carriedItem.Level, 0))
            {
                carriedItem.RemoveBaseItemFlags(true, "NoRemove", "NoDrop");
                victim.Act(ActOptions.ToAll, "{0:P} {1} glows blue.", victim, carriedItem);
                break;
            }
    }

    protected override void Invoke(IItem item)
    {
        if (item.ItemFlags.HasAny("NoDrop", "NoRemove"))
        {
            if (!item.ItemFlags.IsSet("NoUncurse") && !DispelManager.SavesDispel(Level + 2, item.Level, 0))
            {
                item.RemoveBaseItemFlags(true, "NoRemove", "NoDrop");
                Caster.Act(ActOptions.ToAll, "{0:N} glows blue.", item);
                return;
            }
            Caster.Act(ActOptions.ToCharacter, "The curse on {0} is beyond your power.", item);
            return;
        }
        Caster.Act(ActOptions.ToCharacter, "There doesn't seem to be a curse on {0}.", item);
    }
}
