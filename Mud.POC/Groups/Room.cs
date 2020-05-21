using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Groups
{
    public class Room : IRoom
    {
        private List<ICharacter> _people;

        public Room(string name)
        {
            _people = new List<ICharacter>();

            Name = name;
        }

        #region IRoom

        public string Name { get; }

        public IEnumerable<ICharacter> People => _people;

        public IEnumerable<IPlayableCharacter> PlayableCharacters => People.OfType<IPlayableCharacter>();

        public IEnumerable<INonPlayableCharacter> NonPlayableCharacters => People.OfType<INonPlayableCharacter>();

        public void Enter(ICharacter character)
        {
            if (_people.Contains(character))
                return;
            _people.Add(character);
        }

        public void Leave(ICharacter character)
        {
            _people.Remove(character);
        }

        #endregion
    }
}
