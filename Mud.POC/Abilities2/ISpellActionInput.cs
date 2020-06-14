﻿using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;

namespace Mud.POC.Abilities2
{
    public interface ISpellActionInput
    {
        ICharacter Caster { get; }
        string RawParameters { get; }
        CommandParameter[] Parameters { get; }
        IAbilityInfo AbilityInfo { get; }
        int Level { get; }
        CastFromItemOptions CastFromItemOptions { get; }
        bool IsCastFromItem { get; }
    }

    public class CastFromItemOptions
    {
        public IItem Item { get; set; }
        public IEntity PredefinedTarget { get; set; }
    }
}
