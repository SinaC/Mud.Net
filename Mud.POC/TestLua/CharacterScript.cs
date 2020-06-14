using System.Diagnostics;
using Mud.Domain;
using Mud.Server;
using NLua;

namespace Mud.POC.TestLua
{
    public class CharacterScript : LuaScript<ICharacter>
    {
        public ICharacter Character => Entity;

        public CharacterScript(Lua lua, ICharacter character, string tableName)
            : base(lua, character, tableName)
        {
        }

        // Triggers
        public void OnTick()
        {
            Debug.WriteLine($"DEBUG: CharacterScript::OnTick: Entity [{Entity.DebugName}]");
            LuaFunction luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity);
        }

        public void OnSay(ICharacter actor, string message)
        {
            Debug.WriteLine($"DEBUG: CharacterScript::OnSay: Entity [{Entity.DebugName}]");
            LuaFunction luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity, actor, message);
        }

        public void OnGreet(ICharacter actor, ExitDirections from)
        {
            Debug.WriteLine($"DEBUG: CharacterScript::OnGreet: Entity [{Entity.DebugName}]");
            LuaFunction luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity, actor, from);
        }
    }
}
