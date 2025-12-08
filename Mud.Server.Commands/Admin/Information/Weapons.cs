using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("Weapons", "Information")]
[Syntax(
    "[cmd]",
    "[cmd] <class>",
    "[cmd] <race>")]
public class Weapons : AbilitiesAdminGameActionBase
{
    public Weapons(IAbilityManager abilityManager, IClassManager classManager, IRaceManager raceManager)
        : base(abilityManager, classManager, raceManager)
    {
    }

    protected override AbilityTypes? AbilityTypesFilter => AbilityTypes.Weapon;
}
