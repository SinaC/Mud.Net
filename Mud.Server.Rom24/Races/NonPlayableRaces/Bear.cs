using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Bear : RaceBase
    {
        public override string Name => "bear";
        public override Sizes Size => Sizes.Large;
        public override CharacterFlags CharacterFlags => CharacterFlags.None;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Cold | IRVFlags.Bash;
        public override IRVFlags Vulnerabilities => IRVFlags.None;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible| BodyForms.Animal | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Ear | BodyParts.Eye | BodyParts.Fangs | BodyParts.Claws;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Berserk | OffensiveFlags.Disarm | OffensiveFlags.Crush | OffensiveFlags.Bite;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
