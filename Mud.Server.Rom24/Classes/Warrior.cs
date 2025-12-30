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
@"Warriors live for combat and the thrill of battle. They are the best fighters
of all the classes, but lack the subtle skills of thieves and the magical
talents of mages and priests.  Warriors are best for those who don't mind
taking the direct approach, even when another method might be called for.

Warriors begin with skill in the sword, and gain a second attack in combat.")]
[Export(typeof(IClass)), Shared]
public class Warrior : ClassBase
{
    public Warrior(ILogger<Warrior> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, abilityManager, abilityGroupManager)
    {
        AddAvailablePassive(1, "axe", 4);
        AddAvailablePassive(1, "dagger", 2);
        AddAvailablePassive(1, "enhanced damage", 3);
        AddAvailablePassive(1, "flail", 4);
        AddAvailablePassive(1, "mace", 3);
        AddAvailablePassive(1, "parry", 4);
        AddAvailablePassive(1, "polearm", 4);
        AddAvailablePassive(1, "shield block", 2);
        AddAvailablePassive(1, "spear", 3);
        AddAvailablePassive(1, "sword", 2);
        AddAvailablePassive(1, "whip", 4);
        AddAvailableSkill(1, "bash", 4);
        AddAvailableSkill(1, "recall", 2, 50);
        AddAvailableSkill(1, "rescue", 4);
        AddAvailableSkill(1, "scrolls", 8);
        AddAvailableSkill(1, "staves", 8);
        AddAvailableSkill(1, "wands", 8);
        AddAvailableSpell(2, "magic missile", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(3, "dirt kicking", 4);
        AddAvailableSpell(3, "cause light", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(3, "cure light", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(5, "second attack", 3);
        AddAvailableSpell(5, "armor", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(6, "fast healing", 4);
        AddAvailablePassive(6, "hand to hand", 4);
        AddAvailableSpell(6, "chill touch", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(8, "kick", 3);
        AddAvailableSpell(8, "bless", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(8, "cure blindness", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(8, "faerie fire", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "burning hands", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "continual light", Mud.Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "refresh", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(10, "sneak", 6);
        AddAvailableSpell(10, "cause serious", Mud.Domain.ResourceKinds.Mana, 17, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "cure serious", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(11, "disarm", 4);
        AddAvailableSpell(11, "create water", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "protection evil", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "protection good", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(12, "third attack", 4);
        AddAvailableSkill(12, "hide", 6);
        AddAvailableSpell(12, "create food", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(13, "dodge", 6);
        AddAvailableSpell(13, "shocking grasp", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(14, "haggle", 6);
        AddAvailablePassive(14, "peek", 6);
        AddAvailableSpell(14, "cure disease", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(14, "earthquake", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(15, "meditation", 8);
        AddAvailableSkill(15, "trip", 8);
        AddAvailableSpell(15, "blindness", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "cure poison", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "floating disc", Mud.Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "infravision", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "lightning bolt", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "weaken", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableAbility(18, "berserk", [(Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed), (Mud.Domain.ResourceKinds.MovePoints, 50, CostAmountOperators.PercentageCurrent)], 5);
        AddAvailableSpell(18, "fireproof", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(19, "cause critical", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(19, "cure critical", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(20, "lore", 8);
        AddAvailableSpell(20, "calm", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "colour spray", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "create spring", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "giant strength", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(21, "dispel evil", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(21, "dispel good", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(21, "poison", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "call lightning", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "control weather", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "curse", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "fly", Mud.Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "remove curse", Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "summon", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(23, "energy drain", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(23, "heat metal", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(24, "create rose", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(24, "faerie fog", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(25, "pick lock", 8);
        AddAvailableSpell(26, "fireball", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "frenzy", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "plague", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(27, "flamestrike", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(28, "gate", Mud.Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(28, "harm", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(29, "haste", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "dispel magic", Mud.Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "heal", Mud.Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "sanctuary", Mud.Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 2);
        AddAvailableAbility(30, "word of recall", [(Mud.Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed), (Mud.Domain.ResourceKinds.MovePoints, 50, CostAmountOperators.PercentageCurrent)], 1);
        AddAvailableSpell(32, "acid blast", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(32, "slow", Mud.Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(34, "acid breath", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(34, "cancellation", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "chain lightning", Mud.Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "teleport", Mud.Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(37, "pass door", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "frost breath", Mud.Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "portal", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(40, "shield", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(42, "holy word", Mud.Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(45, "demonfire", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(45, "nexus", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(45, "stone skin", Mud.Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(46, "lightning breath", Mud.Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(46, "mass healing", Mud.Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(47, "ray of truth", Mud.Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "gas breath", Mud.Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 2);

        AddAvailableAbilityGroup("weaponsmaster", 20);
        AddAvailableAbilityGroup("attack", 8);
        AddAvailableAbilityGroup("benedictions", 4);
        AddAvailableAbilityGroup("combat", 9);
        AddAvailableAbilityGroup("creation", 8);
        AddAvailableAbilityGroup("curative", 8);
        AddAvailableAbilityGroup("enhancement", 9);
        AddAvailableAbilityGroup("harmful", 6);
        AddAvailableAbilityGroup("healing", 6);
        AddAvailableAbilityGroup("maladictions", 9);
        AddAvailableAbilityGroup("protective", 8);
        AddAvailableAbilityGroup("transportation", 9);
        AddAvailableAbilityGroup("weather", 8);
        AddBasicAbilityGroup("rom basics");
        AddBasicAbilityGroup("warrior basics");
        AddDefaultAbilityGroup("warrior default", 40);
    }

    #region IClass

    public override string Name => "warrior";

    public override string ShortName => "War";

    public override bool SelectableDuringCreation => true;

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Mud.Domain.ResourceKinds.Mana
    ];

    public override IEnumerable<ResourceKinds> CurrentResourceKinds(Shapes shape)
        => ResourceKinds; // always mana

    public override BasicAttributes PrimeAttribute => BasicAttributes.Strength;

    public override int MaxPracticePercentage => 75;

    public override (int thac0_00, int thac0_32) Thac0 => (20, -10);

    public override int MinHitPointGainPerLevel => 11;

    public override int MaxHitPointGainPerLevel => 15;

    #endregion
}
