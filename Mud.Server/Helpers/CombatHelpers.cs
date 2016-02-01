using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static AttackResults MeleeAttack(ICharacter attacker, ICharacter victim, bool dualWield)
        {
            //http://wow.gamepedia.com/Attack_table#Player_melee_and_ranged_attacks
            int levelDiff = attacker.Level - victim.Level;
            // following values must be / 10 to get %age
            int miss = 0;
            int dodge = 0;
            int parry = 0;
            int glance = 0;
            int block = 0;
            int critical = 0;
            int crushing = 0;
            // Following table is only available from PC to NPC
            if (levelDiff <= -10)
            {
                miss = 105;
                dodge = 105;
                parry = 135;
                glance = 1100;
                block = 180;
            }
            else if (levelDiff == -9)
            {
                miss = 90;
                dodge = 90;
                parry = 120;
                glance = 1000;
                block = 165;
            }
            else if (levelDiff == -8)
            {
                miss = 75;
                dodge = 75;
                parry = 105;
                glance = 800;
                block = 150;
            }
            else if (levelDiff == -7)
            {
                miss = 60;
                dodge = 60;
                parry = 90;
                glance = 800;
                block = 135;
            }
            else if (levelDiff == -6)
            {
                miss = 45;
                dodge = 45;
                parry = 75;
                glance = 700;
                block = 120;
            }
            else if (levelDiff == -5)
            {
                miss = 30;
                dodge = 30;
                parry = 60;
                glance = 600;
                block = 105;
            }
            else if (levelDiff == -4)
            {
                miss = 15;
                dodge = 15;
                parry = 45;
                glance = 500;
                block = 90;
            }
            else if (levelDiff == -3)
            {
                miss = 0;
                dodge = 0;
                parry = 3;
                glance = 0;
                block = 75;
            }
            else if (levelDiff == -2)
            {
                miss = 0;
                dodge = 0;
                parry = 0;
                glance = 0;
                block = 60;
            }
            else if (levelDiff == -1)
            {
                miss = 0;
                dodge = 0;
                parry = 0;
                glance = 0;
                block = 45;
            }
            else if (levelDiff == 0)
            {
                miss = 0;
                dodge = 0;
                parry = 0;
                glance = 0;
                block = 30;
            }
            else if (levelDiff == +1)
            {
                miss = 0;
                dodge = 0;
                parry = 0;
                glance = 0;
                block = 15;
            }
            else
            {
                miss = 0;
                dodge = 0;
                parry = 0;
                glance = 0;
                block = 0;
            }
            // TODO: critical and crushing blow
            if (dualWield)
                miss += 190;

            int number = RandomizeHelpers.Instance.Randomizer.Next(1000);
            Log.Default.WriteLine(LogLevels.Debug, "Number: {0} Miss: {1} Dodge: {2} Parry: {3} Glance: {4} Block: {5} Crit: {6} Crush: {7}", number, miss, dodge, parry, glance, block, critical, crushing);
            int cumulativeSum = 0;
            //
            if (number < cumulativeSum + miss)
                return AttackResults.Miss;
            cumulativeSum += miss;
            //
            if (number <= cumulativeSum + dodge)
                return AttackResults.Dodge;
            cumulativeSum += dodge;
            //
            if (number <= cumulativeSum + parry)
                return AttackResults.Parry;
            cumulativeSum += parry;
            //
            if (number <= cumulativeSum + glance)
                return AttackResults.GlancingBlow;
            cumulativeSum += glance;
            //
            if (number <= cumulativeSum + critical)
                return AttackResults.Critical;
            cumulativeSum += critical;
            //
            if (number <= cumulativeSum + crushing)
                return AttackResults.CrushingBlow;
            cumulativeSum += crushing;
            //
            return AttackResults.Hit;

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
    }
}
