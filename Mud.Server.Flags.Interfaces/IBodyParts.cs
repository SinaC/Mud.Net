using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces
{
    public interface IBodyParts : IFlags<string, IBodyPartValues>
    {
    }

    public interface IBodyPartValues : IFlagValues<string> 
    {
    }
}
