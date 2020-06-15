using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Wyvern : RaceBase
    {
        public override string Name => "wyvern";
        public override Sizes Size => Sizes.Medium;
        public override CharacterFlags CharacterFlags => CharacterFlags.Flying | CharacterFlags.DetectHidden | CharacterFlags.DetectInvis;
        public override IRVFlags Immunities => IRVFlags.Poison;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.Light;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Poison | BodyForms.Animal | BodyForms.Dragon;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Feet | BodyParts.Ear | BodyParts.Eye | BodyParts.Tail | BodyParts.Fangs | BodyParts.Scales | BodyParts.Wings;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Bash | OffensiveFlags.Dodge | OffensiveFlags.Fast | OffensiveFlags.Bite;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
