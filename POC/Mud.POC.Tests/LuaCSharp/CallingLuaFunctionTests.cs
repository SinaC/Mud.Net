using Lua;

namespace Mud.POC.Tests.LuaCSharp;

[TestClass]
public class CallingLuaFunctionTests
{
    [TestMethod]
    public async Task Run()
    {
        // Create a LuaState
        var state = LuaState.Create();

        //
        //var results = await state.DoFileAsync("lua2cs.lua");
        var results = await state.DoStringAsync(@"
-- lua2cs.lua

local function add(a, b)
    return a + b
end

return add;");
        var func = results[0];

        // Execute the function with arguments
        var funcResults = await state.CallAsync(func, [1, 2 ]);

        // 3
        Console.WriteLine(funcResults[0]);
    }
}
