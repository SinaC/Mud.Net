using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
[Help(
@"Training in third through seventh attacks allows the character a chance
at an additional strike in a combat, and increases the chance of all
previous attacks as well.  Perfect skill in an attack does NOT assure that
many attacks per round.  The amount of attacks a player may learn is
dependant on his or her class.")]
[OneLineHelp("allows the skilled warrior to land three blows in one round")]
public class ThirdAttack : PassiveBase, IAdditionalHitPassive
{
    private const string PassiveName = "Third Attack";

    public ThirdAttack(ILogger<ThirdAttack> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public int AdditionalHitIndex => 3;
    public bool StopMultiHitIfFailed => true; // stop multi hit if second attack failed

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        var npc = user as INonPlayableCharacter;
        if (user.CharacterFlags.IsSet("Slow") && (npc == null || !npc.OffensiveFlags.IsSet("Fast")))
            return false;
        int chance = learnPercentage / 2;

        return diceRoll < chance;
    }
}
