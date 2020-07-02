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
        public override IIRVFlags Immunities => new IRVFlags("Poison");
        public override IIRVFlags Resistances => new IRVFlags("None");
        public override IIRVFlags Vulnerabilities => new IRVFlags("None");
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override IBodyForms BodyForms => new BodyForms("Poison", "Animal", "Insect");
        public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Wings", "Guts");
        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Dodge", "Fast");
        public override IAssistFlags AssistFlags => new AssistFlags("Race");
    }
}
