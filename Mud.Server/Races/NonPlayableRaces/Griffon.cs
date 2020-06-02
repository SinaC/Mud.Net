using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Races.NonPlayableRaces
{
    public class Griffon : RaceBase
    {
        public override string Name => "griffon";
        public override Sizes Size => Sizes.Medium;
        public override CharacterFlags CharacterFlags => CharacterFlags.Infrared | CharacterFlags.Flying | CharacterFlags.Haste;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Charm | IRVFlags.Bash | IRVFlags.Fire;
        public override IRVFlags Vulnerabilities => IRVFlags.Pierce | IRVFlags.Cold;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Animal | BodyForms.Mammal | BodyForms.Bird;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Eye | BodyParts.Wings | BodyParts.Fangs;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
