using System;
using Mud.Logger;
using Mud.Server.Constants;

namespace Mud.Server.Helpers
{
    public static class CombatHelpers
    {
        public enum AttackResults
        {
            Miss,
            Dodge,
            Parry,
            GlancingBlow,
            Block,
            Critical,
            CrushingBlow,
            Hit,
        }

        // TODO: check block only if shield is equiped

        // White melee: auto-attack
        public static AttackResults WhiteMeleeAttack(ICharacter attacker, ICharacter victim, bool dualWield)
        {
            //http://wow.gamepedia.com/Attack_table#Player_melee_and_ranged_attacks
            if (attacker.ImpersonatedBy != null)
                return MeleeAttackFromImpersonated(attacker, victim, dualWield, true, false, false);
            else
                return MeleeAttackFromNotImpersonated(attacker, victim, dualWield, false, false);

            //// TODO: if victim can't see this, chance are divided by 2

            //// check miss
            //if (miss > 0 && RandomizeHelpers.Instance.Randomizer.Next(1000) <= miss)
            //{
            //    victim.Act(ActOptions.ToCharacter, "{0} misses you.", this);
            //    Act(ActOptions.ToCharacter, "You miss {0}.", victim);
            //    return false;
            //}
            //// check dodge
            //if (dodge > 0 && RandomizeHelpers.Instance.Randomizer.Next(1000) <= dodge)
            //{
            //    victim.Act(ActOptions.ToCharacter, "You dodge {0}'s attack.", this);
            //    Act(ActOptions.ToCharacter, "{0} dodges your attack.", victim);
            //    return false;

            //}
            //// check parry
            //bool victimHasWeapon = victim.Equipments.Any(x => x.Item != null && (x.Slot == EquipmentSlots.Wield || x.Slot == EquipmentSlots.Wield2 || x.Slot == EquipmentSlots.Wield2H));
            //parry = !victimHasWeapon ? parry / 2 : parry; // no weapon -> half chance
            //if (parry > 0 && RandomizeHelpers.Instance.Randomizer.Next(1000) <= parry)
            //{
            //    victim.Act(ActOptions.ToCharacter, "You parry {0}'s attack.", this);
            //    Act(ActOptions.ToCharacter, "{0} parries your attack.", victim);
            //    return false;
            //}
            //// check glance
            //// TODO: http://wow.gamepedia.com/Glancing_Blow
            //// check block
            //EquipedItem victimShield = victim.Equipments.FirstOrDefault(x => x.Item != null && x.Slot == EquipmentSlots.Shield);
            //if (victimShield != null && block > 0 && RandomizeHelpers.Instance.Randomizer.Next(1000) <= block)
            //{
            //    victim.Act(ActOptions.ToCharacter, "You block {0}'s attack with {1}.", this, victimShield.Item);
            //    Act(ActOptions.ToCharacter, "{0} blocks your attack with {1}.", victim, victimShield.Item);
            //    return false;
            //}
        }

        // Yellow melee: melee ability
        public static AttackResults YellowMeleeAttack(ICharacter attacker, ICharacter victim, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
        {
            //http://wow.gamepedia.com/Attack_table#Player_melee_and_ranged_attacks
            //http://wow.gamepedia.com/Glancing_blow#Glancing_blow_implications_-_crit_cap
            //http://wow.gamepedia.com/Hit#Special_attacks
            // according to --^ yellow damage cannot land glancing blow neither critical strike + miss chance is reduced to 8% (dual wield or not)
            if (attacker.ImpersonatedBy != null)
                return MeleeAttackFromImpersonated(attacker, victim, false, false, cannotMiss, cannotBeDodgedParriedBlocked);
            else
                return MeleeAttackFromNotImpersonated(attacker, victim, false, cannotMiss, cannotBeDodgedParriedBlocked);
        }

        public static AttackResults SpellAttack(ICharacter attacker, ICharacter victim, bool cannotMiss)
        {
            if (attacker.ImpersonatedBy != null)
                return SpellAttackFromImpersonated(attacker, victim);
            else
                return SpellAttackFromNotImpersonated(attacker, victim);
        }

        // PC attacking a NPC/PC
        private static AttackResults MeleeAttackFromImpersonated(ICharacter attacker, ICharacter victim, bool dualWield, bool allowGlancingBlow /*yellow melee don't do glancing blow*/, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
        {
            int deltaLevel = attacker.Level - victim.Level;
            // following values must be / 10 to get %age
            int miss;
            int bonusDodge;
            int bonusParry;
            int glance;
            int bonusBlock;
            int malusCritical;
            if (deltaLevel <= -10)
            {
                miss = 105;
                bonusDodge = 105;
                bonusParry = 135;
                glance = 1100;
                bonusBlock = 180;
                malusCritical = 100;
            }
            else if (deltaLevel == -9)
            {
                miss = 90;
                bonusDodge = 90;
                bonusParry = 120;
                glance = 1000;
                bonusBlock = 165;
                malusCritical = 90;
            }
            else if (deltaLevel == -8)
            {
                miss = 75;
                bonusDodge = 75;
                bonusParry = 105;
                glance = 800;
                bonusBlock = 150;
                malusCritical = 80;
            }
            else if (deltaLevel == -7)
            {
                miss = 60;
                bonusDodge = 60;
                bonusParry = 90;
                glance = 800;
                bonusBlock = 135;
                malusCritical = 70;
            }
            else if (deltaLevel == -6)
            {
                miss = 45;
                bonusDodge = 45;
                bonusParry = 75;
                glance = 700;
                bonusBlock = 120;
                malusCritical = 60;
            }
            else if (deltaLevel == -5)
            {
                miss = 30;
                bonusDodge = 30;
                bonusParry = 60;
                glance = 600;
                bonusBlock = 105;
                malusCritical = 50;
            }
            else if (deltaLevel == -4)
            {
                miss = 15;
                bonusDodge = 15;
                bonusParry = 45;
                glance = 500;
                bonusBlock = 90;
                malusCritical = 40;
            }
            else if (deltaLevel == -3)
            {
                miss = 0;
                bonusDodge = 0;
                bonusParry = 3;
                glance = 0;
                bonusBlock = 75;
                malusCritical = 30;
            }
            else if (deltaLevel == -2)
            {
                miss = 0;
                bonusDodge = 0;
                bonusParry = 0;
                glance = 0;
                bonusBlock = 60;
                malusCritical = 20;
            }
            else if (deltaLevel == -1)
            {
                miss = 0;
                bonusDodge = 0;
                bonusParry = 0;
                glance = 0;
                bonusBlock = 45;
                malusCritical = 10;
            }
            else if (deltaLevel == 0)
            {
                miss = 0;
                bonusDodge = 0;
                bonusParry = 0;
                glance = 0;
                bonusBlock = 30;
                malusCritical = 0;
            }
            else if (deltaLevel == +1)
            {
                miss = 0;
                bonusDodge = 0;
                bonusParry = 0;
                glance = 0;
                bonusBlock = 15;
                malusCritical = 0;
            }
            else
            {
                miss = 0;
                bonusDodge = 0;
                bonusParry = 0;
                glance = 0;
                bonusBlock = 0;
                malusCritical = 0;
            }
            //
            if (dualWield)
                miss += 190;

            //
            int dodge = Math.Max(0, victim[SecondaryAttributeTypes.Dodge]*10 + bonusDodge); // dodge is in %
            int parry = Math.Max(0, victim[SecondaryAttributeTypes.Parry]*10 + bonusParry); // parry is in %
            int block = Math.Max(0, victim[SecondaryAttributeTypes.Block]*10 + bonusBlock); // block is in %
            int critical = Math.Max(0, attacker[SecondaryAttributeTypes.Critical] - malusCritical);

            // check flags
            if (cannotMiss)
                miss = 0;
            if (cannotBeDodgedParriedBlocked)
            {
                dodge = 0;
                parry = 0;
                block = 0;
            }

            //
            int roll = RandomizeHelpers.Instance.Randomizer.Next(1000);
            Log.Default.WriteLine(LogLevels.Debug, $"MeleeAttackFromImpersonated: Roll: {roll} Miss: {miss} Dodge: {dodge}/{bonusDodge} Parry: {parry}/{bonusParry} Glance: {glance}[{allowGlancingBlow}] Block: {block}/{bonusBlock} Crit: {critical}/{malusCritical}");
            int cumulativeSum = 0;
            //
            if (roll < cumulativeSum + miss)
                return AttackResults.Miss;
            cumulativeSum += miss;
            //
            if (roll <= cumulativeSum + dodge)
                return AttackResults.Dodge;
            cumulativeSum += dodge;
            //
            if (roll <= cumulativeSum + parry)
                return AttackResults.Parry;
            cumulativeSum += parry;
            //
            if (allowGlancingBlow)
            {
                if (roll <= cumulativeSum + glance)
                    return AttackResults.GlancingBlow;
                cumulativeSum += glance;
            }
            //
            if (roll <= cumulativeSum + block)
                return AttackResults.Block;
            cumulativeSum += block;
            //
            if (roll <= cumulativeSum + critical)
                return AttackResults.Critical;
            //cumulativeSum += critical;
            return AttackResults.Hit;
        }

        private static AttackResults SpellAttackFromImpersonated(ICharacter attacker, ICharacter victim)
        {
            int deltaLevel = attacker.Level - victim.Level;
            // miss chance
            // -10 -> 77
            // -7 -> 44
            // -3 -> 0
            int missChance = (Math.Min(0, Math.Max(-10, deltaLevel)) - 3)*11;
            if (RandomizeHelpers.Instance.Randomizer.Next(100) <= missChance)
                return AttackResults.Miss;
            // crit chance
            int deltaCritical = Math.Min(0, Math.Max(-10, deltaLevel));
            int critical = Math.Max(0, attacker[SecondaryAttributeTypes.Critical] - deltaCritical);
            if (RandomizeHelpers.Instance.Randomizer.Next(100) <= critical)
                return AttackResults.Critical;
            return AttackResults.Hit;
        }

        // NPC attacking a PC
        private static AttackResults MeleeAttackFromNotImpersonated(ICharacter attacker, ICharacter victim, bool dualWield, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
        {
            int deltaLevel = attacker.Level - victim.Level;
            // following values must be / 10 to get %age
            int miss;
            int bonusDodge;
            int bonusParry;
            int bonusBlock;
            int crushing;
            if (deltaLevel <= -5)
            {
                miss = 135;
                bonusDodge = 75;
                bonusParry = 75;
                bonusBlock = 75;
                crushing = 0;
            }
            else if (deltaLevel == -4)
            {
                miss = 105;
                bonusDodge = 60;
                bonusParry = 60;
                bonusBlock = 60;
                crushing = 0;
            }
            else if (deltaLevel == -3)
            {
                miss = 75;
                bonusDodge = 45;
                bonusParry = 45;
                bonusBlock = 45;
                crushing = 0;
            }
            else if (deltaLevel == -2)
            {
                miss = 60;
                bonusDodge = 30;
                bonusParry = 30;
                bonusBlock = 30;
                crushing = 0;
            }
            else if (deltaLevel == -1)
            {
                miss = 45;
                bonusDodge = 15;
                bonusParry = 15;
                bonusBlock = 15;
                crushing = 0;
            }
            else if (deltaLevel == 0)
            {
                miss = 30;
                bonusDodge = 0;
                bonusParry = 0;
                bonusBlock = 0;
                crushing = 0;
            }
            else if (deltaLevel == 1)
            {
                miss = 15;
                bonusDodge = -15;
                bonusParry = -15;
                bonusBlock = -15;
                crushing = 0;
            }
            else if (deltaLevel == 2)
            {
                miss = 0;
                bonusDodge = -30;
                bonusParry = -30;
                bonusBlock = -30;
                crushing = 0;
            }
            else if (deltaLevel == 3)
            {
                miss = 0;
                bonusDodge = -45;
                bonusParry = -45;
                bonusBlock = -45;
                crushing = 0;
            }
            else if (deltaLevel == 4)
            {
                miss = 0;
                bonusDodge = -60;
                bonusParry = -60;
                bonusBlock = -60;
                crushing = 250;
            }
            else if (deltaLevel == 5)
            {
                miss = 0;
                bonusDodge = -75;
                bonusParry = -75;
                bonusBlock = -75;
                crushing = 350;
            }
            else if (deltaLevel == 6)
            {
                miss = 0;
                bonusDodge = -90;
                bonusParry = -90;
                bonusBlock = -90;
                crushing = 450;
            }
            else if (deltaLevel == 7)
            {
                miss = 0;
                bonusDodge = -105;
                bonusParry = -105;
                bonusBlock = -105;
                crushing = 550;
            }
            else if (deltaLevel == 8)
            {
                miss = 0;
                bonusDodge = -120;
                bonusParry = -120;
                bonusBlock = -120;
                crushing = 650;
            }
            else if (deltaLevel == 9)
            {
                miss = 0;
                bonusDodge = -135;
                bonusParry = -135;
                bonusBlock = -135;
                crushing = 750;
            }
            else // 10 and above
            {
                miss = 0;
                bonusDodge = -150;
                bonusParry = -150;
                bonusBlock = -150;
                crushing = 850;
            }
            if (dualWield)
                miss += 190;

            //
            int dodge = Math.Max(0, victim[SecondaryAttributeTypes.Dodge]*10 + bonusDodge); // dodge is in %
            int parry = Math.Max(0, victim[SecondaryAttributeTypes.Parry]*10 + bonusParry); // parry is in %
            int block = Math.Max(0, victim[SecondaryAttributeTypes.Block]*10 + bonusBlock); // block is in %
            int critical = attacker[SecondaryAttributeTypes.Critical];

            // check flags
            if (cannotMiss)
                miss = 0;
            if (cannotBeDodgedParriedBlocked)
            {
                dodge = 0;
                parry = 0;
                block = 0;
            }

            //
            int roll = RandomizeHelpers.Instance.Randomizer.Next(1000);
            Log.Default.WriteLine(LogLevels.Debug, $"MeleeAttackFromNotImpersonated: Roll: {roll} Miss: {miss} Dodge: {dodge}/{bonusDodge} Parry: {parry}/{bonusParry} Block: {block}/{bonusBlock} Crit: {critical} Crush: {crushing}");
            int cumulativeSum = 0;
            //
            if (roll < cumulativeSum + miss)
                return AttackResults.Miss;
            cumulativeSum += miss;
            //
            if (roll <= cumulativeSum + dodge)
                return AttackResults.Dodge;
            cumulativeSum += dodge;
            //
            if (roll <= cumulativeSum + parry)
                return AttackResults.Parry;
            cumulativeSum += parry;
            //
            if (roll <= cumulativeSum + block)
                return AttackResults.Block;
            cumulativeSum += block;
            //
            if (roll <= cumulativeSum + critical)
                return AttackResults.Critical;
            cumulativeSum += critical;
            //
            if (roll <= cumulativeSum + crushing)
                return AttackResults.CrushingBlow;
            //cumulativeSum += crushing;
            //
            return AttackResults.Hit;
        }

        private static AttackResults SpellAttackFromNotImpersonated(ICharacter attacker, ICharacter victim)
        {
            int deltaLevel = attacker.Level - victim.Level;
            int missChance;
            if (deltaLevel <= -10)
                missChance = 90;
            else if (deltaLevel == 9)
                missChance = 81;
            else if (deltaLevel == 8)
                missChance = 70;
            else if (deltaLevel == 7)
                missChance = 59;
            else if (deltaLevel == 6)
                missChance = 48;
            else if (deltaLevel == 5)
                missChance = 37;
            else if (deltaLevel == 4)
                missChance = 26;
            else if (deltaLevel == 3)
                missChance = 15;
            else if (deltaLevel == 2)
                missChance = 12;
            else if (deltaLevel == 1)
                missChance = 9;
            else if (deltaLevel == 0)
                missChance = 6;
            else if (deltaLevel == -1)
                missChance = 3;
            else
                missChance = 0;
            if (RandomizeHelpers.Instance.Randomizer.Next(100) <= missChance)
                return AttackResults.Miss;
            // no critical
            return AttackResults.Hit;
        }
    }
}
