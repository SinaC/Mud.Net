using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Classes;

[Help(
@"Clerics are the most defensively orientated of all the classes.  Most of their
spells focus on healing or defending the faithful, with their few combat spells
being far less powerful than those of mages. However, clerics are the best 
class by far at healing magics, and they posess an impressive area of
protective magics, as well as fair combat prowess.

All clerics begin with skill in the mace.  Other weapon or shield skills must
be purchased, many at a very dear cost.")]
public class Cleric : ClassBase
{
    public Cleric(ILogger<Cleric> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
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
