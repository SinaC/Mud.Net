using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 5)]
public class SecondAttack : PassiveBase
{
    private const string PassiveName = "Second Attack";

    public SecondAttack(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        int chance = learnPercentage / 2;
        var npc = user as INonPlayableCharacter;
        if (user.CharacterFlags.IsSet("Slow") && (npc == null || !npc.OffensiveFlags.IsSet("Fast")))
            chance /= 2;

        return diceRoll < chance;
    }
}
