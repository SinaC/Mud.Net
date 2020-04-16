using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Mud.Container;
using Mud.Domain;
using Mud.Server;
using Mud.Server.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character;
using Mud.Server.Room;
using NLua;
using Lua = NLua.Lua;

namespace Mud.POC
{
    public class TestLuaFunctions
    {
        public void Say(string msg)
        {
            Debug.WriteLine($"Say:[{msg}]");
        }

        public static TestLuaFunctions CreateInstance()
        {
            return new TestLuaFunctions();
        }
    }

    public class LuaOutput
    {
        public static void Print(string msg)
        {
            Debug.WriteLine(msg);
        }
    }

    public abstract class LuaScript<TEntity>
        where TEntity : IEntity
    {
        private readonly LuaTable _table;

        protected readonly Lua Lua;
        protected readonly TEntity Entity;

        protected LuaScript(Lua lua, TEntity entity, string tableName)
        {
            Lua = lua;
            Entity = entity;

            _table = Lua.GetTable(tableName);
        }

        protected LuaFunction GetLuaFunction([CallerMemberName] string memberName = null)
        {
            LuaFunction function = _table?[memberName] as LuaFunction;
            return function;
        }
    }

    public class CharacterScript : LuaScript<ICharacter>
    {
        public ICharacter Character => Entity;

        public CharacterScript(Lua lua, ICharacter character, string tableName)
            :base(lua, character, tableName)
        {
        }

        public void OnTick()
        {
            LuaFunction function = GetLuaFunction();
            function?.Call(Character);
        }

        public void OnSay(ICharacter actor, string message)
        {
            LuaFunction function = GetLuaFunction();
             function?.Call(Character, actor, message);
        }

        public void OnGreet(ICharacter actor, ExitDirections from)
        {
            LuaFunction function = GetLuaFunction();
            object[] returnedValues = function?.Call(Character, actor, from);
        }
    }

    public class TestLua
    {
        private IWorld World => DependencyContainer.Instance.GetInstance<IWorld>();

        public ICharacter GetCharacter()
        {
            return World.Characters.First();
        }

        public void TestIntegration()
        {
            CreateWorld();

            Lua lua = new Lua();
            lua.RegisterFunction("print", typeof(LuaOutput).GetMethod("Print"));

            //// Create Lua table for each blueprint script table name
            //foreach (CharacterBlueprint blueprint in World.CharacterBlueprints.Where(x => x.ScriptTableName != null))
            //{
            //    if (lua.GetTable(blueprint.ScriptTableName) == null)
            //        lua.NewTable(blueprint.ScriptTableName);
            //}

            // Read scripts from file/immediate string
            lua.DoString(
@"
mob1 = {}
function mob1:OnTick()
    print('OnTick:['..self.DisplayName..']  '..tostring(self));
    --print('DISPLAY _G');
    --for n in pairs(_G) do print(n) end
    --for n in pairs(self) do print(n) end
end
function mob1:OnSay(actor, msg)
    print('OnSay:['..self.DisplayName..'] heards ['..actor.DisplayName..'] saying ['..msg..']');
end

mob2 = {}
function mob2:OnTick()
    print('OnTick:'..self.DebugName);
end
function mob2:OnGreet(actor, from)
    print('OnGreet: ['..self.DebugName..'] saw ['..actor.DebugName..'] entering room from ['..tostring(from)..']');
    actor:GainExperience(10000000); -- <-- this should be forbidden, only a few information such as DisplayName, Room, ... should be available
end");

            // TODO: replace 'scripts' with parameter in ICharacter and initialize this in AddCharacter or in Character ctor
            List<CharacterScript> scripts = new List<CharacterScript>();
            foreach (ICharacter character in World.Characters.Where(x => x.Blueprint.ScriptTableName != null))
            {
                string scriptName = character.Blueprint.ScriptTableName;
                var mobScript = lua[scriptName];
                if (mobScript != null)
                {
                    CharacterScript script = new CharacterScript(lua, character, scriptName);
                    scripts.Add(script);
                }
            }


            ICharacter supahmob = World.Characters.First();
            CharacterScript supahmobScript = scripts.FirstOrDefault(x => x.Character == supahmob);
            ICharacter weakmob = World.Characters.Skip(1).First();
            CharacterScript weakmobScript = scripts.FirstOrDefault(x => x.Character == weakmob);

            // Call script from appropriate functions in Server
            foreach (ICharacter character in World.Characters)
            {
                CharacterScript script = scripts.FirstOrDefault(x => x.Character == character);
                script?.OnTick();
            }

            supahmobScript?.OnSay(weakmob, "woot");
            weakmobScript?.OnGreet(supahmob, ExitDirections.SouthWest);

            var mob1InLua = lua["mob1"];
            Debug.WriteLine("mob1InLua: " + mob1InLua.GetType());

            lua.Close();
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
                :base(Guid.NewGuid(), new CharacterData {Name = "test", Class="Thief", Race="Dwarf"}, new Room(Guid.NewGuid(), new RoomBlueprint {Name = "test"}, new Area("area" , 1, 99, "buiders", "credits")))
            {
            }

            public static ITestCharacter Create()
            {
                return new TestCharacter();
            }
        }

        public void TestBasicFunctionality()
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
            lua.RegisterFunction("getCharacter", this, GetType().GetMethod("GetCharacter"));
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

        private void CreateWorld()
        {
            World.AddArea(Guid.NewGuid(), "area", 1, 100, "sinac", "sinac");
            World.AddRoomBlueprint(new RoomBlueprint
            {
                Name = "battle room",
                Description = "A battle room",
                Id = 1,
                ExtraDescriptions = null,
                Exits = null,
            });
            World.AddCharacterBlueprint(new CharacterBlueprint
            {
                Name = "supahmob",
                Level = 50,
                Description = "A big bad mob staring at you.",
                Id = 1,
                Sex = Sex.Male,
                ShortDescription = "Big bad mob",
                LongDescription = "Blah blah blah blah",
                LootTable = null,
                ScriptTableName = "mob1"
            });
            World.AddCharacterBlueprint(new CharacterBlueprint
            {
                Name = "weakmob",
                Level = 50,
                Description = "A weak mob.",
                Id = 2,
                Sex = Sex.Male,
                ShortDescription = "weak mob",
                LongDescription = "Tsekwa",
                LootTable = null,
                ScriptTableName = "mob2"
            });

            World.AddRoom(Guid.NewGuid(), World.GetRoomBlueprint(1), World.Areas.First());
            ICharacter bigBadMob = World.AddCharacter(Guid.NewGuid(), World.GetCharacterBlueprint(1), World.Rooms.First());
            ICharacter weakMob = World.AddCharacter(Guid.NewGuid(), World.GetCharacterBlueprint(2), World.Rooms.First());
        }
    }
}