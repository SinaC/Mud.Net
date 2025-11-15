using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Classes;

public class Cleric : ClassBase
{
    public Cleric(IAbilityManager abilityManager)
        : base(abilityManager)
    {
    }

    #region IClass

    public override string Name => "cleric";

    public override string ShortName => "Cle";

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
    {
        return ResourceKinds; // always mana
    }

    public override BasicAttributes PrimeAttribute => BasicAttributes.Wisdom;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, 2);

    public override int MinHitPointGainPerLevel => 7;

    public override int MaxHitPointGainPerLevel => 10;

    #endregion
}
