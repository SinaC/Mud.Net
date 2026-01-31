using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("backstab", "Ability", "Skill", "Combat")]
[Syntax("[cmd] <victim>")]
[Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 24)]
[Help(
@"Backstab is the favored attack of thieves, murderers, and other rogues.  It
can be used with any weapon type, but is most effective with piercing
weapons such as daggers.  The damage inflicted by a backstab is determined by
the attacker's level, his weapon skill, his backstab skill, and the power of
his opponent.  You must be not visible to the mob (i.e. sneaking or hiding)
in order to backstab.")]
[OneLineHelp("the art of hitting your opponent by surprise")]
public class Backstab : OffensiveSkillBase
{
    private const string SkillName = "Backstab";

    protected override IGuard<ICharacter>[] Guards => [new CannotBeInCombat { Message = "You are facing the wrong end." }];

    public Backstab(ILogger<Backstab> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        if (Victim == User)
            return "How can you sneak up on yourself?";

        var safeResult = Victim.IsSafe(User);
        if (safeResult != null)
            return safeResult;

        // TODO: check kill stealing

        if (User.GetEquipment(EquipmentSlots.MainHand) is not IItemWeapon)
            return "You need to wield a weapon to backstab.";

        if (Victim[ResourceKinds.HitPoints] < Victim.MaxResource(ResourceKinds.HitPoints) / 3)
            return User.ActPhrase("{0} is hurt and suspicious ... you can't sneak up.", Victim);

        return null;
    }

    protected override bool Invoke()
    {
        // TODO: check killer
        if (RandomManager.Chance(Learned)
            || (Learned > 1 && Victim.Position <= Positions.Sleeping))
        {
            BackstabMultiHitModifier modifier = new (SkillName, "backstab", Learned);

            User.MultiHit(Victim, modifier);
            return true;
        }
        else
        {
            //Victim.AbilityDamage(User, 0, SchoolTypes.None, "backstab", true); // Starts fight without doing any damage
            return false;
        }
    }

    private class BackstabMultiHitModifier : IMultiHitModifier
    {
        public BackstabMultiHitModifier(string abilityName, string damageNoun, int learned)
        {
            AbilityName = abilityName;
            DamageNoun = damageNoun;
            Learned = learned;
        }

        #region IMultiHitModifier

        #region IHitModifier

        public string AbilityName { get; }

        public string DamageNoun { get; }

        public int Learned { get; }

        public int DamageModifier(IItemWeapon? weapon, int level, int baseDamage)
        {
            if (weapon != null)
            {
                if (weapon.Type != WeaponTypes.Dagger)
                    return baseDamage * (3 * level / 24);
                else
                    return baseDamage * (3 * level / 12);
            }
            return baseDamage;
        }

        public int Thac0Modifier(int baseThac0)
        {
            return baseThac0 - 8 * (100 - Learned);
        }

        #endregion

        public int MaxAttackCount => 2;

        #endregion
    }
}
