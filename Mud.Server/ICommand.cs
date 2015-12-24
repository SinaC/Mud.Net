using System;

namespace Mud.Server
{
    [Flags]
    public enum CommandFlags
    {
        InGame = 0x01,
        OutOfGame = 0x02,
    }

    public interface ICommand
    {
        CommandFlags Flags { get; }
        string Name { get; }
        string Help { get; }

        bool Execute(IClient actor, string raw, params CommandParameter[] parameters); // return false if invalid, true if valid
    }
}
