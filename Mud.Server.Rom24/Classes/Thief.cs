using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
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
        AddAvailableSpell(2, "magic missile", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(2, "ventriloquate", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(3, "dirt kicking", 4);
        AddAvailableSkill(4, "sneak", 4);
        AddAvailableSkill(5, "steal", 4);
        AddAvailableSpell(5, "detect magic", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(5, "faerie fire", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(6, "lore", 4);
        AddAvailableSpell(6, "chill touch", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(6, "continual light", Mud.Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(6, "detect invis", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(7, "pick lock", 4);
        AddAvailableSpell(7, "floating disc", Mud.Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "detect poison", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "invisibility", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(10, "envenom", 4);
        AddAvailableSpell(10, "armor", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "burning hands", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "create rose", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "infravision", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "create food", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "locate object", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "sleep", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(12, "second attack", 5);
        AddAvailableSkill(12, "disarm", 6);
        AddAvailableSpell(12, "create water", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "detect evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "detect good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "detect hidden", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(12, "refresh", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(13, "parry", 6);
        AddAvailableSkill(14, "kick", 6);
        AddAvailableSpell(14, "shocking grasp", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(15, "hand to hand", 6);
        AddAvailablePassive(15, "meditation", 8);
        AddAvailableSpell(15, "poison", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(16, "fast healing", 6);
        AddAvailableSpell(16, "faerie fog", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "farsight", Mud.Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "weaken", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "blindness", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "protection evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "protection good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(18, "identify", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(18, "lightning bolt", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(19, "fireproof", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "fly", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "know alignment", Mud.Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "colour spray", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "giant strength", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(23, "create spring", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(24, "third attack", 10);
        AddAvailablePassive(25, "enhanced damage", 5);
        AddAvailableSpell(25, "charm person", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(25, "pass door", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(25, "teleport", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "curse", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "energy drain", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "haste", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(28, "control weather", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(29, "slow", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(29, "summon", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "dispel magic", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "fireball", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(31, "call lightning", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(31, "mass invis", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(32, "gate", Mud.Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(33, "acid breath", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(33, "heal", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(34, "cancellation", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(35, "acid blast", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(35, "shield", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "plague", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(38, "frost breath", Mud.Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(39, "chain lightning", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "stone skin", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "word of recall", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(42, "sanctuary", Mud.Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(43, "lightning breath", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(45, "portal", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(47, "gas breath", Mud.Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "calm", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "fire breath", Mud.Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "nexus", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 4);

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
        Mud.Domain.ResourceKinds.Mana
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
