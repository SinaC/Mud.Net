﻿using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class ClassManagerMock : IClassManager
    {
        public IClass this[string name] => new ClassMock(name);

        public IEnumerable<IClass> Classes => throw new NotImplementedException();
    }

    internal class ClassMock : IClass
    {
        public ClassMock(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string DisplayName => Name;

        public string ShortName => Name;

        public IEnumerable<ResourceKinds> ResourceKinds => Enumerable.Empty<ResourceKinds>();

        public IEnumerable<IAbilityUsage> Abilities => Enumerable.Empty<IAbilityUsage>();

        public int MaxPracticePercentage => throw new NotImplementedException();

        public (int thac0_00, int thac0_32) Thac0 => throw new NotImplementedException();

        public int MinHitPointGainPerLevel => throw new NotImplementedException();

        public int MaxHitPointGainPerLevel => throw new NotImplementedException();

        public BasicAttributes PrimeAttribute => throw new NotImplementedException();

        public IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form) => Enumerable.Empty<ResourceKinds>();

        public int GetAttributeByLevel(CharacterAttributes attribute, int level) => level;
    }
}
