using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Rom24.Classes;

[Help(
@"Thieves are a marginal class. They do few things better than any other class,
but have the widest range of skills available.  Thieves are specialists at
thievery and covert actions, being capable of entering areas undetected where
more powerful adventurers would fear to tread.  They are better fighters than
clerics, but lack the wide weapon selection of warriors.

All thieves begin with the dagger combat skill, and are learned in steal as 
well.")]
[Export(typeof(IClass)), Shared]
public class Thief : ClassBase
{
    public Thief(ILogger<Thief> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddPassive(1, "axe", 5);
        AddPassive(1, "dagger", 2);
        AddPassive(1, "dodge", 4);
        AddPassive(1, "flail", 6);
        AddPassive(1, "haggle", 3);
        AddPassive(1, "mace", 3);
        AddPassive(1, "peek", 3);
        AddPassive(1, "polearm", 6);
        AddPassive(1, "shield block", 6);
        AddPassive(1, "spear", 4);
        AddPassive(1, "sword", 3);
        AddPassive(1, "whip", 5);
        AddSkill(1, "backstab", 5);
        AddSkill(1, "hide", 4);
        AddSkill(1, "recall", 2);
        AddSkill(1, "scrolls", 5);
        AddSkill(1, "staves", 5);
        AddSkill(1, "trip", 4);
        AddSkill(1, "wands", 5);
        AddAbility(2, "magic missile", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(2, "ventriloquate", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSkill(3, "dirt kicking", 4);
        AddSkill(4, "sneak", 4);
        AddSkill(5, "steal", 4);
        AddAbility(5, "detect magic", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(5, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSkill(6, "lore", 4);
        AddAbility(6, "chill touch", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(6, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 2);
        AddAbility(6, "detect invis", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSkill(7, "pick lock", 4);
        AddAbility(7, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 2);
        AddAbility(9, "detect poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(9, "invisibility", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSkill(10, "envenom", 4);
        AddAbility(10, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(10, "burning hands", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(10, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAbility(10, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(11, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(11, "locate object", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(11, "sleep", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddPassive(12, "second attack", 5);
        AddSkill(12, "disarm", 6);
        AddAbility(12, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(12, "detect evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(12, "detect good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(12, "detect hidden", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(12, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddPassive(13, "parry", 6);
        AddSkill(14, "kick", 6);
        AddAbility(14, "shocking grasp", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddPassive(15, "hand to hand", 6);
        AddPassive(15, "meditation", 8);
        AddAbility(15, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddPassive(16, "fast healing", 6);
        AddAbility(16, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAbility(16, "farsight", Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 2);
        AddAbility(16, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(17, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(17, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(17, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(18, "identify", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAbility(18, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(19, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAbility(20, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAbility(20, "know alignment", Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 2);
        AddAbility(22, "colour spray", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(22, "giant strength", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(23, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddPassive(24, "third attack", 10);
        AddPassive(25, "enhanced damage", 5);
        AddAbility(25, "charm person", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(25, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(25, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAbility(26, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(26, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAbility(26, "haste", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAbility(28, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAbility(29, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAbility(29, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAbility(30, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(30, "fireball", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(31, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAbility(31, "mass invis", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(32, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 2);
        AddAbility(33, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAbility(33, "heal", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAbility(34, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(35, "acid blast", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(35, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAbility(36, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(38, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 2);
        AddAbility(39, "chain lightning", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAbility(40, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAbility(40, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAbility(42, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 2);
        AddAbility(43, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAbility(45, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAbility(47, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 2);
        AddAbility(50, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAbility(50, "fire breath", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 2);
        AddAbility(50, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 4);
    }

    #region IClass

    public override string Name => "thief";

    public override string ShortName => "Thi";

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
    {
        return ResourceKinds;
    }

    public override BasicAttributes PrimeAttribute => BasicAttributes.Dexterity;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, -4);

    public override int MinHitPointGainPerLevel => 8;

    public override int MaxHitPointGainPerLevel => 13;

    #endregion

}
