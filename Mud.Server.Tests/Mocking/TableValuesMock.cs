using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Table;
using System;

namespace Mud.Server.Tests.Mocking
{
    public class TableValuesMock : ITableValues
    {
        public (int hit, int dam, int carry, int wield, int learn, int practice, int defensive, int hitpoint, int shock) Bonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int CarryBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int DamBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int DefensiveBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int EquipmentSlotMultiplier(EquipmentSlots slot)
        {
            throw new NotImplementedException();
        }

        public int HitBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int HitpointBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int LearnBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public (string name, string color, int proof, int full, int thirst, int food, int servingsize) LiquidInfo(string liquidName)
        {
            throw new NotImplementedException();
        }

        public int MovementLoss(SectorTypes sector)
        {
            throw new NotImplementedException();
        }

        public int PracticeBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int ShockBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }

        public int WieldBonus(ICharacter character)
        {
            throw new NotImplementedException();
        }
    }
}
