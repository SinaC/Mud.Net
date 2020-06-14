using System;
using System.Collections.Generic;

namespace Mud.Server.Blueprints.Character
{
    public class CharacterShopBlueprint : CharacterBlueprintBase
    {
        public List<Type> BuyBlueprintTypes { get; set; } // Type must implement ItemBlueprintBase
        public int ProfitBuy { get; set; }
        public int ProfitSell { get; set; }
        public int OpenHour { get; set; }
        public int CloseHour { get; set; }
    }
}
