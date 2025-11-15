using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Settings.Interfaces;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation, PulseWaitTime = 36)]
public class Nexus : TransportationSpellBase
{
    private const string SpellName = "Nexus";

    private IItemManager ItemManager { get; }
    private ISettings Settings { get; }

    public Nexus(IRandomManager randomManager, ICharacterManager characterManager, IItemManager itemManager, ISettings settings)
        : base(randomManager, characterManager)
    {
        ItemManager = itemManager;
        Settings = settings;
    }

    protected override void Invoke()
    {
        // search warpstone
        if (Caster is INonPlayableCharacter || (Caster is IPlayableCharacter pcCaster && !pcCaster.IsImmortal))
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
        var portal1 = ItemManager.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, Caster.Room) as IItemPortal;
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
        var portal2 = ItemManager.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, Victim.Room) as IItemPortal;
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
