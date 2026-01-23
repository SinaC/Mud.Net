using Lua;

namespace Mud.POC.Tests.LuaCSharp;

[TestClass]
public class QuickStartTests
{
    [TestMethod]
    public async Task Run()
    {
        // Create a LuaState
        var state = LuaState.Create();

        // Execute a Lua script string with DoStringAsync
        var results = await state.DoStringAsync("return 1 + 1");

        // 2
        Console.WriteLine(results[0]);
    }
}
