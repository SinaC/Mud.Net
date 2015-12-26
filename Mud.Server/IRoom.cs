using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public interface IRoom
    {
        // TODO: exits, objects
        IReadOnlyCollection<ICharacter> CharactersInRoom { get; }

        void Enter(ICharacter character);
        void Leave(ICharacter character);
    }
}
