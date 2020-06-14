﻿using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server.Classes
{
    public class Thief : ClassBase
    {
        #region IClass

        public override string Name => "thief";

        public override string ShortName => "Thi";

        public override IEnumerable<ResourceKinds> ResourceKinds { get; } = new List<ResourceKinds>
        {
            Domain.ResourceKinds.Energy
        };

        public override IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form)
        {
            return ResourceKinds; // always energy
        }

        public override int GetPrimaryAttributeByLevel(PrimaryAttributeTypes primaryAttribute, int level)
        {
            return level * 10; // TODO: http://wow.gamepedia.com/Base_attributes
        }

        #endregion

        public Thief()
        {
            AddAbility(1, "dodge");
            AddAbility(5, "rupture");
            AddAbility(10, "parry");
        }
    }
}
