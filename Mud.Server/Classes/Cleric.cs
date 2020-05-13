﻿using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Cleric : ClassBase
    {
        #region IClass

        public override string Name => "cleric";

        public override string ShortName => "Cle";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Mana
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds; // always mana
        }

        public override BasicAttributes PrimeAttribute => BasicAttributes.Wisdom;

        public override int MaxPracticePercentage => 75;

        public override int MinHitPointGainPerLevel => 7;

        public override int MaxHitPointGainPerLevel => 10;

        #endregion

        public Cleric()
        {
        }
    }
}