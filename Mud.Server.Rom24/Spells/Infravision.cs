﻿using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("You no longer see in the dark.")]
    [AbilityDispellable]
    public class Infravision : CharacterFlagsSpellBase
    {
        public const string SpellName = "Infravision";

        public Infravision(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.Infrared;
        protected override TimeSpan Duration => TimeSpan.FromMinutes(2*Level);
        protected override string SelfAlreadyAffected => "You can already see in the dark.";
        protected override string NotSelfAlreadyAffected => "{0} already has infravision.";
        protected override string SelfSuccess => "Your eyes glow red.";
        protected override string NotSelfSuccess => "{0:P} eyes glow red.";
    }
}
