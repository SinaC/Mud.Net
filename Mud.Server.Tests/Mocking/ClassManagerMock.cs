using Mud.Domain;
using Mud.Server.Abilities;
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

        public string ShortName => ShortName;

        public IEnumerable<ResourceKinds> ResourceKinds => Enumerable.Empty<ResourceKinds>();

        public IEnumerable<AbilityAndLevel> Abilities => Enumerable.Empty<AbilityAndLevel>();

        public IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form) => Enumerable.Empty<ResourceKinds>();

        public int GetPrimaryAttributeByLevel(PrimaryAttributeTypes primaryAttribute, int level) => level;
    }
}
