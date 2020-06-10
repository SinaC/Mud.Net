using System.Linq;
using Mud.POC.Abilities2.Helpers;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class ItemInventorySkillBase : SkillBase
    {
        protected IItem Item { get; set; }

        protected ItemInventorySkillBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(SkillActionInput skillActionInput)
        {
            if (skillActionInput.Parameters.Length < 1)
                return "What should the skill be cast upon?";
            Item = FindHelpers.FindByName(User.Inventory.Where(User.CanSee), skillActionInput.Parameters[0]); // TODO: equipments ?
            if (Item == null)
                return "You are not carrying that.";
            return null;
        }
    }
}
