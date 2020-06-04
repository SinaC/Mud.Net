using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterFlagsSpellBase : DefensiveSpellBase
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterFlagsSpellBase(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags))
            {
                if (Victim == Caster)
                    Caster.Send(SelfAlreadyAffected);
                else
                    Caster.Act(ActOptions.ToCharacter, NotSelfAlreadyAffected, Victim);
                return;
            }
            TimeSpan duration = Duration;
            AuraManager.AddAura(Victim, this, Caster, Level, duration, AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags, Operator = AffectOperators.Or });
            Victim.Send(Success);
            if (Victim != Caster)
                Victim.Act(ActOptions.ToRoom, NotSelfSuccess, Victim);
        }

        protected abstract CharacterFlags CharacterFlags { get; }
        protected abstract TimeSpan Duration { get; }
        protected abstract string SelfAlreadyAffected { get; }
        protected abstract string NotSelfAlreadyAffected { get; }
        protected abstract string Success { get; }
        protected abstract string NotSelfSuccess { get; }
    }
}
