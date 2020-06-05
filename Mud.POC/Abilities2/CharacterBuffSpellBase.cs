using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterBuffSpellBase : DefensiveSpellBase
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterBuffSpellBase(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (IsAffected())
                return;
            var auraInfo = AuraInfo;
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, auraInfo.level, auraInfo.duration, AuraFlags.None, true, auraInfo.affects);
            Victim.Act(ActOptions.ToCharacter, VictimAffectMessage, Caster);
            if (Victim != Caster)
                Caster.Act(ActOptions.ToCharacter, CasterAffectMessage, Victim);
        }

        protected abstract string AlreadyAffectedMessage { get; }
        protected abstract string VictimAffectMessage { get; }
        protected abstract string CasterAffectMessage { get; }

        protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo { get; }

        protected virtual bool IsAffected()
        {
            if (Victim.GetAura(AbilityInfo.Name) != null)
            {
                Caster.Act(ActOptions.ToCharacter, AlreadyAffectedMessage, Victim);
                return true;
            }
            return false;
        }
    }
}
