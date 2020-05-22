using System.Collections.Generic;

namespace Mud.POC.GroupsPetsFollowers
{
    public interface IWorld
    {
        IEnumerable<ICharacter> Characters { get; }

        void AddCharacter(ICharacter character);

        void Clear();
    }
}
