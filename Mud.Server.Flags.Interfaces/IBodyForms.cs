using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces;

public interface IBodyForms : IFlags<string, IBodyFormValues>
{
}

public interface IBodyFormValues : IFlagValues<string>
{
}
