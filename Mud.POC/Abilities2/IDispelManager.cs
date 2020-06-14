using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2
{
    public interface IDispelManager
    {
        bool TryDispels(int dispelLevel, ICharacter victim); // dispels every spells
        TryDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName); // dispel first spell
        bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft);
        bool SavesDispel(int dispelLevel, IAura aura);
    }

    public enum TryDispelReturnValues
    {
        NotFound,
        Dispelled,
        FoundAndNotDispelled
    }
}
