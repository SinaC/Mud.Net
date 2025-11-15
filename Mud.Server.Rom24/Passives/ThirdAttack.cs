using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives;

[Passive(PassiveName, LearnDifficultyMultiplier = 6)]
public class ThirdAttack : PassiveBase
{
    private const string PassiveName = "Third Attack";

    public ThirdAttack(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    protected override bool CheckSuccess(ICharacter user, ICharacter victim, int learnPercentage, int diceRoll)
    {
        var npc = user as INonPlayableCharacter;
        if (user.CharacterFlags.IsSet("Slow") && (npc == null || !npc.OffensiveFlags.IsSet("Fast")))
            return false;
        int chance = learnPercentage / 2;

        return diceRoll < chance;
    }

}
