using System.Diagnostics;
using System.Runtime.CompilerServices;
using NLua;

namespace Mud.POC.TestLua
{
    public class LuaScript<T>
    {
        private readonly LuaTable _table;

        protected readonly T Entity;

        protected LuaScript(Lua lua, T entity, string tableName)
        {
            Entity = entity;

            _table = lua.GetTable(tableName);
        }

        protected LuaFunction GetLuaFunction([CallerMemberName] string fctName = null)
        {
            Debug.WriteLine($"DEBUG: LuaScript::GetLuaFunction: Instance [{Entity}] fctName [{fctName}]");
            LuaFunction function = _table?[fctName] as LuaFunction;
            return function;
        }

        // This doesn't work because args is considered one argument instead of a list of arguments
        // should recreate an array of object with Entity as first entry
        // object[] luaArgs = new object[args.Length+1];
        // luaArgs[0] = Entity
        // Array.Copy(args, 0, luaArgs, 1, args.Length);
        // luaFunction?.Call(luaArgs);
        //protected object[] CallLuaFunction(string fctName, params object[] args)
        //{
        //    LuaFunction luaFunction = _table?[fctName] as LuaFunction;
        //    return luaFunction?.Call(Entity, args); // Entity will be 'self' in lua
        //}
    }
}
