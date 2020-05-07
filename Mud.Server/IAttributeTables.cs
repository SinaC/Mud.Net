namespace Mud.Server
{
    public interface IAttributeTables
    {
        (int hit, int dam, int carry, int wield, int learn, int practice, int defensive, int hitpoint, int shock) Bonus(IPlayableCharacter character);
        int HitBonus(IPlayableCharacter character);
        int DamBonus(IPlayableCharacter character);
        int CarryBonus(IPlayableCharacter character);
        int WieldBonus(IPlayableCharacter character);
        int LearnBonus(IPlayableCharacter character);
        int PracticeBonus(IPlayableCharacter character);
        int DefensiveBonus(IPlayableCharacter character);
        int HitpointBonus(IPlayableCharacter character);
        int ShockBonus(IPlayableCharacter character);
    }
}
