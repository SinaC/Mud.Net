using Mud.Common;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpdamage", "MobProgram", Hidden = true)]
[Syntax("mob damage [victim|'all'] [min] [max] {lethal|kill}")]
[Help(
@"Lets mob cause unconditional damage to someone. Nasty, use with caution.
Also, this is silent, you must show your own damage message...")]
public class Damage : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastThreeArguments()];

    private IRandomManager RandomManager { get; }

    public Damage(IRandomManager randomManager)
    {
        RandomManager = randomManager;
    }

    private ICharacter[] Whom { get; set; } = default!;
    private int MinDamage { get; set; }
    private int MaxDamage { get; set; }
    private bool CanBeLethal { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // whom
        Whom = FindHelpers.Find(Actor.Room.People.Where(x => x != Actor), actionInput.Parameters[0]).ToArray();
        if (Whom.Length == 0)
            return StringHelpers.CharacterNotFound;

        // damage range
        if (!actionInput.Parameters[1].IsNumber || !actionInput.Parameters[2].IsNumber)
            return BuildCommandSyntax();

        MinDamage = actionInput.Parameters[1].AsNumber;
        MaxDamage = actionInput.Parameters[2].AsNumber;

        // lethal|kill
        if (actionInput.Parameters.Length > 3)
        {
            if (StringCompareHelpers.AnyStringStartsWith(["lethal", "kill"], actionInput.Parameters[3].Value))
                CanBeLethal = true;
            else
                return BuildCommandSyntax(); ;
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var victim in Whom)
        {
            var randomDamage = RandomManager.Range(MinDamage, MaxDamage);
            var damage = CanBeLethal
                ? randomDamage
                : Math.Min(victim[ResourceKinds.HitPoints], randomDamage);
            victim.AbilityDamage(victim, damage, SchoolTypes.None, null, false); // TODO: NoSourceDamage
        }
    }
}
