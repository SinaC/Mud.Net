namespace Mud.DataStructures.Trie;

public interface IReadOnlyTrie<TValue> : IReadOnlyDictionary<string, TValue>
{
    IEnumerable<TrieEntry<TValue>> GetByPrefix(string prefix);
}
