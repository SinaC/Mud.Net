using Mud.DataStructures.Trie;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface IEntity
    {
        string Name { get; }
        string DebugName { get; }
        IEnumerable<string> Keywords { get; } // name tokenize

        IReadOnlyTrie<ICommandExecutionInfo> Commands { get; }
    }
}
