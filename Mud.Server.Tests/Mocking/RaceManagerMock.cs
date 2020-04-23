using Mud.Domain;
using Mud.Server.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class RaceManagerMock : IRaceManager
    {
        public IRace this[string name] => new RaceMock(name);

        public IEnumerable<IRace> Races => throw new NotImplementedException();
    }

    internal class RaceMock : IRace
    {
        public RaceMock(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string DisplayName => Name;

        public string ShortName => Name;

        public IEnumerable<AbilityAndLevel> Abilities => Enumerable.Empty<AbilityAndLevel>();

        public IEnumerable<EquipmentSlots> EquipmentSlots => Enumerable.Empty<EquipmentSlots>();

        public int GetPrimaryAttributeModifier(PrimaryAttributeTypes primaryAttribute) => 10;
    }
}
