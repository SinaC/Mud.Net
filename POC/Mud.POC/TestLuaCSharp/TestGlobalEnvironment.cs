using Lua;

namespace Mud.POC.TestLuaCSharp
{
    public class TestGlobalEnvironment
    {
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
}
