using Lua;

namespace Mud.POC.TestLuaCSharp
{
//    public class TestCallingLuaFunctions
//    {
//        public async Task Run()
//        {
//            // Create a LuaState
//            var state = LuaState.Create();

//            //
//            //var results = await state.DoFileAsync("lua2cs.lua");
//            var results = await state.DoStringAsync(@"
//-- lua2cs.lua

//local function add(a, b)
//    return a + b
//end

//return add;");
//            var func = results[0].Read<LuaFunction>();

//            // Execute the function with arguments
//            var funcResults = await func.InvokeAsync(state, new[] { 1, 2 }); // <-- doesn't exist

//            // 3
//            Console.WriteLine(funcResults[0]);
//        }
//    }
}
