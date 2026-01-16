using Mud.DataStructures.Flags;
using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Affect;

public interface IFlagsAffect<TFlag> : IAffect
    where TFlag : IFlags<string>
{
    AffectOperators Operator { get; set; } // Add and Or are identical
    TFlag Modifier { get; set; }
}