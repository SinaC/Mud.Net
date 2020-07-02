﻿using System.Collections.Generic;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;

namespace Mud.Server.Race
{
    public abstract class RaceBase : IRace
    {
        #region IRace

        public abstract string Name { get; }

        public string DisplayName => Name.UpperFirstLetter();

        public abstract Sizes Size { get; }

        public abstract ICharacterFlags CharacterFlags { get; }

        public abstract IIRVFlags Immunities { get; }
        public abstract IIRVFlags Resistances { get; }
        public abstract IIRVFlags Vulnerabilities { get; }

        public abstract IEnumerable<EquipmentSlots> EquipmentSlots { get; }

        public abstract IBodyForms BodyForms { get; }
        public abstract IBodyParts BodyParts { get; }

        public abstract IActFlags ActFlags { get; }
        public abstract IOffensiveFlags OffensiveFlags { get; }
        public abstract IAssistFlags AssistFlags { get; }

        #endregion
    }
}
