using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Blueprints
{
    public class ItemArmorBlueprint : ItemBlueprintBase
    {
        public int Pierce { get; set; }
        public int Bash { get; set; }
        public int Slash { get; set; }
        public int Exotic { get; set; }
    }
}
