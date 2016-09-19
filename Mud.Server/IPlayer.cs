using System;
using System.Collections.Generic;
using System.Text;
using Mud.Datas.DataContracts;

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

        int GlobalCooldown { get; } // delay (in Pulse) before next action    check WAIT_STATE

        PlayerStates PlayerState { get; }

        ICharacter Impersonating { get; }

        IPlayer LastTeller { get; } // used by DoReply

        IAdmin SnoopBy { get; } // every messages send to 'this' will be sent to SnoopBy

        DateTime LastCommandTimestamp { get; }
        string LastCommand { get; }

        string Prompt { get; }

        bool IsAfk { get; }
        IEnumerable<string> DelayedTells { get; } // Tell stored while AFK

        void DecreaseGlobalCooldown(); // decrease one by one
        void SetGlobalCooldown(int pulseCount); // set global cooldown delay (in pulse)

        bool Load(string name);
        bool Save();

        void SetLastTeller(IPlayer teller);
        void AddDelayedTell(string sentence);
        void ClearDelayedTells();

        void SetSnoopBy(IAdmin snooper);

        void AddAvatar(CharacterData characterData);
        void StopImpersonating();

        void OnDisconnected();
    }
}
