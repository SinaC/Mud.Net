﻿using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Affect
{
    public interface IItemFlagsAffect : IFlagsAffect<IItemFlags, IItemFlagValues>, IItemAffect<IItem>
    {
    }
}
