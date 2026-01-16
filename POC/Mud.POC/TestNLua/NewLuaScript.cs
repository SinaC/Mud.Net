using System;
using System.Diagnostics;
using NLua;

namespace Mud.POC.TestNLua
{
    public abstract class NewLuaScript<TEntity, TEntityWrapper>
        where TEntity : TEntityWrapper // maybe this is too constraining (for existing code) but it makes sense
    {
        private readonly LuaTable _table;

        protected TEntity Entity { get; }
        protected TEntityWrapper EntityWrapper { get; }

        protected NewLuaScript(NLua.Lua lua, TEntity entity, TEntityWrapper entityWrapper, string tableName)
        {
            Entity = entity;
            EntityWrapper = entityWrapper;

            _table = lua.GetTable(tableName);
        }

        protected object[]? CallLuaFunction(string fctName, params object[] args)
        {
            Debug.WriteLine($"DEBUG: NewLuaScript::CallLuaFunction: Entity [{Entity}] fctName [{fctName}]");
            // Don't use 'Entity' neither 'this' as 'self' to avoid giving access to non-public methods and avoid recursive call
            var luaFunction = _table?[fctName] as LuaFunction;
            if (args.Length > 0)
            {
                object[] luaArgs = new object[args.Length + 1];
                luaArgs[0] = EntityWrapper!;
                Array.Copy(args, 0, luaArgs, 1, args.Length);
                return luaFunction?.Call(luaArgs);
            }
            return luaFunction?.Call(EntityWrapper);
        }
    }

}
