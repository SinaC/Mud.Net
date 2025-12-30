using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.POC.Classes;

[Export(typeof(IClass)), Shared]
public class Druid : ClassBase
{
    private readonly List<ResourceKinds> _bear =
    [
        Mud.Domain.ResourceKinds.Mana,
        Mud.Domain.ResourceKinds.Rage
    ];

    private readonly List<ResourceKinds> _cat =
    [
        Mud.Domain.ResourceKinds.Mana,
        Mud.Domain.ResourceKinds.Energy,
        Mud.Domain.ResourceKinds.Combo,
    ];

    private readonly List<ResourceKinds> _caster =
    [
        Mud.Domain.ResourceKinds.Mana
    ];

    #region IClass

    public override string Name => "druid";

    public override string ShortName => "Dru";

    public override bool SelectableDuringCreation => false;

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Mud.Domain.ResourceKinds.Mana, // normal/cat/bear shape
        Mud.Domain.ResourceKinds.Energy, // cat shape
        Mud.Domain.ResourceKinds.Combo, // cat shape
        Mud.Domain.ResourceKinds.Rage // bear shape
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Shapes shape)
        => shape switch
        {
            Shapes.Bear => _bear,
            Shapes.Cat => _cat,
            _ => _caster,
        };

    public override BasicAttributes PrimeAttribute => BasicAttributes.Constitution;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, 0);

    public override int MinHitPointGainPerLevel => 8;

    public override int MaxHitPointGainPerLevel => 12;

    #endregion

    public Druid(ILogger<Druid> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, abilityManager, abilityGroupManager)
    {
        AddAvailableAbility(5, "Bear Form", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 0, 100);
        AddAvailableAbility(10, "Demoralizing Roar", Mud.Domain.ResourceKinds.Rage, 10, CostAmountOperators.Fixed, 1, 100);
        AddAvailableAbility(15, "Cat Form", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 0, 100);
        AddAvailableAbility(15, "Claw", Mud.Domain.ResourceKinds.Energy, 45, CostAmountOperators.Fixed, 1, 100);
        AddAvailableAbility(20, "Maul", Mud.Domain.ResourceKinds.Rage, 15, CostAmountOperators.Fixed, 1, 100);
        AddAvailableAbility(24, "Rake", Mud.Domain.ResourceKinds.Energy, 40, CostAmountOperators.Fixed, 1, 100);
        AddAvailableAbility(25, "Swipe", Mud.Domain.ResourceKinds.Rage, 16, CostAmountOperators.Fixed, 1, 100);
        AddAvailableAbility(32, "Ferocious Bite", [ (Mud.Domain.ResourceKinds.Combo, 1, CostAmountOperators.AllWithMin), (Mud.Domain.ResourceKinds.Energy, 0, CostAmountOperators.All)], 1, 100);

        AddBasicAbilityGroup("druid basics");

        // to be able to test berserk
        AddAvailableAbility(18, "berserk", [(Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed), (Mud.Domain.ResourceKinds.MovePoints, 50, CostAmountOperators.PercentageCurrent)], 5, 100);

        // add weapons
        foreach (var abilityDefinition in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Weapon))
            AddAvailableAbility(10, abilityDefinition.Name, 1, 100);
        //// Test class with all skills + Passive
        //foreach (var abilityDefinition in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Skill))
        //    if (StringCompareHelpers.StringEquals(abilityDefinition.Name, "Berserk"))
        //        AddAvailableAbility(20, abilityDefinition.Name, Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        //    else
        //        AddAvailableSkill(20, abilityDefinition.Name, 1);
        //foreach (var abilityDefinition in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Passive || x.Type == AbilityTypes.Weapon))
        //    AddAvailableAbility(10, abilityDefinition.Name, null, 0, CostAmountOperators.None, 1);
    }
}
