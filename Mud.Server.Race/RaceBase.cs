﻿using System.Collections.Generic;
using Mud.Common;
using Mud.DataStructures.Flags;
using Mud.Domain;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Race
{
    public abstract class RaceBase : IRace
    {
        #region IRace

        public abstract string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public abstract Sizes Size { get; }

        public abstract Flags CharacterFlags { get; }

        public abstract IRVFlags Immunities { get; }
        public abstract IRVFlags Resistances { get; }
        public abstract IRVFlags Vulnerabilities { get; }

        public abstract IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        public abstract BodyForms BodyForms { get; }
        public abstract BodyParts BodyParts { get; }

        public abstract ActFlags ActFlags { get; }
        public abstract OffensiveFlags OffensiveFlags { get; }
        public abstract AssistFlags AssistFlags { get; }

        #endregion
    }
}
