﻿using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Bear : RaceBase
    {
        public override string Name => "bear";
        public override Sizes Size => Sizes.Large;
        public override ICharacterFlags CharacterFlags => new CharacterFlags();
        public override IIRVFlags Immunities => new IRVFlags();
        public override IIRVFlags Resistances => new IRVFlags("Cold", "Bash");
        public override IIRVFlags Vulnerabilities => new IRVFlags();
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override IBodyForms BodyForms => new BodyForms("Edible", "Animal", "Mammal");
        public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Legs", "Heart", "Brains", "Guts", "Ear", "Eye", "Fangs", "Claws");
        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Berserk", "Disarm", "Crush", "Bite");
        public override IAssistFlags AssistFlags => new AssistFlags();
    }
}
