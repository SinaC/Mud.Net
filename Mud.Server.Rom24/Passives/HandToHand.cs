﻿using Mud.Server.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Passives
{
    [Passive(PassiveName, LearnDifficultyMultiplier = 5)]
    public class HandToHand : PassiveBase
    {
        public const string PassiveName = "Hand to hand";

        public HandToHand(IRandomManager randomManager)
            : base(randomManager)
        {
        }
    }
}
