using Mud.DataStructures.Flags;
using Mud.Domain;

namespace Mud.Server.Interfaces.Affect
{
    public interface IFlagsAffect : IAffect
    {
        AffectOperators Operator { get; set; } // Add and Or are identical
        Flags Modifier { get; set; }
    }
}