using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Special;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_janitor")]
    public class Janitor : ISpecialBehavior
    {
        public bool Execute(INonPlayableCharacter npc)
        {
            // must be awake
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Positions.Sleeping)
                return false;

            // pick some trash
            var trash = npc.Room.Content.FirstOrDefault(x => (x is IItemTrash || x is IItemDrinkContainer || x.Cost < 10) && !x.NoTake && npc.CanSee(x) /*&& npc.CanLoot(x)*/);
            if (trash == null)
                return false;

            npc.Act(ActOptions.ToRoom, "{0} picks up some trash.", npc);
            trash.ChangeContainer(npc);
            return true;
        } 
    }
}
