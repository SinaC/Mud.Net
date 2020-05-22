using System;
using System.Collections.Generic;

namespace Mud.POC.GroupsPetsFollowers
{
    public class World : IWorld
    {
        private static readonly Lazy<IWorld> LazyWorld = new Lazy<IWorld>(() => new World());
        public static IWorld Instance = LazyWorld.Value;

        private readonly List<ICharacter> _characters = new List<ICharacter>();

        public IEnumerable<ICharacter> Characters => _characters;

        public void AddCharacter(ICharacter character)
        {
            _characters.Add(character);
        }

        public void Clear()
        {
            _characters.Clear();
        }
    }
}
