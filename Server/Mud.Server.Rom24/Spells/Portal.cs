using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation, PulseWaitTime = 24)]
[Syntax("cast [spell] <target>")]
[Help(
@"The portal spell is similar to gate, but creates a lasting one-way portal
to the target creature, instead of transporting the caster.  Portals are
entered using 'enter' or 'go' command, as in 'go portal'.  Portals cannot
be made to certain destinations, nor used to escape from gate-proof rooms.
Portal requires a special source of power to be used, unfortunately the
secret of this material component has been lost...")]
[OneLineHelp("creates a one-way portal to a destination")]
public class Portal : TransportationSpellBase
{
    private const string SpellName = "Portal";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    private IItemManager ItemManager { get; }
    private int PortalBlueprintId { get; }

    public Portal(ILogger<Portal> logger, IRandomManager randomManager, ICharacterManager characterManager, IItemManager itemManager, IOptions<Rom24Options> options)
        : base(logger, randomManager, characterManager)
    {
        ItemManager = itemManager;
        PortalBlueprintId = options.Value.SpellBlueprintIds.Portal;
    }

    protected override void Invoke()
    {
        if (!Caster.ImmortalMode.IsSet("Infinite"))
        {
            // search warpstone
            var stone = Caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
            if (stone == null)
            {
                Caster.Send("You lack the proper component for this spell.");
                return;
            }

            // destroy warpsone
            Caster.Act(ActOptions.ToCharacter, "You draw upon the power of {0}.", stone);
            Caster.Act(ActOptions.ToCharacter, "It flares brightly and vanishes!");
            ItemManager.RemoveItem(stone);
        }

        // create portal
        var portal = ItemManager.AddItem<IItemPortal>(Guid.NewGuid(), PortalBlueprintId, $"SpellPortal[{Caster.DebugName}]", Caster.Room);
        if (portal == null)
        {
            Caster.Send("The spell fails to create a portal.");
            return;
        }
        int duration = 2 + Level / 25;
        portal.SetTimer(TimeSpan.FromMinutes(duration));
        portal.ChangeDestination(Victim.Room);
        portal.SetCharge(1 + Level / 25, 1 + Level / 25);

        Caster.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal);
        Caster.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal);
    }
}
