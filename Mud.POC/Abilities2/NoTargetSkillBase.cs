﻿using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class NoTargetSkillBase : SkillBase
    {
        protected NoTargetSkillBase(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        protected override string SetTargets(SkillActionInput skillActionInput) => null;
    }
}
