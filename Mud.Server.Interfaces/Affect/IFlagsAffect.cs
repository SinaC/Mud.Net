using Mud.DataStructures.Flags;
using Mud.Domain;

namespace Mud.Server.Interfaces.Affect;

public interface IFlagsAffect<TFlag, TFlagValues> : IAffect
    where TFlag : IFlags<string, TFlagValues>
    where TFlagValues: IFlagValues<string>
{
    AffectOperators Operator { get; set; } // Add and Or are identical
    TFlag Modifier { get; set; }
}