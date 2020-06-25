using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Bat : RaceBase
    {
        public override string Name => "bat";
        public override Sizes Size => Sizes.Tiny;
        public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying","DarkVision");
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.Light;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Animal | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Eye | BodyParts.Ear | BodyParts.Heart | BodyParts.Brains | BodyParts.Feet | BodyParts.Guts | BodyParts.Wings;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Dodge | OffensiveFlags.Fast;
        public override AssistFlags AssistFlags => AssistFlags.None;
    }
}
