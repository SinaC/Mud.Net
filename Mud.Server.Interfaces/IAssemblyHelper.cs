using System.Collections.Generic;
using System.Reflection;

namespace Mud.Server.Interfaces
{
    public interface IAssemblyHelper
    {
        IEnumerable<Assembly> AllReferencedAssemblies { get; }
        //var asm = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetReferencedAssemblies()).DistinctBy(x => x.FullName);
    }
}
