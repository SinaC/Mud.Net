using System.Collections.Generic;

namespace Mud.POC.GroupsPetsFollowers
{
    public interface IRoom
    {
        string Name { get; }

        IEnumerable<ICharacter> People { get; }
        IEnumerable<IPlayableCharacter> PlayableCharacters { get; }
        IEnumerable<INonPlayableCharacter> NonPlayableCharacters { get; }

        void Enter(ICharacter character);
        void Leave(ICharacter character);
    }
}
