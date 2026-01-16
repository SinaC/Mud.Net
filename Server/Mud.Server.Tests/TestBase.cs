using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.Room;
using Mud.Domain;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Flags;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Item;
using Mud.Server.Options;
using Mud.Random;

namespace Mud.Server.Tests;

[TestClass]
public abstract class TestBase
{
    protected static INonPlayableCharacter GenerateNPC(string characterFlags, IRoom room)
    {
        var blueprint = new CharacterNormalBlueprint
        {
            Name = "NPC",
            Race = "Human",
            Class = "Warrior",
            Level = 6,
            Sex = Sex.Male,
            Size = Sizes.Medium,
            CharacterFlags = new CharacterFlags(characterFlags),
            ActFlags = new ActFlags(),
            OffensiveFlags = new OffensiveFlags(),
            Immunities = new IRVFlags(),
            Resistances = new IRVFlags(),
            Vulnerabilities = new IRVFlags(),
            ShieldFlags = new ShieldFlags(),
        };

        return GenerateNPC(blueprint, room);
    }

    protected static INonPlayableCharacter GenerateNPC(CharacterNormalBlueprint blueprint, IRoom room)
    {
        var loggerMock = new Mock<ILogger<NonPlayableCharacter>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var classManagerMock = new Mock<IClassManager>();
        var raceManagerMock = new Mock<IRaceManager>();
        var resistanceCalculatorMock = new Mock<IResistanceCalculator>();
        var randomManagerMock = new Mock<IRandomManager>();
        var tableValuesMock = new Mock<ITableValues>();

        classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
        raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);
        tableValuesMock.Setup(x => x.DefensiveBonus(It.IsAny<ICharacter>())).Returns(blueprint.Level);
        tableValuesMock.Setup(x => x.HitBonus(It.IsAny<ICharacter>())).Returns(blueprint.Level-5);
        tableValuesMock.Setup(x => x.DamBonus(It.IsAny<ICharacter>())).Returns(blueprint.Level-20);

        var npc = new NonPlayableCharacter(loggerMock.Object, null!, null!, messageForwardOptions, null!, randomManagerMock.Object, tableValuesMock.Object, null!, null!, null!, null!, null!, null!, null!, raceManagerMock.Object, classManagerMock.Object, resistanceCalculatorMock.Object, null!, null!, null!, null!);
        npc.Initialize(Guid.NewGuid(), blueprint, room);

        return npc;
    }

    protected static IItemWeapon GenerateWeapon(string itemFlags, string weaponFlags, IRoom room)
    {
        var loggerMock = new Mock<ILogger<ItemWeapon>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 60, BlueprintIds = null! });

        var weaponBlueprint = new ItemWeaponBlueprint
        {
            Name = "weapon",
            ShortDescription = "weapon",
            Level = 1,
            ItemFlags = new ItemFlags(itemFlags),
            Flags = new WeaponFlags(weaponFlags),
        };

        var weapon = new ItemWeapon(loggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, null!, null!, null!, null!);
        weapon.Initialize(Guid.NewGuid(), weaponBlueprint, room);

        return weapon;
    }

    protected static IItemArmor GenerateArmor(string itemFlags, IRoom room)
    {
        var loggerMock = new Mock<ILogger<ItemArmor>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 60, BlueprintIds = null! });

        var armorBlueprint = new ItemArmorBlueprint
        {
            Name = "armor",
            ShortDescription = "armor",
            Level = 1,
            ItemFlags = new ItemFlags(itemFlags),
        };

        var armor = new ItemArmor(loggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, null!, null!);
        armor.Initialize(Guid.NewGuid(), armorBlueprint, room);

        return armor;
    }

    protected static IRoom GenerateRoom(string roomFlags)
    {
        var loggerMock = new Mock<ILogger<Room.Room>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var areaMock = new Mock<IArea>();

        var roomBlueprint = new RoomBlueprint
        {
            Name = "room",
            RoomFlags = new RoomFlags(roomFlags),
        };

        var room = new Room.Room(loggerMock.Object, null!, null!, messageForwardOptions, null!);
        room.Initialize(Guid.NewGuid(), roomBlueprint, areaMock.Object);

        return room;
    }

}
