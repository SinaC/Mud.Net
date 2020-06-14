using System;
using System.Linq;
using Mud.Container;
using Mud.Server.Blueprints.Area;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;

namespace Mud.POC.TestLua
{
    public abstract class TestLuaBase
    {
        protected IWorld World => DependencyContainer.Current.GetInstance<IWorld>();
        protected IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();

        public abstract void Test();

        protected void CreateWorld()
        {
            World.AddArea(Guid.NewGuid(),  new AreaBlueprint { Name ="area", Builders = "sinac", Credits = "sinac"});
            RoomManager.AddRoomBlueprint(new RoomBlueprint
            {
                Name = "battle room",
                Description = "A battle room",
                Id = 1,
                ExtraDescriptions = null,
                Exits = null,
            });
            World.AddCharacterBlueprint(new CharacterNormalBlueprint
            {
                Name = "supahmob",
                Level = 50,
                Description = "A big bad mob staring at you.",
                Id = 1,
                //Sex = Sex.Male,
                ShortDescription = "Big bad mob",
                LongDescription = "Blah blah blah blah",
                LootTable = null,
                ScriptTableName = "mob1"
            });
            World.AddCharacterBlueprint(new CharacterNormalBlueprint
            {
                Name = "weakmob",
                Level = 50,
                Description = "A weak mob.",
                Id = 2,
                //Sex = Sex.Male,
                ShortDescription = "weak mob",
                LongDescription = "Tsekwa",
                LootTable = null,
                ScriptTableName = "mob2"
            });

            IRoom room = RoomManager.AddRoom(Guid.NewGuid(), RoomManager.GetRoomBlueprint(1), World.Areas.First());
            ICharacter bigBadMob = World.AddNonPlayableCharacter(Guid.NewGuid(), World.GetCharacterBlueprint(1), room);
            ICharacter weakMob = World.AddNonPlayableCharacter(Guid.NewGuid(), World.GetCharacterBlueprint(2), room);
        }
    }
}
