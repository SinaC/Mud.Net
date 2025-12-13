using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class CharacterFlagsSpellBase : DefensiveSpellBase
{
    protected IAuraManager AuraManager { get; }
    protected IFlagFactory<ICharacterFlags, ICharacterFlagValues> FlagFactory { get; }

    protected CharacterFlagsSpellBase(ILogger<CharacterFlagsSpellBase> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory<ICharacterFlags, ICharacterFlagValues> flagFactory)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        FlagFactory = flagFactory;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.HasAny(CharacterFlags))
        {
            if (Victim == Caster)
                Caster.Send(SelfAlreadyAffected);
            else
                Caster.Act(ActOptions.ToCharacter, NotSelfAlreadyAffected, Victim);
            return;
        }
        var duration = Duration;
        AuraManager.AddAura(Victim, AbilityDefinition.Name, Caster, Level, duration, AuraFlags.None, true,
            new CharacterFlagsAffect(FlagFactory) { Modifier = CharacterFlags, Operator = AffectOperators.Or });
        Victim.Send(SelfSuccess);
        if (Victim != Caster)
            Victim.Act(ActOptions.ToRoom, NotSelfSuccess, Victim);
    }

    protected abstract ICharacterFlags CharacterFlags { get; }
    protected abstract TimeSpan Duration { get; }
    protected abstract string SelfAlreadyAffected { get; }
    protected abstract string NotSelfAlreadyAffected { get; }
    protected abstract string SelfSuccess { get; }
    protected abstract string NotSelfSuccess { get; }
}
