using System;
using System.Linq;
using Mud.Container;
using Mud.Domain;
using Mud.Server;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;

namespace Mud.POC.TestLua
{
    public abstract class TestLuaBase
    {
        protected IWorld World => DependencyContainer.Current.GetInstance<IWorld>();

        public abstract void Test();

        protected void CreateWorld()
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
            World.AddCharacterBlueprint(new CharacterNormalBlueprint
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
            World.AddCharacterBlueprint(new CharacterNormalBlueprint
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
