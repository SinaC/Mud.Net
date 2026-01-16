using Mud.Common;
using Mud.Server.Random;

namespace Mud.Server.Tests.Random;

[TestClass]
public class OneHitDiceRollTests
{
    [TestMethod]
    public void CheckDiceRoll()
    {
        var randomManager = new RandomManager(0);

        var results = new Dictionary<int, int>();

        for (int i = 0; i < 10_000_000; i++)
        {
            int diceRoll;
            while ((diceRoll = randomManager.Range(0, (1 << 5)-1)) >= 20)
                ;
            results.Increment(diceRoll);
        }
    }
}
