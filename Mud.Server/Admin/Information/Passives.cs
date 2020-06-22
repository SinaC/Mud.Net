using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("passives", "Information")]
    [Syntax(
        "[cmd]",
        "[cmd] <class>",
        "[cmd] <race>")]
    public class Passives : AbilitiesAdminGameActionBase
    {
        public Passives(IAbilityManager abilityManager, IClassManager classManager, IRaceManager raceManager)
            : base(abilityManager, classManager, raceManager)
        {
        }

        public override AbilityTypes? AbilityTypesFilter => AbilityTypes.Passive;
    }
}
