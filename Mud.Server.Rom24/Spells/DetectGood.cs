﻿using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Detection)]
    [AbilityCharacterWearOffMessage("The gold in your vision disappears.")]
    [AbilityDispellable]
    public class DetectGood : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Good";

        public DetectGood(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override ICharacterFlags CharacterFlags => new CharacterFlags("DetectGood");
        protected override string SelfAlreadyAffected => "You can already sense good.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already detect good.";
        protected override string SelfSuccess => "Your eyes tingle.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
