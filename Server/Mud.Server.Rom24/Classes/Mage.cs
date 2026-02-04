using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Ability.Interfaces;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Class;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Rom24.Classes;

[Help(
@"Mages specialize in the casting of spells, offensive ones in particular.
Mages have the highest-powered magic of any class, and are the only classes
able to use the draconian and enchanting spell groups.  They are also very
skilled at the use of magical items, though their combat skills are the 
weakest of any class.

All mages begin with skill in the dagger. Any other weapon skills must be
purchased, at a very high rate.")]
[Export(typeof(IClass)), Shared]
public class Mage : ClassBase
{
    public Mage(ILogger<Mage> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, abilityManager, abilityGroupManager)
    {
        AddAvailablePassive(1, "axe", 6);
        AddAvailablePassive(1, "dagger", 2);
        AddAvailablePassive(1, "flail", 6);
        AddAvailablePassive(1, "mace", 5);
        AddAvailablePassive(1, "polearm", 6);
        AddAvailablePassive(1, "shield block", 6);
        AddAvailablePassive(1, "spear", 4);
        AddAvailablePassive(1, "sword", 5);
        AddAvailablePassive(1, "whip", 6);
        AddAvailableSkill(1, "recall", 2, 50);
        AddAvailableSkill(1, "scrolls", 2);
        AddAvailableSkill(1, "staves", 2);
        AddAvailableSkill(1, "wands", 2);
        AddAvailableSpell(1, "magic missile", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(1, "ventriloquate", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(2, "detect magic", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(3, "detect invis", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(4, "chill touch", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(4, "floating disc", Mud.Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(5, "invisibility", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(6, "meditation", 5);
        AddAvailableSpell(6, "continual light", Mud.Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(6, "faerie fire", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(7, "haggle", 5);
        AddAvailableSpell(7, "armor", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(7, "burning hands", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(8, "peek", 5);
        AddAvailableSpell(8, "create water", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(8, "refresh", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(9, "infravision", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(9, "locate object", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(9, "recharge", Mud.Domain.ResourceKinds.Mana, 60, CostAmountOperators.Fixed, 1);
        AddAvailableSkill(10, "lore", 6);
        AddAvailableSpell(10, "create food", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(10, "fly", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(10, "shocking grasp", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(10, "sleep", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(11, "detect evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(11, "detect good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(11, "giant strength", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(11, "weaken", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(12, "blindness", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(12, "know alignment", Mud.Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(12, "protection evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(12, "protection good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "fireproof", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "lightning bolt", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "teleport", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(14, "create spring", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(14, "faerie fog", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(14, "farsight", Mud.Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(15, "fast healing", 8);
        AddAvailableSpell(15, "control weather", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(15, "detect hidden", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(15, "detect poison", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(15, "identify", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "colour spray", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "create rose", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "dispel magic", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "enchant armor", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "enchant weapon", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "poison", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(18, "cancellation", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(18, "curse", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(19, "energy drain", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(20, "dodge", 8);
        AddAvailableSpell(20, "charm person", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(20, "shield", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(21, "haste", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(22, "parry", 8);
        AddAvailableSpell(22, "fireball", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(22, "mass invis", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(23, "plague", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(23, "slow", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(24, "pass door", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(24, "summon", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(25, "hand to hand", 8);
        AddAvailableSkill(25, "pick lock", 8);
        AddAvailableSpell(25, "stone skin", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(26, "call lightning", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(27, "gate", Mud.Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(28, "acid blast", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(30, "second attack", 10);
        AddAvailableSpell(31, "acid breath", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 1);
        AddAvailableAbility(32, "word of recall", [(Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed), (Mud.Domain.ResourceKinds.MovePoints, 50, CostAmountOperators.PercentageCurrent)], 1);
        AddAvailableSpell(33, "chain lightning", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(34, "frost breath", Mud.Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(35, "portal", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "sanctuary", Mud.Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(37, "lightning breath", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(39, "gas breath", Mud.Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(40, "fire breath", Mud.Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(40, "nexus", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(45, "enhanced damage", 10);
        AddAvailableSpell(48, "calm", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);

        AddAvailableAbilityGroup("weaponsmaster", 40);
        AddAvailableAbilityGroup("beguiling", 4);
        AddAvailableAbilityGroup("combat", 6);
        AddAvailableAbilityGroup("creation", 4);
        AddAvailableAbilityGroup("detection", 3);
        AddAvailableAbilityGroup("draconian", 8);
        AddAvailableAbilityGroup("enchantment", 6);
        AddAvailableAbilityGroup("enhancement", 5);
        AddAvailableAbilityGroup("illusion", 4);
        AddAvailableAbilityGroup("maladictions", 5);
        AddAvailableAbilityGroup("protective", 4);
        AddAvailableAbilityGroup("transportation", 4);
        AddAvailableAbilityGroup("weather", 4);
        AddBasicAbilityGroup("rom basics");
        AddBasicAbilityGroup("mage basics");
        AddDefaultAbilityGroup("mage default", 40);
    }

    #region IClass

    public override string Name => "mage";

    public override string ShortName => "Mag";

    public override bool SelectableDuringCreation => true;

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Mud.Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Shapes shape)
        => ResourceKinds; // always mana

    public override BasicAttributes PrimeAttribute => BasicAttributes.Intelligence;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, 6);

    public override bool IncreasedManaGainWhenLeveling => true;

    public override int MinHitPointGainPerLevel => 6;

    public override int MaxHitPointGainPerLevel => 8;

    #endregion
}
