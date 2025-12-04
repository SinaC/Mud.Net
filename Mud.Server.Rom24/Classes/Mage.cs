using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

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
    public Mage(ILogger<Mage> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddPassive(1, "axe", 6);
        AddPassive(1, "dagger", 2);
        AddPassive(1, "flail", 6);
        AddPassive(1, "mace", 5);
        AddPassive(1, "polearm", 6);
        AddPassive(1, "shield block", 6);
        AddPassive(1, "spear", 4);
        AddPassive(1, "sword", 5);
        AddPassive(1, "whip", 6);
        AddSkill(1, "recall", 2);
        AddSkill(1, "scrolls", 2);
        AddSkill(1, "staves", 2);
        AddSkill(1, "wands", 2);
        AddSpell(1, "magic missile", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(1, "ventriloquate", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(2, "detect magic", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(3, "detect invis", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(4, "chill touch", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(4, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 1);
        AddSpell(5, "invisibility", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(6, "meditation", 5);
        AddSpell(6, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 1);
        AddSpell(6, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(7, "haggle", 5);
        AddSpell(7, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(7, "burning hands", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddPassive(8, "peek", 5);
        AddSpell(8, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(8, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(9, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(9, "locate object", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(9, "recharge", Domain.ResourceKinds.Mana, 60, CostAmountOperators.Fixed, 1);
        AddSkill(10, "lore", 6);
        AddSpell(10, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(10, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(10, "shocking grasp", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(10, "sleep", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(11, "detect evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(11, "detect good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(11, "giant strength", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(11, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(12, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(12, "know alignment", Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 1);
        AddSpell(12, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(12, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(13, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(13, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(13, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddSpell(14, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(14, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(14, "farsight", Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 1);
        AddPassive(15, "fast healing", 8);
        AddSpell(15, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddSpell(15, "detect hidden", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(15, "detect poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(15, "identify", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(16, "colour spray", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(16, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSpell(16, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(16, "enchant armor", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddSpell(17, "enchant weapon", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddSpell(17, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(18, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(18, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(19, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddPassive(20, "dodge", 8);
        AddSpell(20, "charm person", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(20, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(21, "haste", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddPassive(22, "parry", 8);
        AddSpell(22, "fireball", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(22, "mass invis", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(23, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(23, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSpell(24, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(24, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddPassive(25, "hand to hand", 8);
        AddSkill(25, "pick lock", 8);
        AddSpell(25, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(26, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(27, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 1);
        AddSpell(28, "acid blast", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddPassive(30, "second attack", 10);
        AddSpell(31, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 1);
        AddSpell(32, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(33, "chain lightning", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddSpell(34, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 1);
        AddSpell(35, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddSpell(36, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 1);
        AddSpell(37, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 1);
        AddSpell(39, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 1);
        AddSpell(40, "fire breath", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 1);
        AddSpell(40, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddPassive(45, "enhanced damage", 10);
        AddSpell(48, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
    }

    #region IClass

    public override string Name => "mage";

    public override string ShortName => "Mag";

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
    {
        return ResourceKinds; // always mana
    }

    public override BasicAttributes PrimeAttribute => BasicAttributes.Intelligence;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, 6);

    public override int MinHitPointGainPerLevel => 6;

    public override int MaxHitPointGainPerLevel => 8;

    #endregion
}
