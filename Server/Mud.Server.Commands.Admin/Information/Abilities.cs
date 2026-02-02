using Mud.Server.Ability.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("abilities", "Information")]
[Syntax(
    "[cmd]",
    "[cmd] <class>",
    "[cmd] <race>")]
public class Abilities : AbilitiesAdminGameActionBase
{
    public Abilities(IAbilityManager abilityManager, IClassManager classManager, IRaceManager raceManager)
        : base(abilityManager, classManager, raceManager)
    {
    }

    protected override AbilityTypes? AbilityTypesFilter => null;
}
