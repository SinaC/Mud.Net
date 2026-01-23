using Lua;

namespace Mud.POC.Tests.LuaCSharp;

[TestClass]
public class LuaTableTests
{
    [TestMethod]
    public async Task Run()
    {
        // Create a LuaState
        var state = LuaState.Create();

        // Create a table in Lua
        var results = await state.DoStringAsync("return { a = 1, b = 2, c = 3 }");
        var table1 = results[0].Read<LuaTable>();

        // 1
        Console.WriteLine(table1["a"]);

        // Create a table in C#
        results = await state.DoStringAsync("return { 1, 2, 3 }");
        var table2 = results[0].Read<LuaTable>();

        // 1 (Note: Lua arrays are 1-indexed)
        Console.WriteLine(table2[1]);
    }
}
