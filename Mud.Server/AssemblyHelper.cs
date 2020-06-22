using Mud.Common;
using Mud.Server.Interfaces;
using System.Collections.Generic;
using System.Reflection;

namespace Mud.Server
{
    public class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies => Assembly.GetExecutingAssembly().Yield();
    }
}
