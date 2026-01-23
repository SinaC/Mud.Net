using System.Numerics;
using Lua;
using Lua.CodeAnalysis.Syntax.Nodes;

namespace Mud.POC.Tests.LuaCSharp;

[TestClass]
public class LuaObjectTests
{
    [TestMethod]
    public async Task Run1()
    {
        var state = LuaState.Create();

        // Add an instance of the defined LuaObject as a global variable
        // (Implicit conversion to LuaValue is automatically defined for classes with the LuaObject attribute)
        state.Environment["Vector3"] = new LuaVector3();

        state.Environment["print"] = new LuaFunction((context, ct) =>
        {
            // Get the arguments using context.GetArgument<T>()
            var arg0 = context.GetArgument<double>(0);
            var arg1 = context.GetArgument<double>(1);
            var arg2 = context.GetArgument<double>(2);

            Console.WriteLine(arg0 + " " + arg1 + " " + arg2);

            return new(0);
        });

        await state.DoStringAsync(
@"
-- vector3_sample.lua

local v1 = Vector3.create(1, 2, 3)
-- 1  2  3
print(v1.x, v1.y, v1.z)

local v2 = v1:normalized()
-- 0.26726123690605164  0.5345224738121033  0.8017836809158325
print(v2.x, v2.y, v2.z)");
    }

    [TestMethod]
    public async Task Run2()
    {
        var state = LuaState.Create();

        // Add an instance of the defined LuaObject as a global variable
        // (Implicit conversion to LuaValue is automatically defined for classes with the LuaObject attribute)
        state.Environment["Vector3"] = new LuaVector3();

        state.Environment["print"] = new LuaFunction((context, ct) =>
        {
            //var arg = context.GetArgument<object>(0);
            var arg = context.GetArgument<LuaVector3>(0);
            Console.WriteLine(arg);

            return new(0);
        });

        await state.DoStringAsync(
@"
local v1 = Vector3.create(1, 1, 1)
local v2 = Vector3.create(2, 2, 2)

print(v1) -- <1, 1, 1>
print(v2) -- <2, 2, 2>

print(v1 + v2) -- <3, 3, 3>
print(v1 - v2) -- <-1, -1, -1>
");
    }
}

// Add LuaObject attribute and partial keyword
[LuaObject]
public partial class LuaVector3
{
    Vector3 vector;

    // Add LuaMember attribute to members that will be used in Lua
    // The argument specifies the name used in Lua (if omitted, the member name is used)
    [LuaMember("x")]
    public float X
    {
        get => vector.X;
        set => vector = vector with { X = value };
    }

    [LuaMember("y")]
    public float Y
    {
        get => vector.Y;
        set => vector = vector with { Y = value };
    }

    [LuaMember("z")]
    public float Z
    {
        get => vector.Z;
        set => vector = vector with { Z = value };
    }

    // Static methods are treated as regular Lua functions
    [LuaMember("create")]
    public static LuaVector3 Create(float x, float y, float z)
    {
        return new LuaVector3()
        {
            vector = new Vector3(x, y, z)
        };
    }

    // Instance methods implicitly receive the instance (this) as the first argument
    // In Lua, this is accessed with instance:method() syntax
    [LuaMember("normalized")]
    public LuaVector3 Normalized()
    {
        return new LuaVector3()
        {
            vector = Vector3.Normalize(vector)
        };
    }

    [LuaMetamethod(LuaObjectMetamethod.Add)]
    public static LuaVector3 Add(LuaVector3 a, LuaVector3 b)
    {
        return new LuaVector3()
        {
            vector = a.vector + b.vector
        };
    }

    [LuaMetamethod(LuaObjectMetamethod.Sub)]
    public static LuaVector3 Sub(LuaVector3 a, LuaVector3 b)
    {
        return new LuaVector3()
        {
            vector = a.vector - b.vector
        };
    }

    [LuaMetamethod(LuaObjectMetamethod.ToString)]
    public override string ToString()
    {
        return vector.ToString();
    }
}
