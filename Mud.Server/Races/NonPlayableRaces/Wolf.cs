using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Races.NonPlayableRaces
{
    public class Wolf : RaceBase
    {
        public override string Name => "wolf";
        public override Sizes Size => Sizes.Small;
        public override CharacterFlags CharacterFlags => CharacterFlags.DarkVision | CharacterFlags.DetectHidden;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.None;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Animal | BodyForms.Mammal | BodyForms.Fur;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Ear | BodyParts.Eye | BodyParts.Tail | BodyParts.Fangs | BodyParts.Claws;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Dodge | OffensiveFlags.Fast | OffensiveFlags.Bite;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
