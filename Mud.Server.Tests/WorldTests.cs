using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;

namespace Mud.Server.Tests
{
    [TestClass]
    public class WorldTests
    {
        [TestMethod]
        public void AddItem_EachBlueprint_Test()
        {
            World.World world = new World.World();
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint {Id = 1, Name = "room1"}, new Area.Area("Area", 1, 100, "builders", "credits"));
            var itemBlueprintBaseType = typeof(ItemBlueprintBase);
            var itemBlueprintTypes = itemBlueprintBaseType.Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsAssignableFrom(itemBlueprintBaseType));

            foreach (var itemBlueprintType in itemBlueprintTypes)
            {
                var blueprint = Activator.CreateInstance(itemBlueprintType) as ItemBlueprintBase;
                blueprint.Name = "blueprint " + blueprint.GetType().Name;
                if (!(blueprint is ItemCorpseBlueprint))
                {
                    IItem item = world.AddItem(Guid.NewGuid(), blueprint, room);
                    Assert.IsNotNull(item);
                }
            }
        }
    }
}
