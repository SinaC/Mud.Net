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
        AddAvailableSpell(2, "magic missile", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(3, "dirt kicking", 4);
        AddAvailableSpell(3, "cause light", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(3, "cure light", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(5, "second attack", 3);
        AddAvailableSpell(5, "armor", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(6, "fast healing", 4);
        AddAvailablePassive(6, "hand to hand", 4);
        AddAvailableSpell(6, "chill touch", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(8, "kick", 3);
        AddAvailableSpell(8, "bless", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(8, "cure blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(8, "faerie fire", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "burning hands", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "continual light", Domain.ResourceKinds.Mana, 7, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(9, "refresh", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(10, "sneak", 6);
        AddAvailableSpell(10, "cause serious", Domain.ResourceKinds.Mana, 17, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(10, "cure serious", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(11, "disarm", 4);
        AddAvailableSpell(11, "create water", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "protection evil", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(11, "protection good", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(12, "third attack", 4);
        AddAvailableSkill(12, "hide", 6);
        AddAvailableSpell(12, "create food", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(13, "dodge", 6);
        AddAvailableSpell(13, "shocking grasp", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(14, "haggle", 6);
        AddAvailablePassive(14, "peek", 6);
        AddAvailableSpell(14, "cure disease", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(14, "earthquake", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailablePassive(15, "meditation", 8);
        AddAvailableSkill(15, "trip", 8);
        AddAvailableSpell(15, "blindness", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "cure poison", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "floating disc", Domain.ResourceKinds.Mana, 40, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "infravision", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(16, "lightning bolt", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(17, "weaken", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableAbility(18, "berserk", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 5);
        AddAvailableSpell(18, "fireproof", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(19, "cause critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(19, "cure critical", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(20, "lore", 8);
        AddAvailableSpell(20, "calm", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "colour spray", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "create spring", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(20, "giant strength", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(21, "dispel evil", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(21, "dispel good", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(21, "poison", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "call lightning", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "control weather", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "curse", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "fly", Domain.ResourceKinds.Mana, 10, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "remove curse", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(22, "summon", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(23, "energy drain", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(23, "heat metal", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(24, "create rose", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(24, "faerie fog", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSkill(25, "pick lock", 8);
        AddAvailableSpell(26, "fireball", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "frenzy", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(26, "plague", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(27, "flamestrike", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(28, "gate", Domain.ResourceKinds.Mana, 80, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(28, "harm", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(29, "haste", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "dispel magic", Domain.ResourceKinds.Mana, 15, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "heal", Domain.ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "sanctuary", Domain.ResourceKinds.Mana, 75, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(30, "word of recall", Domain.ResourceKinds.Mana, 5, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(32, "acid blast", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(32, "slow", Domain.ResourceKinds.Mana, 30, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(34, "acid breath", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(34, "cancellation", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "chain lightning", Domain.ResourceKinds.Mana, 25, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(36, "teleport", Domain.ResourceKinds.Mana, 35, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(37, "pass door", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "frost breath", Domain.ResourceKinds.Mana, 125, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(40, "portal", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(40, "shield", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(42, "holy word", Domain.ResourceKinds.Mana, 200, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(45, "demonfire", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(45, "nexus", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(45, "stone skin", Domain.ResourceKinds.Mana, 12, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(46, "lightning breath", Domain.ResourceKinds.Mana, 150, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(46, "mass healing", Domain.ResourceKinds.Mana, 100, CostAmountOperators.Fixed, 4);
        AddAvailableSpell(47, "ray of truth", Domain.ResourceKinds.Mana, 20, CostAmountOperators.Fixed, 2);
        AddAvailableSpell(50, "gas breath", Domain.ResourceKinds.Mana, 175, CostAmountOperators.Fixed, 2);

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

    public override IEnumerable<ResourceKinds> ResourceKinds { get; } =
    [
        Domain.ResourceKinds.Mana
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
