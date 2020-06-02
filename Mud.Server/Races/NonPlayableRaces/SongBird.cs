using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Races.NonPlayableRaces
{
    public class SongBird : RaceBase
    {
        public override string Name => "song bird";
        public override Sizes Size => Sizes.Tiny;
        public override CharacterFlags CharacterFlags => CharacterFlags.Flying;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.None;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Animal | BodyForms.Bird;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Feet | BodyParts.Eye | BodyParts.Wings;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Dodge | OffensiveFlags.Fast;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
