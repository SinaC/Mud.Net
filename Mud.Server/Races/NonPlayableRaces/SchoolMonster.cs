using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Races.NonPlayableRaces
{
    public class SchoolMonster : RaceBase
    {
        public override string Name => "school monster";
        public override Sizes Size => Sizes.Medium;
        public override CharacterFlags CharacterFlags => CharacterFlags.None;
        public override IRVFlags Immunities => IRVFlags.Summon | IRVFlags.Charm;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.Magic;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Biped | BodyForms.Animal | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Feet | BodyParts.Ear | BodyParts.Eye | BodyParts.Tail | BodyParts.Claws;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
