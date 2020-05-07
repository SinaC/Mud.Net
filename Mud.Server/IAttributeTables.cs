namespace Mud.Server
{
    public interface IAttributeTables
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
    }
}
