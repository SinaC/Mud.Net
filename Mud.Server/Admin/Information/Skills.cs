﻿using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("skills", "Information")]
    [Syntax(
        "[cmd]",
        "[cmd] <class>",
        "[cmd] <race>")]
    public class Skills : AbilitiesAdminGameActionBase
    {
        public Skills(IAbilityManager abilityManager, IClassManager classManager, IRaceManager raceManager)
            : base(abilityManager, classManager, raceManager)
        {
        }

        public override AbilityTypes? AbilityTypesFilter => AbilityTypes.Skill;
    }
}
