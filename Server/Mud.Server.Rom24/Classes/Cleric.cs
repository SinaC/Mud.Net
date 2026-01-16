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
    public Cleric(ILogger<Cleric> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, abilityManager, abilityGroupManager)
    {
        AddAvailablePassive(1, "axe", 6);
        AddAvailablePassive(1, "dagger", 3);
        AddAvailablePassive(1, "flail", 3);
        AddAvailablePassive(1, "mace", 2);
        AddAvailablePassive(1, "polearm", 6);
        AddAvailablePassive(1, "shield block", 4);
        AddAvailablePassive(1, "spear", 4);
        AddAvailablePassive(1, "sword", 6);
        AddAvailablePassive(1, "whip", 5);
        AddAvailableSkill(1, "recall", 2, 50);
        AddAvailableSkill(1, "scrolls", 3);
        AddAvailableSkill(1, "staves", 3);
        AddAvailableSkill(1, "wands", 3);
        AddAvailableSpell(1, "cause light", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(1, "cure light", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(2, "armor", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(3, "create water", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(3, "faerie fire", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(4, "continual light", Mud.Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(4, "detect evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(4, "detect good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(5, "create food", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(5, "refresh", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(6, "meditation", 5);
        AddAvailableSpell(6, "cure blindness", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(6, "detect magic", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(7, "bless", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(7, "cause serious", Mud.Domain.ResourceKinds.Mana, 17, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(7, "cure serious", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(7, "detect poison", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(8, "blindness", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(8, "detect invis", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(9, "fast healing", 5);
        AddAvailableSpell(9, "know alignment", Mud.Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(9, "protection evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(9, "protection good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(10, "hand to hand", 5);
        AddAvailableSkill(10, "lore", 6);
        AddAvailableSpell(10, "earthquake", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(10, "floating disc", Mud.Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(11, "create rose", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(11, "detect hidden", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSkill(12, "kick", 4);
        AddAvailableSpell(12, "fireproof", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(12, "poison", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(12, "summon", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "cause critical", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "cure critical", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "cure disease", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(13, "infravision", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(14, "cure poison", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(14, "weaken", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(15, "dispel evil", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(15, "dispel good", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(15, "locate object", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "calm", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "farsight", Mud.Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "heat metal", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(16, "identify", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(17, "create spring", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(17, "gate", Mud.Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(17, "plague", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(18, "haggle", 8);
        AddAvailableSpell(18, "call lightning", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(18, "curse", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(18, "fly", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(18, "remove curse", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(19, "control weather", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(20, "parry", 8);
        AddAvailableSpell(20, "flamestrike", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(20, "sanctuary", Mud.Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(21, "peek", 7);
        AddAvailableSpell(21, "faerie fog", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(21, "heal", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(22, "dodge", 8);
        AddAvailableSpell(22, "energy drain", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(22, "teleport", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(23, "harm", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(23, "lightning bolt", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailablePassive(24, "second attack", 8);
        AddAvailableSpell(24, "dispel magic", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(24, "frenzy", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailableSkill(25, "pick lock", 8);
        AddAvailableSpell(25, "mass invis", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(26, "cancellation", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableAbility(28, "word of recall", [(Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed), (Mud.Domain.ResourceKinds.MovePoints, 50, CostAmountOperators.PercentageCurrent)], 1);
        AddAvailablePassive(30, "enhanced damage", 9);
        AddAvailableSpell(30, "portal", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "slow", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(32, "acid breath", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(32, "pass door", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(34, "demonfire", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(35, "nexus", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(35, "ray of truth", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(35, "shield", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(36, "frost breath", Mud.Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(36, "holy word", Mud.Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(38, "mass healing", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "lightning breath", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(40, "stone skin", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(43, "gas breath", Mud.Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 1);
        AddAvailableSpell(45, "fire breath", Mud.Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 1);

        AddAvailableAbilityGroup("weaponsmaster", 40);
        AddAvailableAbilityGroup("attack", 5);
        AddAvailableAbilityGroup("benedictions", 4);
        AddAvailableAbilityGroup("creation", 4);
        AddAvailableAbilityGroup("curative", 4);
        AddAvailableAbilityGroup("detection", 3);
        AddAvailableAbilityGroup("harmful", 4);
        AddAvailableAbilityGroup("healing", 3);
        AddAvailableAbilityGroup("maladictions", 5);
        AddAvailableAbilityGroup("protective", 4);
        AddAvailableAbilityGroup("transportation", 4);
        AddAvailableAbilityGroup("weather", 4);
        AddBasicAbilityGroup("rom basics");
        AddBasicAbilityGroup("cleric basics");
        AddDefaultAbilityGroup("cleric default", 40);
    }

    #region IClass

    public override string Name => "cleric";

    public override string ShortName => "Cle";

    public override bool SelectableDuringCreation => true;

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Mud.Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Shapes shape)
        => ResourceKinds; // always mana

    public override BasicAttributes PrimeAttribute => BasicAttributes.Wisdom;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, 2);

    public override int MinHitPointGainPerLevel => 7;

    public override int MaxHitPointGainPerLevel => 10;

    #endregion
}
