using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    [Export(typeof(IRace)), Shared]
    public class Troll : RaceBase
    {
        public Troll(IServiceProvider serviceProvider)
        : base(serviceProvider)
        {
        }

        public override string Name => "troll";
        public override Sizes Size => Sizes.Large;
        public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "Infrared", "Regeneration", "DetectHidden");
        public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
        public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Charm", "Bash");
        public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Fire", "Acid");
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
        public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Poison", "Sentient", "Biped", "Mammal");
        public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Arms", "Legs", "Heart", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Claws", "Fangs");
        public override IActFlags ActFlags => new ActFlags(ServiceProvider);
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider, "Berserk");
        public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
    }
}
