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
    [Spell(SpellName, AbilityEffects.Cure)]
    public class CureBlindness : CureSpellBase
    {
        public const string SpellName = "Cure Blindness";

        public CureBlindness(IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
            : base(randomManager, abilityManager, dispelManager)
        {
        }

        protected override string ToCureAbilityName => Blindness.SpellName;
        protected override string SelfNotFoundMsg => "You aren't blind.";
        protected override string NotSelfFoundMsg => "{0:N} doesn't appear to be blinded.";
    }
}
