using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Transportation, PulseWaitTime = 24)]
    public class Portal : TransportationSpellBase
    {
        public const string SpellName = "Portal";

        private IItemManager ItemManager { get; }
        private ISettings Settings { get; }

        public Portal(IRandomManager randomManager, IItemManager itemManager, ISettings settings)
            : base(randomManager)
        {
            ItemManager = itemManager;
            Settings = settings;
        }

        protected override void Invoke()
        {
            if (Caster is INonPlayableCharacter || (Caster is IPlayableCharacter pcCaster && !pcCaster.IsImmortal))
            {
                // search warpstone
                IItemWarpstone stone = Caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
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
