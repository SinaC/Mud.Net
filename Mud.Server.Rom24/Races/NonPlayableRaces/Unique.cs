using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    [Export(typeof(IRace)), Shared]
    public class Unique : RaceBase
    {
        public Unique(IServiceProvider serviceProvider)
        : base(serviceProvider)
        {
        }

        public override string Name => "unique";
        public override Sizes Size => Sizes.Tiny;
        public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider);
        public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
        public override IIRVFlags Resistances => new IRVFlags(ServiceProvider);
        public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider);
        public override IEnumerable<EquipmentSlots> EquipmentSlots =>
        [
            Domain.EquipmentSlots.Light,
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Amulet, // 2 amulets
            Domain.EquipmentSlots.Amulet,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Cloak,
            Domain.EquipmentSlots.Waist,
            Domain.EquipmentSlots.Wrists, // 2 wrists
            Domain.EquipmentSlots.Wrists,
            Domain.EquipmentSlots.Arms,
            Domain.EquipmentSlots.Hands,
            Domain.EquipmentSlots.Ring, // 2 rings
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.MainHand, // 2 hands
            Domain.EquipmentSlots.OffHand,
            Domain.EquipmentSlots.Float,
        ];
        public override IBodyForms BodyForms => new BodyForms(ServiceProvider);
        public override IBodyParts BodyParts => new BodyParts(ServiceProvider);
        public override IActFlags ActFlags => new ActFlags(ServiceProvider);
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider);
        public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
    }
}
