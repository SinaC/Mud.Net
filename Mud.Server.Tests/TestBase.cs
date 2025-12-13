using Microsoft.Extensions.Logging;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Item;
using Mud.Server.Options;
using Mud.Server.Random;
using Mud.Server.Tests.Mocking;

namespace Mud.Server.Tests
{
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
                CharacterFlags = CreateCharacterFlags(characterFlags),
                ActFlags = CreateActFlags(),
                OffensiveFlags = CreateOffensiveFlags(),
                Immunities = CreateIRV(),
                Resistances = CreateIRV(),
                Vulnerabilities = CreateIRV(),
                ShieldFlags = CreateShieldFlags(),
            };

            return GenerateNPC(blueprint, room);
        }

        protected static INonPlayableCharacter GenerateNPC(CharacterNormalBlueprint blueprint, IRoom room)
        {
            var loggerMock = new Mock<ILogger<NonPlayableCharacter>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var flagFactoryMock = new Mock<IFlagFactory>();
            var classManagerMock = new Mock<IClassManager>();
            var raceManagerMock = new Mock<IRaceManager>();
            var damageModifierManagerMock = new Mock<IDamageModifierManager>();
            var randomManagerMock = new Mock<IRandomManager>();
            var tableValuesMock = new Mock<ITableValues>();

            classManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IClass>().Object);
            raceManagerMock.SetupGet(x => x[It.IsAny<string>()]).Returns(new Mock<IRace>().Object);
            tableValuesMock.Setup(x => x.DefensiveBonus(It.IsAny<ICharacter>())).Returns(blueprint.Level);
            tableValuesMock.Setup(x => x.HitBonus(It.IsAny<ICharacter>())).Returns(blueprint.Level-5);
            tableValuesMock.Setup(x => x.DamBonus(It.IsAny<ICharacter>())).Returns(blueprint.Level-20);
            flagFactoryMock.Setup(x => x.CreateInstance<ICharacterFlags, ICharacterFlagValues>(It.IsAny<string[]>())).Returns<string[]>(CreateCharacterFlags);
            flagFactoryMock.Setup(x => x.CreateInstance<IBodyForms, IBodyFormValues>(It.IsAny<string[]>())).Returns<string[]>(CreateBodyForms);
            flagFactoryMock.Setup(x => x.CreateInstance<IBodyParts, IBodyPartValues>(It.IsAny<string[]>())).Returns<string[]>(CreateBodyParts);
            flagFactoryMock.Setup(x => x.CreateInstance<IShieldFlags, IShieldFlagValues>(It.IsAny<string[]>())).Returns<string[]>(CreateShieldFlags);
            flagFactoryMock.Setup(x => x.CreateInstance<IIRVFlags, IIRVFlagValues>(It.IsAny<string[]>())).Returns<string[]>(CreateIRV);
            flagFactoryMock.Setup(x => x.CreateInstance<IActFlags, IActFlagValues>(It.IsAny<string[]>())).Returns<string[]>(CreateActFlags);
            flagFactoryMock.Setup(x => x.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>(It.IsAny<string[]>())).Returns<string[]>(CreateOffensiveFlags);
            flagFactoryMock.Setup(x => x.CreateInstance<IAssistFlags, IAssistFlagValues>(It.IsAny<string[]>())).Returns<string[]>(CreateAssistFlags);

            var npc = new NonPlayableCharacter(loggerMock.Object, null, null, null, messageForwardOptions, randomManagerMock.Object, tableValuesMock.Object, null, null, null, null, null, null, raceManagerMock.Object, classManagerMock.Object, damageModifierManagerMock.Object, null, flagFactoryMock.Object, null, null, null);
            npc.Initialize(Guid.NewGuid(), blueprint, room);

            return npc;
        }

        protected static IItemWeapon GenerateWeapon(string itemFlags, string weaponFlags, IRoom room)
        {
            var loggerMock = new Mock<ILogger<ItemWeapon>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var itemFlagsFactory = new Mock<IFlagFactory<IItemFlags, IItemFlagValues>>();
            var weaponFlagsFactory = new Mock<IFlagFactory<IWeaponFlags, IWeaponFlagValues>>();

            itemFlagsFactory.Setup(x => x.CreateInstance(It.IsAny<string[]>())).Returns<string[]>(CreateItemFlags);
            weaponFlagsFactory.Setup(x => x.CreateInstance(It.IsAny<string[]>())).Returns<string[]>(CreateWeaponFlags);

            var weaponBlueprint = new ItemWeaponBlueprint
            {
                Name = "weapon",
                ShortDescription = "weapon",
                Level = 1,
                ItemFlags = CreateItemFlags(itemFlags),
                Flags = CreateWeaponFlags(weaponFlags),
            };

            var weapon = new ItemWeapon(loggerMock.Object, null, null, null, messageForwardOptions, null, null, null, itemFlagsFactory.Object, weaponFlagsFactory.Object);
            weapon.Initialize(Guid.NewGuid(), weaponBlueprint, room);

            return weapon;
        }

        protected static IItemArmor GenerateArmor(string itemFlags, IRoom room)
        {
            var loggerMock = new Mock<ILogger<ItemArmor>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var itemFlagsFactory = new Mock<IFlagFactory<IItemFlags, IItemFlagValues>>();

            itemFlagsFactory.Setup(x => x.CreateInstance(It.IsAny<string[]>())).Returns<string[]>(CreateItemFlags);

            var armorBlueprint = new ItemArmorBlueprint
            {
                Name = "armor",
                ShortDescription = "armor",
                Level = 1,
                ItemFlags = CreateItemFlags(itemFlags),
            };

            var armor = new ItemArmor(loggerMock.Object, null, null, null, messageForwardOptions, null, null, itemFlagsFactory.Object);
            armor.Initialize(Guid.NewGuid(), armorBlueprint, room);

            return armor;
        }

        protected static IRoom GenerateRoom(string roomFlags)
        {
            var loggerMock = new Mock<ILogger<Room.Room>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var roomFlagsFactory = new Mock<IFlagFactory<IRoomFlags, IRoomFlagValues>>();
            var areaMock = new Mock<IArea>();

            roomFlagsFactory.Setup(x => x.CreateInstance(It.IsAny<string[]>())).Returns<string[]>(CreateRoomFlags);

            var roomBlueprint = new RoomBlueprint
            {
                Name = "room",
                RoomFlags = CreateRoomFlags(roomFlags),
            };

            var room = new Room.Room(loggerMock.Object, null, null, null, messageForwardOptions, null, roomFlagsFactory.Object);
            room.Initialize(Guid.NewGuid(), roomBlueprint, areaMock.Object);

            return room;
        }

        protected static ICharacterFlags CreateCharacterFlags(params string[] flags)
        {
            var characterFlags = new CharacterFlags(new Rom24CharacterFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        characterFlags.Set(flag);
                    }
                }
            }
            return characterFlags;
        }

        protected static IIRVFlags CreateIRV(params string[] flags)
        {
            var irv = new IRVFlags(new Rom24IRVFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        irv.Set(flag);
                    }
                }
            }
            return irv;
        }

        protected static IBodyForms CreateBodyForms(params string[] flags)
        {
            var bodyForms = new BodyForms(new Rom24BodyFormValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        bodyForms.Set(flag);
                    }
                }
            }
            return bodyForms;
        }

        protected static IBodyParts CreateBodyParts(params string[] flags)
        {
            var bodyParts = new BodyParts(new Rom24BodyPartValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        bodyParts.Set(flag);
                    }
                }
            }
            return bodyParts;
        }

        protected static IShieldFlags CreateShieldFlags(params string[] flags)
        {
            var ShieldFlags = new ShieldFlags(new Rom24ShieldFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        ShieldFlags.Set(flag);
                    }
                }
            }
            return ShieldFlags;
        }

        protected static IActFlags CreateActFlags(params string[] flags)
        {
            var ActFlags = new ActFlags(new Rom24ActFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        ActFlags.Set(flag);
                    }
                }
            }
            return ActFlags;
        }

        protected static IOffensiveFlags CreateOffensiveFlags(params string[] flags)
        {
            var OffensiveFlags = new OffensiveFlags(new Rom24OffensiveFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        OffensiveFlags.Set(flag);
                    }
                }
            }
            return OffensiveFlags;
        }

        protected static IAssistFlags CreateAssistFlags(params string[] flags)
        {
            var AssistFlags = new AssistFlags(new Rom24AssistFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        AssistFlags.Set(flag);
                    }
                }
            }
            return AssistFlags;
        }

        protected static IItemFlags CreateItemFlags(params string[] flags)
        {
            var itemFlags = new ItemFlags(new Rom24ItemFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        itemFlags.Set(flag);
                    }
                }
            }
            return itemFlags;
        }

        protected static IWeaponFlags CreateWeaponFlags(params string[] flags)
        {
            var weaponFlags = new WeaponFlags(new Rom24WeaponFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        weaponFlags.Set(flag);
                    }
                }
            }
            return weaponFlags;
        }

        protected static IRoomFlags CreateRoomFlags(params string[] flags)
        {
            var roomFlags = new RoomFlags(new Rom24RoomFlagValues());
            if (flags.Length > 0)
            {
                foreach (var flag in flags)
                {
                    if (!string.IsNullOrWhiteSpace(flag))
                    {
                        roomFlags.Set(flag);
                    }
                }
            }
            return roomFlags;
        }
    }
}
