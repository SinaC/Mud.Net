using System.Collections.Generic;

namespace Mud.DataStructures.Trie
{
    public interface IReadOnlyTrie<TValue>
    {
        IEnumerable<TrieEntry<TValue>> GetByPrefix(string prefix);
        IEnumerable<string> Keys { get; }
    }
}
