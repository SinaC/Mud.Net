using Mud.Blueprints.Area;
using Mud.Blueprints.Character;
using Mud.Blueprints.Room;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.POC.TestNLua
{
    public abstract class TestLuaBase
    {
        protected IAreaManager AreaManager { get; }
        protected IRoomManager RoomManager { get; }
        protected ICharacterManager CharacterManager { get; }
        protected IItemManager ItemManager { get; }

        protected TestLuaBase(IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
        {
            AreaManager = areaManager;
            RoomManager = roomManager;
            CharacterManager = characterManager;
            ItemManager = itemManager;
        }

        public abstract void Test();

        protected void CreateWorld()
        {
            AreaManager.AddArea(Guid.NewGuid(),  new AreaBlueprint { Name ="area", Builders = "sinac", Credits = "sinac"});
            RoomManager.AddRoomBlueprint(new RoomBlueprint
            {
                Name = "battle room",
                Description = "A battle room",
                Id = 1,
                ExtraDescriptions = null!,
                Exits = null!,
            });
            CharacterManager.AddCharacterBlueprint(new CharacterNormalBlueprint
            {
                Name = "supahmob",
                Level = 50,
                Description = "A big bad mob staring at you.",
                Id = 1,
                //Sex = Sex.Male,
                ShortDescription = "Big bad mob",
                LongDescription = "Blah blah blah blah",
                LootTable = null!,
                ScriptTableName = "mob1"
            });
            CharacterManager.AddCharacterBlueprint(new CharacterNormalBlueprint
            {
                Name = "weakmob",
                Level = 50,
                Description = "A weak mob.",
                Id = 2,
                //Sex = Sex.Male,
                ShortDescription = "weak mob",
                LongDescription = "Tsekwa",
                LootTable = null!,
                ScriptTableName = "mob2"
            });

            IRoom room = RoomManager.AddRoom(Guid.NewGuid(), RoomManager.GetRoomBlueprint(1)!, AreaManager.Areas.First());
            ICharacter bigBadMob = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), CharacterManager.GetCharacterBlueprint(1)!, room);
            ICharacter weakMob = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), CharacterManager.GetCharacterBlueprint(2)!, room);
        }
    }
}
