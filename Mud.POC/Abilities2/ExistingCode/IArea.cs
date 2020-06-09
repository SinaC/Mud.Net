﻿using Mud.POC.Abilities2.Domain;
using System.Collections.Generic;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IArea
    {
        IEnumerable<IRoom> Rooms { get; }
        IEnumerable<ICharacter> Characters { get; }

        AreaFlags AreaFlags { get; }
    }
}