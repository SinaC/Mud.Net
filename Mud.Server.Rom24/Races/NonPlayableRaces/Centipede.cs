﻿using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces
{
    public class Centipede : RaceBase
    {
        public override string Name => "centipede";
        public override Sizes Size => Sizes.Small;
        public override ICharacterFlags CharacterFlags => new CharacterFlags("DarkVision");
        public override IIRVFlags Immunities => new IRVFlags();
        public override IIRVFlags Resistances => new IRVFlags("Pierce", "Cold");
        public override IIRVFlags Vulnerabilities => new IRVFlags("Bash");
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override IBodyForms BodyForms => new BodyForms("Poison", "Animal", "Insect");
        public override IBodyParts BodyParts => new BodyParts("Head", "Legs", "Eye");
        public override IActFlags ActFlags => new ActFlags();
        public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
        public override IAssistFlags AssistFlags => new AssistFlags();
    }
}
