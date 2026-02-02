using Mud.Server.Ability.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Commands.Admin.Information;

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

    protected override AbilityTypes? AbilityTypesFilter => AbilityTypes.Passive;
}
