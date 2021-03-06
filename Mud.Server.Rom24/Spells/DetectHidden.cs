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
    [AbilityCharacterWearOffMessage("You feel less aware of your surroundings.")]
    [AbilityDispellable]
    public class DetectHidden : CharacterFlagsSpellBase
    {
        public const string SpellName = "Detect Hidden";

        public DetectHidden(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override ICharacterFlags CharacterFlags => new CharacterFlags("DetectHidden");
        protected override string SelfAlreadyAffected => "You are already as alert as you can be.";
        protected override string NotSelfAlreadyAffected => "{0:N} can already sense hidden lifeforms.";
        protected override string SelfSuccess => "Your awareness improves.";
        protected override string NotSelfSuccess => "Ok.";
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level);
    }
}
