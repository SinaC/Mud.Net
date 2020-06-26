using Mud.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Ability;
using Mud.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Flags;

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

        public IEnumerable<IAbilityUsage> Abilities => Enumerable.Empty<IAbilityUsage>();

        public IEnumerable<EquipmentSlots> EquipmentSlots => EnumHelpers.GetValues<EquipmentSlots>().SelectMany(x => Enumerable.Repeat(x, 2)); // two of each

        public BodyForms BodyForms => BodyForms.Amphibian;

        public BodyParts BodyParts => BodyParts.Fins;

        public Sizes Size => Sizes.Medium;

        public ICharacterFlags CharacterFlags => new CharacterFlags();

        public IRVFlags Immunities => IRVFlags.None;

        public IRVFlags Resistances =>IRVFlags.None;

        public IRVFlags Vulnerabilities => IRVFlags.None;

        public ActFlags ActFlags => ActFlags.None;

        public OffensiveFlags OffensiveFlags => OffensiveFlags.None;

        public AssistFlags AssistFlags => AssistFlags.None;
    }
}
