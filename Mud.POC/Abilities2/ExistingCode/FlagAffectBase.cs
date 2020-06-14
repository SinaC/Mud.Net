using Mud.POC.Abilities2.Domain;
using System;

namespace Mud.POC.Abilities2.ExistingCode
{
    public abstract class FlagAffectBase<T> : IAffect
        where T:Enum
    {
        public AffectOperators Operator { get; set; } // Add and Or are identical
        public T Modifier { get; set; }
    }
}