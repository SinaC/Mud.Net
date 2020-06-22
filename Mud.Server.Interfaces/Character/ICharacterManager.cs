using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using System;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Character
{
    public interface ICharacterManager
    {
        IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints { get; }

        CharacterBlueprintBase GetCharacterBlueprint(int id);
        TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
            where TBlueprint : CharacterBlueprintBase;

        void AddCharacterBlueprint(CharacterBlueprintBase blueprint);

        IEnumerable<ICharacter> Characters { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }
        IEnumerable<IPlayableCharacter> PlayableCharacters { get; }

        IPlayableCharacter AddPlayableCharacter(Guid guid, PlayableCharacterData playableCharacterData, IPlayer player, IRoom room);
        INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room);
        INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room);

        void RemoveCharacter(ICharacter character);
    }
}
