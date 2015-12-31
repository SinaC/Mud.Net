using System;

namespace Mud.Server
{
    public enum PlayerStates
    {
        Connecting, // before login successfull
        Connected, // if not playing and not creating avatar
        CreatingAvatar, // creating avatar
        Playing, // playing avatar
    }

    public delegate void SendDataEventHandler(IPlayer player, string data);

    public interface IPlayer : IActor
    {
        event SendDataEventHandler SendData;

        Guid Id { get; }
        string Name { get; }
        string DisplayName { get; } // First letter is in upper-case
        
        PlayerStates PlayerState { get; }

        ICharacter Impersonating { get; }

        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        bool Load(string name);

        void OnDisconnected();
    }
}
