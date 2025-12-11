using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
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
    public Thief(ILogger<Thief> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, abilityManager, abilityGroupManager)
    {
        AddAvailablePassive(1, "axe", 5);
        AddAvailablePassive(1, "dagger", 2);
        AddAvailablePassive(1, "dodge", 4);
        AddAvailablePassive(1, "flail", 6);
        AddAvailablePassive(1, "haggle", 3);
        AddAvailablePassive(1, "mace", 3);
        AddAvailablePassive(1, "peek", 3);
        AddAvailablePassive(1, "polearm", 6);
        AddAvailablePassive(1, "shield block", 6);
        AddAvailablePassive(1, "spear", 4);
        AddAvailablePassive(1, "sword", 3);
        AddAvailablePassive(1, "whip", 5);
        AddAvailableSkill(1, "backstab", 5);
        AddAvailableSkill(1, "hide", 4);
        AddAvailableSkill(1, "recall", 2, 50);
        AddAvailableSkill(1, "scrolls", 5);
        AddAvailableSkill(1, "staves", 5);
        AddAvailableSkill(1, "trip", 4);
        AddAvailableSkill(1, "wands", 5);
        AddAvailableSpell(2, "magic missile", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(2, "ventriloquate", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(3, "dirt kicking", 4);
        AddAvailableSkill(4, "sneak", 4);
        AddAvailableSkill(5, "steal", 4);
        AddAvailableSpell(5, "detect magic", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(5, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(6, "lore", 4);
        AddAvailableSpell(6, "chill touch", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(6, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(6, "detect invis", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(7, "pick lock", 4);
        AddAvailableSpell(7, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "detect poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "invisibility", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(10, "envenom", 4);
        AddAvailableSpell(10, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "burning hands", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "locate object", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "sleep", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(12, "second attack", 5);
        AddAvailableSkill(12, "disarm", 6);
        AddAvailableSpell(12, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "detect evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "detect good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "detect hidden", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(13, "parry", 6);
        AddAvailableSkill(14, "kick", 6);
        AddAvailableSpell(14, "shocking grasp", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(15, "hand to hand", 6);
        AddAvailablePassive(15, "meditation", 8);
        AddAvailableSpell(15, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(16, "fast healing", 6);
        AddAvailableSpell(16, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "farsight", Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(18, "identify", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(18, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(19, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "know alignment", Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "colour spray", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "giant strength", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(23, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(24, "third attack", 10);
        AddAvailablePassive(25, "enhanced damage", 5);
        AddAvailableSpell(25, "charm person", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(25, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(25, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "haste", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(28, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(29, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(29, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "fireball", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(31, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(31, "mass invis", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(32, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(33, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(33, "heal", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(34, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(35, "acid blast", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(35, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(38, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(39, "chain lightning", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(42, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(43, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(45, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(47, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "fire breath", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 4);

        AddAvailableAbilityGroup("weaponsmaster", 40);
        AddAvailableAbilityGroup("beguiling", 6);
        AddAvailableAbilityGroup("combat", 10);
        AddAvailableAbilityGroup("creation", 8);
        AddAvailableAbilityGroup("detection", 6);
        AddAvailableAbilityGroup("enhancement", 9);
        AddAvailableAbilityGroup("illusion", 7);
        AddAvailableAbilityGroup("maladictions", 9);
        AddAvailableAbilityGroup("protective", 7);
        AddAvailableAbilityGroup("transportation", 8);
        AddAvailableAbilityGroup("weather", 8);
        AddBasicAbilityGroup("rom basics");
        AddBasicAbilityGroup("thief basics");
        AddDefaultAbilityGroup("thief default", 40);
    }

    #region IClass

    public override string Name => "thief";

    public override string ShortName => "Thi";

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Shapes shape)
        => ResourceKinds; // always mana

    public override BasicAttributes PrimeAttribute => BasicAttributes.Dexterity;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, -4);

    public override int MinHitPointGainPerLevel => 8;

    public override int MaxHitPointGainPerLevel => 13;

    #endregion

}
