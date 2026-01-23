using Lua;

namespace Mud.POC.Tests.LuaCSharp;

[TestClass]
public class CallingCSharpFunctionTests
{
    [TestMethod]
    public async Task MyTestMethod()
    {
        // Create a LuaState
        var state = LuaState.Create();

        state.Environment["add"] = new LuaFunction((context, ct) =>
        {
            // Get the arguments using context.GetArgument<T>()
            var arg0 = context.GetArgument<double>(0);
            var arg1 = context.GetArgument<double>(1);

            // Set the return value to the context
            context.Return(arg0 + arg1);

            // If there are multiple values, you need to pass them together as follows.
            // context.Return(arg0, arg1);
            // context.Return([arg0, arg1]);

            // Return the number of values
            return new(1);
            // return new(context.Return(arg0 + arg1)); // or this way
        });

        // Execute a Lua script
        var results = await state.DoStringAsync(
@"return add(1, 2)"); // cs2lua.lua
    }
}
