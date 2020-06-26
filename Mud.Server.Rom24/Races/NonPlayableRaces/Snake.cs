using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Snake : RaceBase
    {
        public override string Name => "snake";
        public override Sizes Size => Sizes.Small;
        public override ICharacterFlags CharacterFlags => new CharacterFlags();
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Poison;
        public override IRVFlags Vulnerabilities => IRVFlags.Cold;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible| BodyForms.Animal | BodyForms.Reptile | BodyForms.Snake | BodyForms.ColdBlood;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Eye | BodyParts.Tail | BodyParts.Claws | BodyParts.Scales | BodyParts.LongTongue;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Bite;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
