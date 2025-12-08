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
        AddPassive(1, "axe", 6);
        AddPassive(1, "dagger", 3);
        AddPassive(1, "flail", 3);
        AddPassive(1, "mace", 2);
        AddPassive(1, "polearm", 6);
        AddPassive(1, "shield block", 4);
        AddPassive(1, "spear", 4);
        AddPassive(1, "sword", 6);
        AddPassive(1, "whip", 5);
        AddSkill(1, "recall", 2, 50);
        AddSkill(1, "scrolls", 3);
        AddSkill(1, "staves", 3);
        AddSkill(1, "wands", 3);
        AddSpell(1, "cause light", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(1, "cure light", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(2, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(3, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(3, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(4, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 1);
        AddSpell(4, "detect evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(4, "detect good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(5, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(5, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddPassive(6, "meditation", 5);
        AddSpell(6, "cure blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(6, "detect magic", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(7, "bless", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(7, "cause serious", Domain.ResourceKinds.Mana, 17, CostAmountOperators.Fixed, 1);
        AddSpell(7, "cure serious", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(7, "detect poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(8, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(8, "detect invis", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(9, "fast healing", 5);
        AddSpell(9, "know alignment", Domain.ResourceKinds.Mana, 9, CostAmountOperators.Fixed, 1);
        AddSpell(9, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(9, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(10, "hand to hand", 5);
        AddSkill(10, "lore", 6);
        AddSpell(10, "earthquake", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(10, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 1);
        AddSpell(11, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSpell(11, "detect hidden", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSkill(12, "kick", 4);
        AddSpell(12, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(12, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(12, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddSpell(13, "cause critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(13, "cure critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(13, "cure disease", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(13, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(14, "cure poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(14, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(15, "dispel evil", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(15, "dispel good", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(15, "locate object", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(16, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSpell(16, "farsight", Domain.ResourceKinds.Mana, 36, CostAmountOperators.Fixed, 1);
        AddSpell(16, "heat metal", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddSpell(16, "identify", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(17, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(17, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 1);
        AddSpell(17, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddPassive(18, "haggle", 8);
        AddSpell(18, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(18, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(18, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 1);
        AddSpell(18, "remove curse", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddSpell(19, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 1);
        AddPassive(20, "parry", 8);
        AddSpell(20, "flamestrike", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(20, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 1);
        AddPassive(21, "peek", 7);
        AddSpell(21, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(21, "heal", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 1);
        AddPassive(22, "dodge", 8);
        AddSpell(22, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddSpell(22, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddSpell(23, "harm", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 1);
        AddSpell(23, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddPassive(24, "second attack", 8);
        AddSpell(24, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 1);
        AddSpell(24, "frenzy", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSkill(25, "pick lock", 8);
        AddSpell(25, "mass invis", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(26, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(28, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 1);
        AddPassive(30, "enhanced damage", 9);
        AddSpell(30, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddSpell(30, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 1);
        AddSpell(32, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 1);
        AddSpell(32, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(34, "demonfire", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(35, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddSpell(35, "ray of truth", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 1);
        AddSpell(35, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(36, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 1);
        AddSpell(36, "holy word", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 2);
        AddSpell(38, "mass healing", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddSpell(40, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 1);
        AddSpell(40, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 1);
        AddSpell(43, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 1);
        AddSpell(45, "fire breath", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 1);

        AddAbilityGroup("weaponsmaster", 40);
        AddAbilityGroup("attack", 5);
        AddAbilityGroup("benedictions", 4);
        AddAbilityGroup("creation", 4);
        AddAbilityGroup("curative", 4);
        AddAbilityGroup("detection", 3);
        AddAbilityGroup("harmful", 4);
        AddAbilityGroup("healing", 3);
        AddAbilityGroup("maladictions", 5);
        AddAbilityGroup("protective", 4);
        AddAbilityGroup("transportation", 4);
        AddAbilityGroup("weather", 4);
        AddBasicAbilityGroup("rom basics");
        AddBasicAbilityGroup("cleric basics");
        AddDefaultAbilityGroup("cleric default", 40);
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
