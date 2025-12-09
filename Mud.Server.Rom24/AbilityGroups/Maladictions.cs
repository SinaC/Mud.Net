using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"Maladictions are a group of curses and other baneful spells, designed to
cripple, inconvenience, or torture, rather than kill outright.  These spells
may be cast by any class.")]
[OneLineHelp("a selection of curses fit for any witch")]
[Export(typeof(IAbilityGroup)), Shared]
public class Maladictions : AbilityGroupBase
{
    public Maladictions()
    {
        AddAbility("blindness");
        AddAbility("change sex");
        AddAbility("curse");
        AddAbility("energy drain");
        AddAbility("plague");
        AddAbility("poison");
        AddAbility("slow");
        AddAbility("weaken");
    }

    #region IAbilityGroup

    public override string Name => "maladictions";

    #endregion
}
