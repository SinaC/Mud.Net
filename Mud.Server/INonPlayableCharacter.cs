using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface INonPlayableCharacter : ICharacter
    {
        CharacterBlueprintBase Blueprint { get; }

        string DamageNoun { get; }
        SchoolTypes DamageType { get; }
        int DamageDiceCount { get; }
        int DamageDiceValue { get; }
        int DamageDiceBonus { get; }

        ActFlags ActFlags { get; }

        OffensiveFlags OffensiveFlags { get; }

        bool IsQuestObjective(IPlayableCharacter questingCharacter);

        // Pet/charmies
        IPlayableCharacter Master { get; } // character allowed to order us
        void ChangeMaster(IPlayableCharacter master);
        bool Order(string rawParameters, params CommandParameter[] parameters);

        // Mapping
        PetData MapPetData();
    }
}
