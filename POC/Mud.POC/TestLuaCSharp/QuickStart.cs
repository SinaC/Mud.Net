using Lua;

namespace Mud.POC.TestLuaCSharp
{
    public class QuickStart
    {
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
}
