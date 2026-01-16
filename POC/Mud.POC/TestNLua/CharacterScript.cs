using System.Diagnostics;
using Mud.Server.Interfaces.Character;

namespace Mud.POC.TestNLua
{
    public class CharacterScript : LuaScript<ICharacter>
    {
        public ICharacter Character => Entity;

        public CharacterScript(NLua.Lua lua, ICharacter character, string tableName)
            : base(lua, character, tableName)
        {
        }

        // Triggers
        public void OnTick()
        {
            Debug.WriteLine($"DEBUG: CharacterScript::OnTick: Entity [{Entity.DebugName}]");
            var luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity);
        }

        public void OnSay(ICharacter actor, string message)
        {
            Debug.WriteLine($"DEBUG: CharacterScript::OnSay: Entity [{Entity.DebugName}]");
            var luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity, actor, message);
        }

        public void OnGreet(ICharacter actor, int from)
        {
            Debug.WriteLine($"DEBUG: CharacterScript::OnGreet: Entity [{Entity.DebugName}]");
            var luaFunction = GetLuaFunction();
            luaFunction?.Call(Entity, actor, from);
        }
    }
}
