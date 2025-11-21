using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Classes;

[Help(
@"Warriors live for combat and the thrill of battle. They are the best fighters
of all the classes, but lack the subtle skills of thieves and the magical
talents of mages and priests.  Warriors are best for those who don't mind
taking the direct approach, even when another method might be called for.

Warriors begin with skill in the sword, and gain a second attack in combat.")]
public class Warrior : ClassBase
{
    public Warrior(ILogger<Warrior> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
    }

    #region IClass

    public override string Name => "warrior";

    public override string ShortName => "War";

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } = [];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
    {
        return ResourceKinds;
    }

    public override BasicAttributes PrimeAttribute => BasicAttributes.Strength;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, -10);

    public override int MinHitPointGainPerLevel => 11;

    public override int MaxHitPointGainPerLevel => 15;

    #endregion
}
