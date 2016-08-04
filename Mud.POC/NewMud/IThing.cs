using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.NewMud
{
    public interface IThing
    {
        string Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }
    }
}
