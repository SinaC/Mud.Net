using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Modron : RaceBase
    {
        public override string Name => "modron";
        public override Sizes Size => Sizes.Medium;
        public override CharacterFlags CharacterFlags => CharacterFlags.Infrared;
        public override IRVFlags Immunities => IRVFlags.Charm | IRVFlags.Negative | IRVFlags.Holy | IRVFlags.Mental | IRVFlags.Disease;
        public override IRVFlags Resistances => IRVFlags.Fire | IRVFlags.Cold | IRVFlags.Acid;
        public override IRVFlags Vulnerabilities => IRVFlags.None;
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
        public override BodyForms BodyForms => BodyForms.Sentient;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Arms | BodyParts.Legs | BodyParts.Hands | BodyParts.Feet | BodyParts.Ear | BodyParts.Eye | BodyParts.Fingers;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Fast | OffensiveFlags.Bite;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
