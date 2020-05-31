using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterDebuffSpellBase : OffensiveSpellBase, IAbilityCharacterBuff, IAbilityDispellable
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterDebuffSpellBase(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        #region IAbility

        public override AbilityEffects Effects => AbilityEffects.Debuff;
        public override AbilityFlags Flags => AbilityFlags.CanBeDispelled;

        #endregion

        #region OffensiveSpellBase

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if (!CanAffect(caster, level, victim))
                return;
            (int level, TimeSpan duration, IAffect[] affects) aura = AuraInfo(caster, level, victim);
            AuraManager.AddAura(victim, this, caster, aura.level, aura.duration, AuraFlags.None, true, aura.affects);
            victim.Act(ActOptions.ToCharacter, VictimAffectMessage, caster);
            victim.Act(ActOptions.ToRoom, RoomAffectMessage, victim);
        }

        #endregion

        #region ICharacterBuff

        public abstract string CharacterWearOffMessage { get; }

        #endregion

        #region IDispel

        public abstract string DispelRoomMessage { get; }

        #endregion

        protected abstract SchoolTypes DebuffType { get; }
        protected abstract string VictimAffectMessage { get; }
        protected abstract string RoomAffectMessage { get; }

        protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo(ICharacter caster, int level, ICharacter victim);

        protected virtual bool CanAffect(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.GetAura(this) != null || victim.SavesSpell(level, DebuffType))
                return false;
            return true;
        }
    }
}
