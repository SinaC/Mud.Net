﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("backstab", "Abilities", "Skills", "Combat")]
    [Skill(SkillName, AbilityEffects.Damage, PulseWaitTime = 24)]
    public class Backstab : OffensiveSkillBase
    {
        public const string SkillName = "Backstab";

        public Backstab(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override string Setup(SkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (User.Fighting != null)
                return "You are facing the wrong end.";

            if (Victim == User)
                return "How can you sneak up on yourself?";

            if (Victim.IsSafe(User))
                return "Not on that victim.";

            // TODO: check kill stealing

            if (!(User.GetEquipment(EquipmentSlots.MainHand) is IItemWeapon))
                return "You need to wield a weapon to backstab.";

            if (Victim.HitPoints < Victim.MaxHitPoints / 3)
                return User.ActPhrase("{0} is hurt and suspicious ... you can't sneak up.", Victim);

            return null;
        }

        protected override bool Invoke()
        {
            // TODO: check killer
            if (RandomManager.Chance(Learned)
                || (Learned > 1 && Victim.Position <= Positions.Sleeping))
            {
                BackstabMultitHitModifier modifier = new BackstabMultitHitModifier(this, Learned);

                User.MultiHit(Victim, modifier);
                return true;
            }
            else
            {
                Victim.AbilityDamage(User, 0, SchoolTypes.None, "backstab", true); // Starts fight without doing any damage
                return false;
            }
        }

        public class BackstabMultitHitModifier : IMultiHitModifier
        {
            public BackstabMultitHitModifier(IAbility ability, int learned)
            {
                Ability = ability;
                Learned = learned;
            }

            #region IMultiHitModifier

            #region IHitModifier

            public int MaxAttackCount => 2;

            #endregion

            public IAbility Ability { get; }

            public int Learned { get; }

            public int DamageModifier(IItemWeapon weapon, int level, int baseDamage)
            {
                if (weapon != null)
                {
                    if (weapon?.Type != WeaponTypes.Dagger)
                        return baseDamage * (2 + level / 10);
                    else
                        return baseDamage * (2 + level / 8);
                }
                return baseDamage;
            }

            public int Thac0Modifier(int baseThac0)
            {
                return baseThac0 - 10 * (100 - Learned);
            }

            #endregion
        }
    }
}
