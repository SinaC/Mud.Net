using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Doll : RaceBase
    {
        public override string Name => "doll";
        public override Sizes Size => Sizes.Medium;
        public override CharacterFlags CharacterFlags => CharacterFlags.None;
        public override IRVFlags Immunities => IRVFlags.Cold | IRVFlags.Poison | IRVFlags.Negative | IRVFlags.Holy | IRVFlags.Mental | IRVFlags.Disease | IRVFlags.Drowning;
        public override IRVFlags Resistances => IRVFlags.Bash | IRVFlags.Light;
        public override IRVFlags Vulnerabilities => IRVFlags.Slash | IRVFlags.Fire | IRVFlags.Lightning | IRVFlags.Acid | IRVFlags.Energy;
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
        public override BodyForms BodyForms => BodyForms.Other| BodyForms.Construct | BodyForms.Biped | BodyForms.ColdBlood;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Arms | BodyParts.Legs | BodyParts.Hands | BodyParts.Feet | BodyParts.Eye | BodyParts.Fingers | BodyParts.Ear;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Fast | OffensiveFlags.Bite;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
