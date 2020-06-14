﻿using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Race.NonPlayableRace
{
    public class Fido : RaceBase
    {
        public override string Name => "fido";
        public override Sizes Size => Sizes.Tiny;
        public override CharacterFlags CharacterFlags => CharacterFlags.None;
        public override IRVFlags Immunities => IRVFlags.None;
        public override IRVFlags Resistances => IRVFlags.None;
        public override IRVFlags Vulnerabilities => IRVFlags.Magic;
        public override IEnumerable<EquipmentSlots> EquipmentSlots => new List<EquipmentSlots>
        {
            Domain.EquipmentSlots.Head,
            Domain.EquipmentSlots.Chest,
            Domain.EquipmentSlots.Float,
        };
        public override BodyForms BodyForms => BodyForms.Edible | BodyForms.Poison | BodyForms.Animal | BodyForms.Mammal;
        public override BodyParts BodyParts => BodyParts.Head | BodyParts.Body | BodyParts.Legs | BodyParts.Heart | BodyParts.Brains | BodyParts.Guts | BodyParts.Feet | BodyParts.Ear | BodyParts.Eye | BodyParts.Tail | BodyParts.Fangs;
        public override ActFlags ActFlags => ActFlags.None;
        public override OffensiveFlags OffensiveFlags => OffensiveFlags.Dodge;
        public override AssistFlags AssistFlags => AssistFlags.Race;
    }
}
