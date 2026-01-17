using Mud.Blueprints.Character;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Interfaces.Character;

public interface ICharacterManager
{
    IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints { get; }

    CharacterBlueprintBase? GetCharacterBlueprint(int id);
    TBlueprint? GetCharacterBlueprint<TBlueprint>(int id)
        where TBlueprint : CharacterBlueprintBase;

    void AddCharacterBlueprint(CharacterBlueprintBase blueprint);

    IEnumerable<ICharacter> Characters { get; }
    IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
    IEnumerable<IPlayableCharacter> PlayableCharacters { get; }
    int Count(int blueprintId);

    IPlayableCharacter AddPlayableCharacter(Guid guid, AvatarData playableCharacterData, IPlayer player, IRoom room);
    INonPlayableCharacter? AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room);
    INonPlayableCharacter? AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room);

    void RemoveCharacter(ICharacter character);

    void Cleanup();
}
