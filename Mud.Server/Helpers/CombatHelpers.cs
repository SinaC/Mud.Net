using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;

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

        public enum CombatDifficulties
        {
            Grey,   // 0
            Green,  // 1
            Yellow, // 2
            Orange, // 3
            Red,    // 4
            Skull   // 5
        }

        #region Experience to next level

        //http://wow.gamepedia.com/Experience_to_level#Experience_to_level_pre-7.0_.28Post-Squish.29
        public static readonly IDictionary<int, long> ExperienceToNextLevel = new Dictionary<int, long>
        {
            {1, 400},
            {2, 900},
            {3, 1400},
            {4, 2100},
            {5, 2800},
            {6, 3600},
            {7, 4500},
            {8, 5400},
            {9, 6500},
            {10, 6700},

            {11, 7000},
            {12, 7700},
            {13, 8700},
            {14, 9700},
            {15, 10800},
            {16, 11900},
            {17, 13100},
            {18, 14200},
            {19, 15400},
            {20, 16600},

            {21, 17900},
            {22, 19200},
            {23, 20400},
            {24, 21800},
            {25, 23100},
            {26, 24400},
            {27, 25800},
            {28, 27100},
            {29, 29000},
            {30, 31000},

            {31, 33300},
            {32, 35700},
            {33, 38400},
            {34, 41100},
            {35, 44000},
            {36, 47000},
            {37, 49900},
            {38, 53000},
            {39, 56200},
            {40, 74300},

            {41, 78500},
            {42, 82800},
            {43, 87100},
            {44, 91600},
            {45, 96300},
            {46, 101000},
            {47, 105800},
            {48, 110700},
            {49, 115700},
            {50, 120900},

            {51, 126100},
            {52, 131500},
            {53, 137000},
            {54, 142500},
            {55, 148200},
            {56, 154000},
            {57, 159900},
            {58, 165800},
            {59, 172000},
            {60, 254000},

            {61, 275000},
            {62, 301000},
            {63, 328000},
            {64, 359000},
            {65, 367000},
            {66, 374000},
            {67, 381000},
            {68, 388000},
            {69, 395000},
            {70, 405000},

            {71, 415000},
            {72, 422000},
            {73, 427000},
            {74, 432000},
            {75, 438000},
            {76, 445000},
            {77, 455000},
            {78, 462000},
            {79, 474000},
            {80, 482000},

            {81, 487000},
            {82, 492000},
            {83, 497000},
            {84, 506000},
            {85, 517000},
            {86, 545000},
            {87, 550000},
            {88, 556000},
            {89, 562000},
            {90, 774800},

            {91, 783900},
            {92, 790400},
            {93, 798200},
            {94, 807300},
            {95, 815100},
            {96, 821600},
            {97, 830700},
            {98, 838500},
            {99, 846300},
            {100, 0},
        };

        public static readonly IDictionary<int, long> CumulativeExperienceByLevel = ExperienceToNextLevel.ToDictionary(x => x.Key, x => ExperienceToNextLevel.Where(y => y.Key < x.Key).Sum(y => y.Value));

        #endregion

        public static CombatDifficulties GetConColor(int level, int targetLevel)
        {
            if (level + 5 <= targetLevel)
                return level + 10 <= targetLevel 
                    ? CombatDifficulties.Skull 
                    : CombatDifficulties.Red;
            switch (targetLevel - level)
            {
                case 4:
                case 3:
                    return CombatDifficulties.Orange;
                case 2:
                case 1:
                case 0:
                case -1:
                case -2:
                    return CombatDifficulties.Yellow;
                default:
                    // More advanced formula for grey/green levels
                    if (level <= 5)
                        return CombatDifficulties.Green; // All others are green.
                    if (level <= 39)
                        // Its below or equal to the 'grey level'
                        return targetLevel <= level - 5 - (int) Math.Floor((decimal)level/10) 
                            ? CombatDifficulties.Grey 
                            : CombatDifficulties.Green;
                    // Over level 39:
                    return targetLevel <= level - 1 - (int) Math.Floor((decimal)level /5) 
                        ? CombatDifficulties.Grey 
                        : CombatDifficulties.Green;
            }
        }

        //http://wow.gamepedia.com/Mob_experience#Example_Code
        private static int GetZeroDifference(int level)
        {
            if (level <= 7)
                return 5;
            if (level <= 9)
                return 6;
            if (level <= 11)
                return 7;
            if (level <= 15)
                return 8;
            if (level <= 19)
                return 9;
            if (level <= 29)
                return 11;
            if (level <= 39)
                return 12;
            if (level <= 44)
                return 13;
            if (level <= 49)
                return 14;
            if (level <= 54)
                return 15;
            if (level <= 59)
                return 16;
            return 17; // Approx.
        }

        private static decimal GetMobExperience(int playerLvl, int mobLvl)
        {
            // TODO: replace +45 according to http://wow.gamepedia.com/Mob_experience#Basic_Formula
            if (mobLvl >= playerLvl)
            {
                decimal temp = ((decimal)playerLvl * 5 + 45) * (1 + 0.05m * ((decimal)mobLvl - (decimal)playerLvl));
                decimal tempCap = ((decimal)playerLvl * 5 + 45) * 1.2m;
                return Math.Floor(Math.Min(temp, tempCap));
            }
            if (GetConColor(playerLvl, mobLvl) == CombatDifficulties.Grey)
                return 0; // np difficulty -> no gain
            return Math.Floor((decimal)playerLvl * 5 + 45) * (1 - ((decimal)playerLvl - (decimal)mobLvl) / (decimal)GetZeroDifference(playerLvl));
        }

        private static decimal GetEliteMobExperience(int playerLvl, int mobLvl)
        {
            return GetMobExperience(playerLvl, mobLvl) * 2;
        }

        private static decimal GetMobExperienceFull(int playerLvl, int mobLvl, bool isElite)
        {
            return isElite ? GetEliteMobExperience(playerLvl, mobLvl) : GetMobExperience(playerLvl, mobLvl);
        }

        private static decimal ApplyRestedExperience(decimal experience, long restedExperienceLeft)
        {
            if (restedExperienceLeft == 0)
                return experience;
            if (restedExperienceLeft >= experience)
                return experience * 2;
            //Restedness is partially covering the XP gained.
            // XP = rest + (AXP - (rest / 2))
            return restedExperienceLeft + (experience - (decimal)restedExperienceLeft / 2);
        }

        private static decimal GetPartyExperienceBonus(int playerCount)
        {
            switch (playerCount)
            {
                case 3:
                    return 1.116m;
                case 4:
                    return 1.3m;
                case 5:
                    return 1.4m;
                default:
                    return 1;
            }
        }

        public static long GetSoloMobExperienceFull(int playerLvl, int mobLvl, bool isElite, long restedExperienceLeft)
        {
            return (long)ApplyRestedExperience(GetMobExperienceFull(playerLvl, mobLvl, isElite), restedExperienceLeft);
        }

        public static long GetDuoMobExperienceFull(int player1Lvl, int player2Lvl, int mobLvl, bool isElite, long restedExperienceLeft)
        {
            int highestLvl = Math.Max(player1Lvl, player2Lvl);
            decimal temp = GetMobExperienceFull(highestLvl, mobLvl, isElite);
            return (long)ApplyRestedExperience(player1Lvl * temp / (player1Lvl + player2Lvl), restedExperienceLeft);
        }

        // Party Mob XP
        public static long GetPartyMobExperienceFull(int playerLvl, int highestLvl, int sumLvls, int playerCount, int mobLvl, bool isElite, long restedExperienceLeft)
        {
            decimal temp = GetMobExperienceFull(highestLvl, mobLvl, isElite) * GetPartyExperienceBonus(playerCount);
            // temp = XP from soloing via highest lvl...
            temp = (temp*playerLvl)/sumLvls;
            return (long)ApplyRestedExperience(temp, restedExperienceLeft);
        }

        // TODO: check block only if shield is equiped

        // White melee: auto-attack
        public static AttackResults WhiteMeleeAttack(ICharacter attacker, ICharacter victim, bool notMainWield)
        {
            //http://wow.gamepedia.com/Attack_table#Player_melee_and_ranged_attacks
            if (attacker is IPlayableCharacter playableCharacter)
                return MeleeAttackFromImpersonated(playableCharacter, victim, notMainWield, true, false, false);
            else
                return MeleeAttackFromNotImpersonated(attacker as INonPlayableCharacter, victim, notMainWield, false, false);

            //// TODO: if victim can't see this, chance are divided by 2
        }

        // Yellow melee: melee ability
        public static AttackResults YellowMeleeAttack(ICharacter attacker, ICharacter victim, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
        {
            //http://wow.gamepedia.com/Attack_table#Player_melee_and_ranged_attacks
            //http://wow.gamepedia.com/Glancing_blow#Glancing_blow_implications_-_crit_cap
            //http://wow.gamepedia.com/Hit#Special_attacks
            // according to --^ yellow damage cannot land glancing blow neither critical strike + miss chance is reduced to 8% (dual wield or not)
            if (attacker is IPlayableCharacter playableCharacter)
                return MeleeAttackFromImpersonated(playableCharacter, victim, false, false, cannotMiss, cannotBeDodgedParriedBlocked);
            else
                return MeleeAttackFromNotImpersonated(attacker as INonPlayableCharacter, victim, false, cannotMiss, cannotBeDodgedParriedBlocked);
        }

        public static AttackResults SpellAttack(ICharacter attacker, ICharacter victim, bool cannotMiss)
        {
            if (attacker is IPlayableCharacter playableCharacter)
                return SpellAttackFromImpersonated(playableCharacter, victim);
            else
                return SpellAttackFromNotImpersonated(attacker as INonPlayableCharacter, victim);
        }

        // PC attacking a NPC/PC
        private static AttackResults MeleeAttackFromImpersonated(IPlayableCharacter attacker, ICharacter victim, bool notMainWield, bool allowGlancingBlow /*yellow melee don't do glancing blow*/, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
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
            if (notMainWield)
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
            int roll = DependencyContainer.Current.GetInstance<IRandomManager>().Randomizer.Next(1000);
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

        private static AttackResults SpellAttackFromImpersonated(IPlayableCharacter attacker, ICharacter victim)
        {
            int deltaLevel = attacker.Level - victim.Level;
            // miss chance
            // -10 -> 77
            // -7 -> 44
            // -3 -> 0
            int missChance = (Math.Min(0, Math.Max(-10, deltaLevel)) - 3)*11;
            if (DependencyContainer.Current.GetInstance<IRandomManager>().Randomizer.Next(100) <= missChance)
                return AttackResults.Miss;
            // crit chance
            int deltaCritical = Math.Min(0, Math.Max(-10, deltaLevel));
            int critical = Math.Max(0, attacker[SecondaryAttributeTypes.Critical] - deltaCritical);
            if (DependencyContainer.Current.GetInstance<IRandomManager>().Randomizer.Next(100) <= critical)
                return AttackResults.Critical;
            return AttackResults.Hit;
        }

        // NPC attacking a PC
        private static AttackResults MeleeAttackFromNotImpersonated(INonPlayableCharacter attacker, ICharacter victim, bool notMainWield, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
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
            if (notMainWield)
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
            int roll = DependencyContainer.Current.GetInstance<IRandomManager>().Randomizer.Next(1000);
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

        private static AttackResults SpellAttackFromNotImpersonated(INonPlayableCharacter attacker, ICharacter victim)
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
            if (DependencyContainer.Current.GetInstance<IRandomManager>().Randomizer.Next(100) <= missChance)
                return AttackResults.Miss;
            // no critical
            return AttackResults.Hit;
        }
    }
}
