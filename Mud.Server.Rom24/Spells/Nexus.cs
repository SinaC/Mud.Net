using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation, PulseWaitTime = 36), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax("cast [spell] <target>")]
[Help(
@"This spell is virtually identical to portal (see 'help portal'), with the
only difference being that while portal creates a one-way gate, a nexus 
spell makes a two-sided gate.  It also lasts longer than the lower-powered
portal spell.  Both spells require an additional power source, the secret
of which has been lost...")]
[OneLineHelp("forms a two-way portal to a far off destination")]
public class Nexus : TransportationSpellBase
{
    private const string SpellName = "Nexus";

    private IItemManager ItemManager { get; }
    private int PortalBlueprintId { get; }

    public Nexus(ILogger<Nexus> logger, IRandomManager randomManager, ICharacterManager characterManager, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager, characterManager)
    {
        ItemManager = itemManager;
        PortalBlueprintId = options.Value.SpellBlueprintIds.Portal;
    }

    protected override void Invoke()
    {
        // search warpstone
        if (!Caster.ImmortalMode.HasFlag(ImmortalModeFlags.Infinite))
        {
            var stone = Caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
            if (stone == null)
            {
                Caster.Send("You lack the proper component for this spell.");
                return;
            }

            // destroy warpstone
            Caster.Act(ActOptions.ToCharacter, "You draw upon the power of {0}.", stone);
            Caster.Act(ActOptions.ToCharacter, "It flares brightly and vanishes!");
            ItemManager.RemoveItem(stone);
        }

        int duration = 1 + Level / 10;

        // create portal one (Caster -> victim)
        var portal1 = ItemManager.AddItem<IItemPortal>(Guid.NewGuid(), PortalBlueprintId, Caster.Room);
        if (portal1 == null)
        {
            Caster.Send("The spell fails to create a portal.");
            return;
        }
        portal1.SetTimer(TimeSpan.FromMinutes(duration));
        portal1.ChangeDestination(Victim.Room);
        portal1.SetCharge(1, 1);

        Caster.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal1);
        Caster.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal1);

        if (Caster.Room == Victim.Room)
            return; // no second portal if rooms are the same

        // create portal two (victim -> Caster)
        var portal2 = ItemManager.AddItem<IItemPortal>(Guid.NewGuid(), PortalBlueprintId, Victim.Room);
        if (portal2 == null)
        {
            Caster.Send("The spell fails to create a portal.");
            return;
        }
        portal2.SetTimer(TimeSpan.FromMinutes(duration));
        portal2.ChangeDestination(Caster.Room);
        portal2.SetCharge(1, 1);

        Victim.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal2);
        Victim.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal2);
    }
}
