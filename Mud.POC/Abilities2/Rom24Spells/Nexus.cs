using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Transportation, PulseWaitTime = 36)]
    public class Nexus : TransportationSpellBase
    {
        public const string SpellName = "Nexus";

        private IItemManager ItemManager { get; }
        private ISettings Settings { get; }

        public Nexus(IRandomManager randomManager, IItemManager itemManager, ISettings settings)
            : base(randomManager)
        {
            ItemManager = itemManager;
            Settings = settings;
        }

        protected override void Invoke()
        {
            // search warpstone
            if (Caster is INonPlayableCharacter || (Caster is IPlayableCharacter pcCaster && !pcCaster.IsImmortal))
            {
                IItemWarpstone stone = Caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
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
            IItemPortal portal1 = ItemManager.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, Caster.Room) as IItemPortal;
            if (portal1 != null)
            {
                portal1.SetTimer(TimeSpan.FromMinutes(duration));
                portal1.ChangeDestination(Victim.Room);
                portal1.SetCharge(1, 1);

                Caster.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal1);
                Caster.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal1);
            }

            if (Caster.Room == Victim.Room)
                return; // no second portal if rooms are the same

            // create portal two (victim -> Caster)
            IItemPortal portal2 = ItemManager.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, Victim.Room) as IItemPortal;
            if (portal2 != null)
            {
                portal2.SetTimer(TimeSpan.FromMinutes(duration));
                portal2.ChangeDestination(Caster.Room);
                portal2.SetCharge(1, 1);

                Victim.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal2);
                Victim.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal2);
            }
        }
    }
}
