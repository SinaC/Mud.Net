using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public interface IEntity : IActor
    {
        Guid Id { get; }
        string Name { get; }
        string Description { get; }
        bool Impersonable { get; }
    }
}
