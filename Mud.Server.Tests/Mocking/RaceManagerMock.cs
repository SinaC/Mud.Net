using Mud.Domain;
using Mud.Server.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Common;

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

        public IEnumerable<AbilityUsage> Abilities => Enumerable.Empty<AbilityUsage>();

        public IEnumerable<EquipmentSlots> EquipmentSlots => EnumHelpers.GetValues<EquipmentSlots>().SelectMany(x => Enumerable.Repeat(x, 2)); // two of each

        public Sizes Size => Sizes.Medium;

        public IRVFlags Immunities => IRVFlags.None;

        public IRVFlags Resistances =>IRVFlags.None;

        public IRVFlags Vulnerabilities => IRVFlags.None;

        public int ClassExperiencePercentageMultiplier(IClass c) => 100;

        public int GetMaxAttribute(CharacterAttributes attribute) => 13;

        public int GetStartAttribute(CharacterAttributes attribute) => 18;
    }
}
