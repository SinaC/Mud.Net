using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Rom24.Classes;

[Help(
@"Warriors live for combat and the thrill of battle. They are the best fighters
of all the classes, but lack the subtle skills of thieves and the magical
talents of mages and priests.  Warriors are best for those who don't mind
taking the direct approach, even when another method might be called for.

Warriors begin with skill in the sword, and gain a second attack in combat.")]
[Export(typeof(IClass)), Shared]
public class Warrior : ClassBase
{
    public Warrior(ILogger<Warrior> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddPassive(1, "axe", 4);
        AddPassive(1, "dagger", 2);
        AddPassive(1, "enhanced damage", 3);
        AddPassive(1, "flail", 4);
        AddPassive(1, "mace", 3);
        AddPassive(1, "parry", 4);
        AddPassive(1, "polearm", 4);
        AddPassive(1, "shield block", 2);
        AddPassive(1, "spear", 3);
        AddPassive(1, "sword", 2);
        AddPassive(1, "whip", 4);
        AddSkill(1, "bash", 4);
        AddSkill(1, "recall", 2);
        AddSkill(1, "rescue", 4);
        AddSkill(1, "scrolls", 8);
        AddSkill(1, "staves", 8);
        AddSkill(1, "wands", 8);
        AddSpell(2, "magic missile", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSkill(3, "dirt kicking", 4);
        AddSpell(3, "cause light", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(3, "cure light", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddPassive(5, "second attack", 3);
        AddSpell(5, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddPassive(6, "fast healing", 4);
        AddPassive(6, "hand to hand", 4);
        AddSpell(6, "chill touch", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSkill(8, "kick", 3);
        AddSpell(8, "bless", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(8, "cure blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(8, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(9, "burning hands", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(9, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 2);
        AddSpell(9, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddSkill(10, "sneak", 6);
        AddSpell(10, "cause serious", Domain.ResourceKinds.Mana, 17, CostAmountOperators.Fixed, 2);
        AddSpell(10, "cure serious", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSkill(11, "disarm", 4);
        AddSpell(11, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(11, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(11, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddPassive(12, "third attack", 4);
        AddSkill(12, "hide", 6);
        AddSpell(12, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddPassive(13, "dodge", 6);
        AddSpell(13, "shocking grasp", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddPassive(14, "haggle", 6);
        AddPassive(14, "peek", 6);
        AddSpell(14, "cure disease", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(14, "earthquake", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddPassive(15, "meditation", 8);
        AddSkill(15, "trip", 8);
        AddSpell(15, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(16, "cure poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(16, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 2);
        AddSpell(16, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(16, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(17, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAbility(18, "berserk", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 5);
        AddSpell(18, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddSpell(19, "cause critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(19, "cure critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSkill(20, "lore", 8);
        AddSpell(20, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddSpell(20, "colour spray", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(20, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(20, "giant strength", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(21, "dispel evil", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(21, "dispel good", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(21, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddSpell(22, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(22, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddSpell(22, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(22, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddSpell(22, "remove curse", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(22, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddSpell(23, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddSpell(23, "heat metal", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddSpell(24, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddSpell(24, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddSkill(25, "pick lock", 8);
        AddSpell(26, "fireball", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(26, "frenzy", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddSpell(26, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(27, "flamestrike", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(28, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 2);
        AddSpell(28, "harm", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddSpell(29, "haste", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddSpell(30, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddSpell(30, "heal", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddSpell(30, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 2);
        AddSpell(30, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddSpell(32, "acid blast", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(32, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddSpell(34, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddSpell(34, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(36, "chain lightning", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddSpell(36, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddSpell(37, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(40, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 2);
        AddSpell(40, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddSpell(40, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddSpell(42, "holy word", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 4);
        AddSpell(45, "demonfire", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(45, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 4);
        AddSpell(45, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddSpell(46, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddSpell(46, "mass healing", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddSpell(47, "ray of truth", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddSpell(50, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 2);
    }

    #region IClass

    public override string Name => "warrior";

    public override string ShortName => "War";

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Domain.ResourceKinds.Mana
    ];

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
