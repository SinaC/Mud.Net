using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Admin.Information;

[AdminCommand("spells", "Information")]
[Syntax(
    "[cmd]",
    "[cmd] <class>",
    "[cmd] <race>")]
public class Spells : AbilitiesAdminGameActionBase
{
    public Spells(IAbilityManager abilityManager, IClassManager classManager, IRaceManager raceManager)
        : base(abilityManager, classManager, raceManager)
    {
    }

    protected override AbilityTypes? AbilityTypesFilter => AbilityTypes.Spell;
}
