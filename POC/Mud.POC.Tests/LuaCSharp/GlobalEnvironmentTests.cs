using Lua;

namespace Mud.POC.Tests.LuaCSharp;

[TestClass]
public class GlobalEnvironmentTests
{
    [TestMethod]
    public async Task Run()
    {
        // Create a LuaState
        var state = LuaState.Create();

        // Set a = 10
        state.Environment["a"] = 10;

        var results = await state.DoStringAsync("return a");

        // 10
        Console.WriteLine(results[0]);
    }
}
