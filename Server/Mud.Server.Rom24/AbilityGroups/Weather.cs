using Mud.Common.Attributes;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.AbilityGroups;

[Help(
@"These spells allow the caster to manipulate local weather conditions, as well
as summon fog clouds, lightning, or even electrical glows.  This group is
usable by all classes.")]
[OneLineHelp("spells for conjuring and mastering the elements")]
[Export(typeof(IAbilityGroup)), Shared]
public class Weather : AbilityGroupBase
{
    public Weather()
    {
        AddAbility("call lightning");
        AddAbility("control weather");
        AddAbility("faerie fire");
        AddAbility("faerie fog");
        AddAbility("lightning bolt");
    }

    #region IAbilityGroup

    public override string Name => "weather";

    #endregion
}
