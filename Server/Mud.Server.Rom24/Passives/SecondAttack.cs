using Microsoft.Extensions.Logging;
using Mud.Random;
using Mud.Server.Ability.Passive;
using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
[Help(
@"Training in second attack allows the character a chance at additional strikes
in combat -- allow a 100% second attack does NOT guarantee 2 attacks every
round.  Any class may learn this skill, although clerics and mages have a 
very hard time with it.")]
[OneLineHelp("with training, the skilled thief can hit twice as fast")]
public class SecondAttack : PassiveBase, IAdditionalHitPassive
{
    private const string PassiveName = "Second Attack";

    protected override string Name => PassiveName;

    public SecondAttack(ILogger<SecondAttack> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public int AdditionalHitIndex => 2;
    public bool StopMultiHitIfFailed => true; // stop multi hit if second attack failed

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        int chance = learnPercentage / 2;
        var npc = user as INonPlayableCharacter;
        if (user.CharacterFlags.IsSet("Slow") && (npc == null || !npc.OffensiveFlags.IsSet("Fast")))
            chance /= 2;

        return diceRoll < chance;
    }
}
