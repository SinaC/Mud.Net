using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Table
{
    public interface ITableValues
    {
        (int hit, int dam, int carry, int wield, int learn, int practice, int defensive, int hitpoint, int shock) Bonus(ICharacter character);
        int HitBonus(ICharacter character);
        int DamBonus(ICharacter character);
        int CarryBonus(ICharacter character);
        int WieldBonus(ICharacter character);
        int LearnBonus(ICharacter character);
        int PracticeBonus(ICharacter character);
        int DefensiveBonus(ICharacter character);
        int HitpointBonus(ICharacter character);
        int ShockBonus(ICharacter character);

        int EquipmentSlotMultiplier(EquipmentSlots slot);

        int MovementLoss(SectorTypes sector);

        (string name, string color, int proof, int full, int thirst, int food, int servingsize) LiquidInfo(string liquidName);
    }
}
