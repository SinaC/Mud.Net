﻿using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Passive;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 8)]
    public class Meditation : PassiveBase
    {
        public const string PassiveName = "Meditation";

        public Meditation(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
