using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Detection)]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell reveals the alignment of the target character.")]
[OneLineHelp("determines the moral character of a monster or person")]
public class KnowAlignment : DefensiveSpellBase
{
    private const string SpellName = "Know Alignment";

    public KnowAlignment(ILogger<KnowAlignment> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        int ap = Victim.Alignment;
        string msg;
        if (ap > 700) msg = "{0:N} has a pure and good aura.";
        else if (ap > 350) msg = "{0:N} is of excellent moral character.";
        else if (ap > 100) msg = "{0:N} is often kind and thoughtful.";
        else if (ap > -100) msg = "{0:N} doesn't have a firm moral commitment.";
        else if (ap > -350) msg = "{0:N} lies to {0:s} friends.";
        else if (ap > -700) msg = "{0:N} is a black-hearted murderer.";
        else msg = "{0:N} is the embodiment of pure evil!.";
        Caster.Act(ActOptions.ToCharacter, msg, Victim);
    }
}
