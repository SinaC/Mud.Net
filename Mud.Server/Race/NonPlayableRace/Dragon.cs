using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Race.NonPlayableRace
{
    public class Dragon : RaceBase
    {
        public override string Name => "dragon";
        public override Sizes Size => Sizes.Huge;
        public override CharacterFlags CharacterFlags => CharacterFlags.Infrared | CharacterFlags.Flying;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Charm | IRVFlags.Bash | IRVFlags.Fire;
        public override IRVFlags Vulnerabilities => IRVFlags.Pierce | IRVFlags.Cold;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Light,
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Amulet, // 2 amulets
            Domain.EquipmentSlots.Amulet,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Cloak,
            Domain.EquipmentSlots.Ring, // 2 rings
            Domain.EquipmentSlots.Ring,
            Domain.EquipmentSlots.Legs,
            Domain.EquipmentSlots.Feet,
            Domain.EquipmentSlots.MainHand, // 2 hands
            Domain.EquipmentSlots.OffHand,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Sentient | BodyForms.Dragon;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Hands | BodyParts.Feet | BodyParts.Fingers | BodyParts.Ear | BodyParts.Eye | BodyParts.Wings | BodyParts.Tail | BodyParts.Fangs | BodyParts.Scales | BodyParts.Claws;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
