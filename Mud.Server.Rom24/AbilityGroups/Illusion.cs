using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The illusions spells are dedicated to deception and trickery.  They can be
used to mask appearances, or create distractions for the party.  Currently,
the illusion group is very small, but it will be expanded in the future.
Only thieves and mages can cast illusion spells.")]
[OneLineHelp("magics for concealing and deceiving")]
[Export(typeof(IAbilityGroup)), Shared]
public class Illusion : AbilityGroupBase
{
    public Illusion()
    {
        AddAbility("invisibility");
        AddAbility("mass invis");
        AddAbility("ventriloquate");
    }

    #region IAbilityGroup

    public override string Name => "illusion";

    #endregion
}
