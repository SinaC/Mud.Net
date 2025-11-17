using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using NLua;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mud.POC.TestLua
{
    public class TestLuaIntegration : TestLuaBase
    {
        public TestLuaIntegration(IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
            : base(areaManager, roomManager, characterManager, itemManager)
        {
        }

        public override void Test()
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
@"import = function () end -- avoid end-user importing external modules
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
            foreach (INonPlayableCharacter character in CharacterManager.NonPlayableCharacters.Where(x => x.Blueprint.ScriptTableName != null))
            {
                string scriptName = character.Blueprint.ScriptTableName;
                var mobScript = lua[scriptName];
                if (mobScript != null)
                {
                    CharacterScript script = new CharacterScript(lua, character, scriptName);
                    scripts.Add(script);
                }
            }

            //
            ICharacter supahmob = CharacterManager.Characters.First();
            CharacterScript supahmobScript = scripts.FirstOrDefault(x => x.Character == supahmob);
            ICharacter weakmob = CharacterManager.Characters.Skip(1).First();
            CharacterScript weakmobScript = scripts.FirstOrDefault(x => x.Character == weakmob);

            // Call script from appropriate functions in Server (OnTick)
            foreach (ICharacter character in CharacterManager.Characters)
            {
                CharacterScript script = scripts.FirstOrDefault(x => x.Character == character);
                script?.OnTick();
            }

            supahmobScript?.OnSay(weakmob, "woot");
            weakmobScript?.OnGreet(supahmob, 5);

            var mob1InLua = lua["mob1"];
            Debug.WriteLine("mob1InLua: " + mob1InLua.GetType());

            lua.Close();
        }
    }
}