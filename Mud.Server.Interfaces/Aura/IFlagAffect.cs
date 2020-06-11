using System;
using Mud.Domain;

namespace Mud.Server.Interfaces.Aura
{
    public interface IFlagAffect<T> : IAffect
        where T:Enum
    {
        AffectOperators Operator { get; set; } // Add and Or are identical
        T Modifier { get; set; }
    }
}