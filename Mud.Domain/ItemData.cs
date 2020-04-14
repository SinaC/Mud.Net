using System.Collections.Generic;

namespace Mud.Domain
{
    public class ItemData
    {
        public int ItemId { get; set; }

        public List<ItemData> Contains { get; set; }

        //TODO: enchantments/auras
    }
}
