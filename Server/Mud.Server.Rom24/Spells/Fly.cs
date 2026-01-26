using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 18), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[AbilityCharacterWearOffMessage("You slowly float to the ground.")]
[AbilityDispellable("{0:N} falls to the ground!")]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell enables the target character to fly.")]
[OneLineHelp("allows the target to fly over nearly all obstacles")]
public class Fly : DefensiveSpellBase
{
    private const string SpellName = "Fly";

    private IAuraManager AuraManager { get; }

    public Fly(ILogger<Fly> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.IsSet("Flying"))
        {
            if (Victim == Caster)
                Caster.Send("You are already airborne.");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} doesn't need your help to fly.", Victim);
            return;
        }
        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level + 3), new AuraFlags(), true,
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Flying"), Operator = AffectOperators.Or });
        Caster.Act(ActOptions.ToAll, "{0:P} feet rise off the ground.", Victim);
    }
}
