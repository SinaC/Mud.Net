using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Bee : RaceBase
    {
        public override string Name => "bee";
        public override Sizes Size => Sizes.Tiny;
        public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying", "Infrared", "Haste");
        public override IRVFlags Immunities => IRVFlags.Poison;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.None;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Poison | BodyForms.Animal | BodyForms.Insect;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Wings | BodyParts.Guts;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Dodge | OffensiveFlags.Fast;
        public override AssistFlags AssistFlags => AssistFlags.Race;
    }
}
