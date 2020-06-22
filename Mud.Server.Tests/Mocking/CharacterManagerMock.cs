using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class CharacterManagerMock : ICharacterManager
    {
        private readonly List<ICharacter> _characters = new List<ICharacter>();
        private readonly List<CharacterBlueprintBase> _characterBlueprints = new List<CharacterBlueprintBase>();

        public IReadOnlyCollection<CharacterBlueprintBase> CharacterBlueprints => _characterBlueprints;

        public IEnumerable<ICharacter> Characters => _characters;
        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => Characters.OfType<INonPlayableCharacter>();
        public IEnumerable<IPlayableCharacter> PlayableCharacters => Characters.OfType<IPlayableCharacter>();

        public void AddCharacterBlueprint(CharacterBlueprintBase blueprint)
        {
            _characterBlueprints.Add(blueprint);
        }

        public IPlayableCharacter AddPlayableCharacter(Guid guid, PlayableCharacterData characterData, IPlayer player, IRoom room)
        {
            IPlayableCharacter character = new Character.PlayableCharacter.PlayableCharacter(guid, characterData, player, room);
            _characters.Add(character);
            return character;
        }

        public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, IRoom room)
        {
            INonPlayableCharacter character = new Character.NonPlayableCharacter.NonPlayableCharacter(guid, blueprint, room);
            _characters.Add(character);
            return character;
        }

        public INonPlayableCharacter AddNonPlayableCharacter(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room)
        {
            throw new NotImplementedException();
        }

        public CharacterBlueprintBase GetCharacterBlueprint(int id)
        {
            return _characterBlueprints.FirstOrDefault(x => x.Id == id);
        }

        public TBlueprint GetCharacterBlueprint<TBlueprint>(int id)
                    where TBlueprint : CharacterBlueprintBase
        {
            return _characterBlueprints.OfType<TBlueprint>().FirstOrDefault(x => x.Id == id);
        }

        public void RemoveCharacter(ICharacter character)
        {
            throw new NotImplementedException();
        }
    }
}
