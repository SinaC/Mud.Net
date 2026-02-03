using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Race;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    [Export(typeof(IRace)), Shared]
    public class Troll : RaceBase
    {
        public override string Name => "troll";
        public override Sizes Size => Sizes.Large;
        public override ICharacterFlags CharacterFlags => new CharacterFlags("Infrared", "Regeneration", "DetectHidden");
        public override IIRVFlags Immunities => new IRVFlags();
        public override IIRVFlags Resistances => new IRVFlags("Charm", "Bash");
        public override IIRVFlags Vulnerabilities => new IRVFlags("Fire", "Acid");
        public override IEnumerable<EquipmentSlots> EquipmentSlots =>
        [
            Mud.Domain.EquipmentSlots.Light,
            Mud.Domain.EquipmentSlots.Head,
            Mud.Domain.EquipmentSlots.Amulet, // 2 amulets
            Mud.Domain.EquipmentSlots.Amulet,
            Mud.Domain.EquipmentSlots.Chest,
            Mud.Domain.EquipmentSlots.Cloak,
            Mud.Domain.EquipmentSlots.Waist,
            Mud.Domain.EquipmentSlots.Wrists, // 2 wrists
            Mud.Domain.EquipmentSlots.Wrists,
            Mud.Domain.EquipmentSlots.Arms,
            Mud.Domain.EquipmentSlots.Hands,
            Mud.Domain.EquipmentSlots.Ring, // 2 rings
            Mud.Domain.EquipmentSlots.Ring,
            Mud.Domain.EquipmentSlots.Legs,
            Mud.Domain.EquipmentSlots.Feet,
            Mud.Domain.EquipmentSlots.MainHand, // 2 hands
            Mud.Domain.EquipmentSlots.OffHand,
            Mud.Domain.EquipmentSlots.Float,
        ];
        public override IBodyForms BodyForms => new BodyForms( "Edible", "Poison", "Sentient", "Biped", "Mammal");
        public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Arms", "Legs", "Heart", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Claws", "Fangs");
        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Berserk");
        public override IAssistFlags AssistFlags => new AssistFlags();
    }
}
