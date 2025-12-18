using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.AbilityGroup;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"The transportation group is used for travel, whether by flight, magical
teleportation, or walking through walls.  All classes may learn these spells,
which are among the most useful in the game.")]
[OneLineHelp("spells for getting from here to there")]
[Export(typeof(IAbilityGroup)), Shared]
public class Transportation : AbilityGroupBase
{
    public Transportation()
    {
        AddAbility("fly");
        AddAbility("gate");
        AddAbility("nexus");
        AddAbility("pass door");
        AddAbility("portal");
        AddAbility("summon");
        AddAbility("teleport");
        AddAbility("word of recall");
    }

    #region IAbilityGroup

    public override string Name => "transportation";

    #endregion
}
