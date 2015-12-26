using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public interface IPlayer : IActor
    {
        Guid Id { get; }
        string Name { get; }

        ICharacter Impersonating { get; }

        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        bool GoInGame(ICharacter character);
        bool GoOutOfGame();

        void OnDisconnected();
    }
}
