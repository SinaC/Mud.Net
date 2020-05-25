using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterBuffSpellBase : DefensiveSpellBase, ICharacterBuff
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterBuffSpellBase(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        #region IAbility

        public override AbilityFlags Flags => AbilityFlags.CanBeDispelled;
        public override AbilityEffects Effects => AbilityEffects.Buff;

        #endregion

        #region DefensiveSpellBase

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if (IsAffected(caster, level, victim))
                return;
            (int level, TimeSpan duration, IAffect[] affects) aura = AuraInfo(caster, level, victim);
            AuraManager.AddAura(victim, this, caster, aura.level, aura.duration, AuraFlags.None, true, aura.affects);
            victim.Act(ActOptions.ToCharacter, VictimAffectMessage, caster);
            if (victim != caster)
                caster.Act(ActOptions.ToCharacter, CasterAffectMessage, victim);
        }

        #endregion

        #region ICharacterBuff

        public abstract string CharacterWearOffMessage { get; }

        #endregion

        protected abstract string AlreadyAffectedMessage { get; }
        protected abstract string VictimAffectMessage { get; }
        protected abstract string CasterAffectMessage { get; }

        protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo(ICharacter caster, int level, ICharacter victim);

        protected virtual bool IsAffected(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.GetAura(this) != null)
            {
                caster.Act(ActOptions.ToCharacter, AlreadyAffectedMessage, victim);
                return true;
            }
            return false;
        }
    }
}
