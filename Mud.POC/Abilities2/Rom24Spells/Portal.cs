using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Portal", AbilityEffects.Transportation, PulseWaitTime = 24)]
    public class Portal : TransportationSpellBase
    {
        private IItemManager ItemManager { get; }
        private ISettings Settings { get; }

        public Portal(IRandomManager randomManager, IWiznet wiznet, IItemManager itemManager, ISettings settings)
            : base(randomManager, wiznet)
        {
            ItemManager = itemManager;
        }

        protected override void Invoke()
        {
            // search warpstone
            IItemWarpstone stone = Caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
            if (stone == null && (Caster as IPlayableCharacter)?.IsImmortal != true)
            {
                Caster.Send("You lack the proper component for this spell.");
                return;
            }

            // destroy warpsone
            if (stone != null)
            {
                Caster.Act(ActOptions.ToCharacter, "You draw upon the power of {0}.", stone);
                Caster.Act(ActOptions.ToCharacter, "It flares brightly and vanishes!");
                ItemManager.RemoveItem(stone);
            }

            // create portal
            IItemPortal portal = ItemManager.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, Caster.Room) as IItemPortal;
            if (portal != null)
            {
                int duration = 2 + Level / 25;
                portal.SetTimer(TimeSpan.FromMinutes(duration));
                portal.ChangeDestination(Victim.Room);
                portal.SetCharge(1 + Level / 25, 1 + Level / 25);

                Caster.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal);
                Caster.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal);
            }
        }
    }
}
