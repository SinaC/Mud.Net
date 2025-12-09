using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.POC.Classes;

[Export(typeof(IClass)), Shared]
public class Druid : ClassBase
{
    //private readonly List<ResourceKinds> _rageOnly = new List<ResourceKinds>
    //{
    //    Domain.ResourceKinds.Rage
    //};

    //private readonly List<ResourceKinds> _energyOnly = new List<ResourceKinds>
    //{
    //    Domain.ResourceKinds.Energy
    //};

    //private readonly List<ResourceKinds> _manaOnly = new List<ResourceKinds>
    //{
    //    Domain.ResourceKinds.Mana
    //};

    #region IClass

    public override string Name => "druid";

    public override string ShortName => "Dru";

    //public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
    //{
    //    Domain.ResourceKinds.Mana, // others
    //    Domain.ResourceKinds.Energy, // cat form
    //    Domain.ResourceKinds.Rage // bear form
    //};
    public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
    {
        Domain.ResourceKinds.Mana
    };

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
    {
        //switch (form)
        //{
        //    case Forms.Bear:
        //        return _rageOnly;
        //    case Forms.Cat:
        //        return _energyOnly;
        //    default:
        //        return _manaOnly;
        //}
        return ResourceKinds;
    }

    public override BasicAttributes PrimeAttribute => BasicAttributes.Constitution;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, 0);

    public override int MinHitPointGainPerLevel => 8;

    public override int MaxHitPointGainPerLevel => 12;

    #endregion

    public Druid(ILogger<Druid> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, abilityManager, abilityGroupManager)
    {
        // Test class with all skills + Passive
        foreach (var abilityDefinition in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Skill))
            if (StringCompareHelpers.StringEquals(abilityDefinition.Name, "Berserk"))
                AddAbility(20, abilityDefinition.Name, Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
            else
                AddSkill(20, abilityDefinition.Name, 1);
        foreach (var abilityDefinition in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Passive || x.Type == AbilityTypes.Weapon))
            AddAbility(10, abilityDefinition.Name, null, 0, CostAmountOperators.None, 1);
    }
}
