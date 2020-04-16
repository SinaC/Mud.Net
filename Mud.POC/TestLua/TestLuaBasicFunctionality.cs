using System;
using System.Linq;
using Mud.Domain;
using Mud.Server;
using Mud.Server.Area;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character;
using Mud.Server.Room;
using NLua;

namespace Mud.POC.TestLua
{
    public class TestLuaBasicFunctionality : TestLuaBase
    {
        public ICharacter GetCharacter() // referenced here (***)
        {
            return World.Characters.First();
        }

        public interface ITestCharacter
        {
            void Act(string action);
        }

        public class TestCharacter : Character, ITestCharacter
        {
            public void Act(string action)
            {
            }

            public TestCharacter()
                : base(Guid.NewGuid(), new CharacterData { Name = "test", Class = "Thief", Race = "Dwarf" }, new Room(Guid.NewGuid(), new RoomBlueprint { Name = "test" }, new Area("area", 1, 99, "buiders", "credits")))
            {
            }

            public static ITestCharacter Create()
            {
                return new TestCharacter();
            }
        }

        public override void Test()
        {
            Lua lua = new Lua();

            ////
            //TestLuaFunctions testLuaFunctions = new TestLuaFunctions();
            //lua.RegisterFunction("Say", testLuaFunctions, testLuaFunctions.GetType().GetMethod("Say"));
            //lua.RegisterFunction("CreateInstance", typeof(TestLuaFunctions).GetMethod("CreateInstance"));
            //lua.DoString("Say('pouet')");
            //lua.DoString("instance = CreateInstance()");
            //lua.DoString("instance:Say('woohoo')");

            //TestLuaFunctions externalInstance = (TestLuaFunctions) lua["instance"];
            //externalInstance.Say("using externally created instance to call Say");

            //
            CreateWorld();

            ITestCharacter testCharacter = TestCharacter.Create();
            lua["testcharacter"] = testCharacter;

            lua.RegisterFunction("print", typeof(LuaOutput).GetMethod("Print"));
            //lua["this"] = bigBadMob;
            //lua["room"] = World.Rooms.First();
            lua.RegisterFunction("getCharacter", this, GetType().GetMethod("GetCharacter")); // here (***)
            lua.DoString(
            @"print('this is a debug message')
            local this = getCharacter()
            cName = this.DisplayName
            function luanet.each(o)
               local e = o:GetEnumerator()
               return function()
                  if e:MoveNext() then
                    return e.Current
                 end
               end
            end

            local each = luanet.each;
            
            for c in each(this.Room.People) do
                local name = c == getCharacter() and 'me' or c.DisplayName;
                print('in room:'..name);
            end

           -- local globals = _G
            --for g in each(globals) do
            --    print('global:'..tostring(g));
            --end");
            //doesn't work    lua.DoString("function this.pouet(s) print(s) end");
            var cName = lua["cName"];
            var luanet = lua["luanet"];
            var globals = lua["_G"];
            var testCharacter2 = lua["testcharacter"];

            lua.Close();
        }
    }
}
