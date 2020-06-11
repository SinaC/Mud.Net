using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Race.NonPlayableRace
{
    public class Kobold : RaceBase
    {
        public override string Name => "kobold";
        public override Sizes Size => Sizes.Small;
        public override CharacterFlags CharacterFlags => CharacterFlags.Infrared;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Poison;
        public override IRVFlags Vulnerabilities => IRVFlags.Magic;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
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
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Poison | BodyForms.Sentient | BodyForms.Biped | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Arms | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Hands | BodyParts.Feet | BodyParts.Fingers | BodyParts.Ear | BodyParts.Eye | BodyParts.Tail;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.Race;
    }
}
