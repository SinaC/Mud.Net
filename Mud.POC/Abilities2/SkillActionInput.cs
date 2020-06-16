﻿using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Interfaces.GameAction;

namespace Mud.POC.Abilities2
{
    public class SkillActionInput : ISkillActionInput
    {
        public ICharacter User { get; }
        public string RawParameters { get; }
        public ICommandParameter[] Parameters { get; }
        public IAbilityInfo AbilityInfo { get; }

        public SkillActionInput(IActionInput actionInput, IAbilityInfo abilityInfo, ICharacter user)
        {
            User = user;
            RawParameters = actionInput.RawParameters;
            Parameters = actionInput.Parameters;
            AbilityInfo = abilityInfo;
        }
    }
}
