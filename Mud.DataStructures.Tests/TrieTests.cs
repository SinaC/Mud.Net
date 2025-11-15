using Mud.DataStructures.Trie;

namespace Mud.DataStructures.Tests;

[TestClass]
public class TrieTests
{
    [TestMethod]
    public void GetByPrefix_NoResult()
    {
        Trie<int> trie = new()
        {
            { "apple", 1 },
            { "banana", 2 }
        };

        var results = trie.GetByPrefix("z").ToList();
        Assert.IsEmpty(results);
    }

    [TestMethod]
    public void GetByPrefix_OneResult()
    {
        Trie<int> trie = new()
        {
            { "apple", 1 },
            { "banana", 2 }
        };

        var results = trie.GetByPrefix("a").ToList();
        Assert.ContainsSingle(results);
        Assert.AreEqual("apple", results.Single().Key);
        Assert.AreEqual(1, results.Single().Value);
    }

    [TestMethod]
    public void GetByPrefix_MultipleResults()
    {
        Trie<int> trie = new()
        {
            { "apple", 1 },
            { "apricot", 2 }
        };

        var results = trie.GetByPrefix("a").ToList();
        Assert.HasCount(2, results);
        Assert.ContainsSingle(results.Where(x => x.Key == "apple"));
        Assert.ContainsSingle(results.Where(x => x.Key == "apricot"));
        Assert.ContainsSingle(results.Where(x => x.Value == 1));
        Assert.ContainsSingle(results.Where(x => x.Value == 2));
    }
}
