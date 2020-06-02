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
        public IEnumerable<IPlayableRace> PlayableRaces => throw new NotImplementedException();
        public IEnumerable<IRace> Races => throw new NotImplementedException();

        public IRace this[string name] => new RaceMock(name);
    }

    internal class RaceMock : IRace
    {
        public RaceMock(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string DisplayName => Name;

        public IEnumerable<AbilityUsage> Abilities => Enumerable.Empty<AbilityUsage>();

        public IEnumerable<EquipmentSlots> EquipmentSlots => EnumHelpers.GetValues<EquipmentSlots>().SelectMany(x => Enumerable.Repeat(x, 2)); // two of each

        public BodyForms BodyForms => BodyForms.Amphibian;

        public BodyParts BodyParts => BodyParts.Fins;

        public Sizes Size => Sizes.Medium;

        public CharacterFlags CharacterFlags => CharacterFlags.None;

        public IRVFlags Immunities => IRVFlags.None;

        public IRVFlags Resistances =>IRVFlags.None;

        public IRVFlags Vulnerabilities => IRVFlags.None;

        public ActFlags ActFlags => ActFlags.None;

        public OffensiveFlags OffensiveFlags => OffensiveFlags.None;

        public AssistFlags AssistFlags => AssistFlags.None;
    }
}
