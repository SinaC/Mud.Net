using System.Reflection;

namespace Mud.Common;

public interface IAssemblyHelper
{
    IEnumerable<Assembly> AllReferencedAssemblies { get; }
    //var asm = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetReferencedAssemblies()).DistinctBy(x => x.FullName);
}
