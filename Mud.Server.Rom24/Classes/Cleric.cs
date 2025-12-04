using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Rom24.Classes;

[Help(
@"Clerics are the most defensively orientated of all the classes.  Most of their
spells focus on healing or defending the faithful, with their few combat spells
being far less powerful than those of mages. However, clerics are the best 
class by far at healing magics, and they posess an impressive area of
protective magics, as well as fair combat prowess.

All clerics begin with skill in the mace.  Other weapon or shield skills must
be purchased, many at a very dear cost.")]
[Export(typeof(IClass)), Shared]
public class Cleric : ClassBase
{
    public Cleric(ILogger<Cleric> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddPassive(1, "axe", 6);
        AddPassive(1, "dagger", 3);
        AddPassive(1, "flail", 3);
        AddPassive(1, "mace", 2);
        AddPassive(1, "polearm", 6);
        AddPassive(1, "shield block", 4);
        AddPassive(1, "spear", 4);
        AddPassive(1, "sword", 6);
        AddPassive(1, "whip", 5);
        AddSkill(1, "recall", 2);
        AddSkill(1, "scrolls", 3);
        AddSkill(1, "staves", 3);
        AddSkill(1, "wands", 3);
        AddAbility(1, "cause light", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(1, "cure light", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAbility(2, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(3, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(3, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(4, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 1);
        AddAbility(4, "detect evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(4, "detect good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(5, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(5, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddPassive(6, "meditation", 5);
        AddAbility(6, "cure blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(6, "detect magic", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(7, "bless", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(7, "cause serious", Domain.ResourceKinds.Mana, 17, CostAmountOperators.Fixed, 1);
        AddAbility(7, "cure serious", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(7, "detect poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(8, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(8, "detect invis", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(9, "fast healing", 5);
        AddAbility(9, "know alignment", Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 1);
        AddAbility(9, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(9, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(10, "hand to hand", 5);
        AddSkill(10, "lore", 6);
        AddAbility(10, "earthquake", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(10, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 1);
        AddAbility(11, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAbility(11, "detect hidden", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSkill(12, "kick", 4);
        AddAbility(12, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAbility(12, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAbility(12, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddAbility(13, "cause critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(13, "cure critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(13, "cure disease", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(13, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(14, "cure poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(14, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(15, "dispel evil", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(15, "dispel good", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(15, "locate object", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(16, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAbility(16, "farsight", Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 1);
        AddAbility(16, "heat metal", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddAbility(16, "identify", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAbility(17, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(17, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 1);
        AddAbility(17, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddPassive(18, "haggle", 8);
        AddAbility(18, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(18, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(18, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAbility(18, "remove curse", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAbility(19, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddPassive(20, "parry", 8);
        AddAbility(20, "flamestrike", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(20, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 1);
        AddPassive(21, "peek", 7);
        AddAbility(21, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAbility(21, "heal", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddPassive(22, "dodge", 8);
        AddAbility(22, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAbility(22, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAbility(23, "harm", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAbility(23, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddPassive(24, "second attack", 8);
        AddAbility(24, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAbility(24, "frenzy", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSkill(25, "pick lock", 8);
        AddAbility(25, "mass invis", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(26, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(28, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(30, "enhanced damage", 9);
        AddAbility(30, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAbility(30, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAbility(32, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 1);
        AddAbility(32, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(34, "demonfire", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(35, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAbility(35, "ray of truth", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAbility(35, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAbility(36, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 1);
        AddAbility(36, "holy word", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 2);
        AddAbility(38, "mass healing", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAbility(40, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 1);
        AddAbility(40, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAbility(43, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 1);
        AddAbility(45, "fire breath", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 1);
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
