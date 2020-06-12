﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
    public class ColourSpray : DamageTableSpellBase
    {
        public const string SpellName = "Colour Spray";

        private IAuraManager AuraManager { get; }

        public ColourSpray(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override SchoolTypes DamageType => SchoolTypes.Light;
        protected override string DamageNoun => "colour spray";
        protected override int[] Table => new[]
        {
             0,
             0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
            58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
            65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
            73, 73, 74, 75, 76, 76, 77, 78, 79, 79
        };

        protected override void Invoke()
        {
            base.Invoke();
            if (SavesSpellResult || DamageResult != DamageResults.Done)
                return;
            IEffect<ICharacter> blindnessEffect = new BlindnessEffect(AuraManager);
            blindnessEffect.Apply(Victim, Caster, Blindness.SpellName, Level, 0);
        }
    }
}
