using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterFlagsSpellBase : DefensiveSpellBase, IAbilityCharacterBuff
    {
        protected IAuraManager AuraManager { get; }

        public CharacterFlagsSpellBase(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags))
            {
                if (victim == caster)
                    caster.Send(SelfAlreadyAffected);
                else
                    caster.Act(ActOptions.ToCharacter, NotSelfAlreadyAffected, victim);
                return;
            }
            TimeSpan duration = Duration(level);
            AuraManager.AddAura(victim, this, caster, level, duration, AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags, Operator = AffectOperators.Or });
            victim.Send(Success);
            if (victim != caster)
                victim.Act(ActOptions.ToRoom, NotSelfSuccess, victim);
        }

        #region IAbility

        public override AbilityEffects Effects => AbilityEffects.Buff;

        #endregion

        #region IAbilityCharacterBuff

        public abstract string CharacterWearOffMessage { get; }

        #endregion

        protected abstract CharacterFlags CharacterFlags { get; }
        protected abstract TimeSpan Duration(int level);
        protected abstract string SelfAlreadyAffected { get; }
        protected abstract string NotSelfAlreadyAffected { get; }
        protected abstract string Success { get; }
        protected abstract string NotSelfSuccess { get; }
    }
}
