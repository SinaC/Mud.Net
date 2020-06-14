using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Race.NonPlayableRace
{
    public class Centipede : RaceBase
    {
        public override string Name => "centipede";
        public override Sizes Size => Sizes.Small;
        public override CharacterFlags CharacterFlags => CharacterFlags.DarkVision;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.Pierce | IRVFlags.Cold;
        public override IRVFlags Vulnerabilities => IRVFlags.Bash;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Poison | BodyForms.Animal | BodyForms.Insect;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Legs | BodyParts.Eye;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.None;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
