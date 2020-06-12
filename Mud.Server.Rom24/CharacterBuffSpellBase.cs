using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24
{
    public abstract class CharacterBuffSpellBase : DefensiveSpellBase
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterBuffSpellBase(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (IsAffected)
                return;
            var auraInfo = AuraInfo;
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, auraInfo.level, auraInfo.duration, AuraFlags.None, true, auraInfo.affects);
            Victim.Act(ActOptions.ToCharacter, VictimAffectMessage, Caster);
            if (Victim != Caster)
                Caster.Act(ActOptions.ToCharacter, CasterAffectMessage, Victim);
        }

        protected abstract string SelfAlreadyAffectedMessage { get; }
        protected abstract string NotSelfAlreadyAffectedMessage { get; }
        protected abstract string VictimAffectMessage { get; }
        protected abstract string CasterAffectMessage { get; }

        protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo { get; }

        protected virtual bool IsAffected
        {
            get
            {
                if (Victim.GetAura(AbilityInfo.Name) != null)
                {
                    if (Victim != Caster)
                        Caster.Send(SelfAlreadyAffectedMessage);
                    else
                        Caster.Act(ActOptions.ToCharacter, NotSelfAlreadyAffectedMessage, Victim);
                    return true;
                }

                return false;
            }
        }
    }
}
