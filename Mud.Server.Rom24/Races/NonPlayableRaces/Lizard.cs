﻿using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Lizard : RaceBase
    {
        public override string Name => "lizard";
        public override Sizes Size => Sizes.Small;
        public override ICharacterFlags CharacterFlags => new CharacterFlags("DarkVision");
        public override IIRVFlags Immunities => new IRVFlags();
        public override IIRVFlags Resistances => new IRVFlags("Poison");
        public override IIRVFlags Vulnerabilities => new IRVFlags("Cold");
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override IBodyForms BodyForms => new BodyForms("Edible", "Animal", "Reptile", "ColdBlood");
        public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Eye", "Tail", "Fangs");
        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Bite");
        public override IAssistFlags AssistFlags => new AssistFlags();
    }
}
