﻿using System.Diagnostics;
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
            Debug.WriteLine($"DEBUG: LuaScript::GetLuaFunction: Entity [{Entity}] fctName [{fctName}]");
            LuaFunction function = _table?[fctName] as LuaFunction;
            return function;
        }
    }
}
