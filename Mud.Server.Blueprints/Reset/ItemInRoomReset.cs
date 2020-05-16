using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Blueprints.Reset
{
    public class ItemInRoomReset : ResetBase
    {
        public int ItemId { get; set; }
        public int GlobalLimit { get; set; }
        public int LocalLimit { get; set; }
    }
}
