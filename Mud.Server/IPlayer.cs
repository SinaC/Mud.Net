using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.Server
{
    public enum PlayerStates
    {
        Loading,
        Playing, // if not impersonating and not creating avatar
        CreatingAvatar, // creating avatar
        Impersonating, // playing avatar
    }

    public delegate void SendDataEventHandler(IPlayer player, string data);
    public delegate void PageDataEventHandler(IPlayer player, StringBuilder data);

    public interface IPlayer : IActor
    {
        event SendDataEventHandler SendData;
        event PageDataEventHandler PageData;

        Guid Id { get; }
        string Name { get; }
        string DisplayName { get; } // First letter is in upper-case
        List<ICharacter> Avatars { get; } // List of character a player can impersonate

        int GlobalCooldown { get; } // delay (in Pulse) before next action    check WAIT_STATE

        PlayerStates PlayerState { get; }

        ICharacter Impersonating { get; }

        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        bool Load(string name);

        void OnDisconnected();
    }
}
