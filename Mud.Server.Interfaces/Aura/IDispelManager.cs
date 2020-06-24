using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Aura
{
    public interface IDispelManager
    {
        bool TryDispels(int dispelLevel, ICharacter victim); // dispels every spells
        TryDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName); // dispel first spell
        bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft);
    }

    public enum TryDispelReturnValues
    {
        NotFound,
        Dispelled,
        FoundAndNotDispelled
    }
}
