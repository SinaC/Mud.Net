﻿using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Class;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Rom24.Classes
{
    public class Warrior : ClassBase
    {
        public Warrior(IAbilityManager abilityManager)
            : base(abilityManager)
        {
        }

        #region IClass

        public override string Name => "warrior";

        public override string ShortName => "War";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = Enumerable.Empty<ResourceKinds>();

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds;
        }

        public override BasicAttributes PrimeAttribute => BasicAttributes.Strength;

        public override int MaxPracticePercentage => 75;

        public override (int thac0_00, int thac0_32) Thac0 => (20, -10);

        public override int MinHitPointGainPerLevel => 11;

        public override int MaxHitPointGainPerLevel => 15;

        #endregion
    }
}
