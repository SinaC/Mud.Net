using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;

namespace Mud.Server.Ability.Spell
{
    public abstract class CharacterFlagsSpellBase : DefensiveSpellBase
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterFlagsSpellBase(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
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
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, duration, AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags, Operator = AffectOperators.Or });
            Victim.Send(SelfSuccess);
            if (Victim != Caster)
                Victim.Act(ActOptions.ToRoom, NotSelfSuccess, Victim);
        }

        protected abstract CharacterFlags CharacterFlags { get; }
        protected abstract TimeSpan Duration { get; }
        protected abstract string SelfAlreadyAffected { get; }
        protected abstract string NotSelfAlreadyAffected { get; }
        protected abstract string SelfSuccess { get; }
        protected abstract string NotSelfSuccess { get; }
    }
}
